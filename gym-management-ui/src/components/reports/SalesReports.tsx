import React, { useState, useEffect } from 'react';
import { reportsAPI } from '../../services/api';
import SearchBar from '../common/SearchBar';
import Pagination from '../common/Pagination';

interface SalesReportEntry {
  membershipId?: number;
  date: string;
  memberName: string;
  enquiryName?: string;
  email?: string;
  planName: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
}

const SalesReports: React.FC = () => {
  const [salesData, setSalesData] = useState<SalesReportEntry[]>([]);
  const [filteredSalesData, setFilteredSalesData] = useState<SalesReportEntry[]>([]);
  const [paginatedSalesData, setPaginatedSalesData] = useState<SalesReportEntry[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [filterType, setFilterType] = useState<'last3months' | 'last6months' | 'custom'>('last3months');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  
  // Search and Pagination state
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [itemsPerPage, setItemsPerPage] = useState<number>(50);

  // Summary calculations (with safety checks)
  const totalRevenue = Array.isArray(filteredSalesData) ? filteredSalesData.reduce((sum, entry) => sum + (entry.totalAmount || 0), 0) : 0;
  const totalPaid = Array.isArray(filteredSalesData) ? filteredSalesData.reduce((sum, entry) => sum + (entry.paidAmount || 0), 0) : 0;
  const totalPending = Array.isArray(filteredSalesData) ? filteredSalesData.reduce((sum, entry) => sum + (entry.remainingAmount || 0), 0) : 0;
  const totalMembers = Array.isArray(filteredSalesData) ? filteredSalesData.length : 0;

  useEffect(() => {
    if (filterType !== 'custom') {
      fetchData();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterType]);

  // Apply search filter
  useEffect(() => {
    applySearch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [salesData, searchQuery]);

  // Apply pagination
  useEffect(() => {
    applyPagination();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filteredSalesData, currentPage, itemsPerPage]);

  const fetchData = async () => {
    try {
      setLoading(true);
      setError('');
      
      let response: any;
      
      if (filterType === 'last3months') {
        response = await reportsAPI.getSalesLast3Months();
      } else if (filterType === 'last6months') {
        response = await reportsAPI.getSalesLast6Months();
      } else {
        response = await reportsAPI.getSalesByDateRange(startDate, endDate);
      }
      
      // Backend returns object with 'memberships' array
      if (response && response.memberships && Array.isArray(response.memberships)) {
        setSalesData(response.memberships);
      } else if (Array.isArray(response)) {
        // Fallback if backend returns direct array
        setSalesData(response);
      } else {
        console.warn('API returned unexpected data format:', response);
        setSalesData([]);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch sales data');
      console.error('Error fetching sales data:', err);
      setSalesData([]);
    } finally {
      setLoading(false);
    }
  };

  const applySearch = () => {
    if (!searchQuery) {
      setFilteredSalesData(salesData);
      setCurrentPage(1);
      return;
    }
    
    const query = searchQuery.toLowerCase();
    const filtered = salesData.filter(entry =>
      entry.memberName?.toLowerCase().includes(query) ||
      entry.enquiryName?.toLowerCase().includes(query) ||
      entry.email?.toLowerCase().includes(query) ||
      entry.planName?.toLowerCase().includes(query)
    );
    
    setFilteredSalesData(filtered);
    setCurrentPage(1);
  };

  const applyPagination = () => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    setPaginatedSalesData(filteredSalesData.slice(startIndex, endIndex));
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (items: number) => {
    setItemsPerPage(items);
    setCurrentPage(1);
  };

  const totalPages = Math.ceil(filteredSalesData.length / itemsPerPage);

  const handleCustomDateSearch = () => {
    if (!startDate || !endDate) {
      setError('Please select both start and end dates');
      return;
    }
    if (new Date(startDate) > new Date(endDate)) {
      setError('Start date must be before end date');
      return;
    }
    fetchData();
  };

  const handleExportToExcel = async () => {
    try {
      setLoading(true);
      const blob = await reportsAPI.exportSalesToExcel(
        filterType === 'custom' ? startDate : undefined,
        filterType === 'custom' ? endDate : undefined
      );
      
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Sales_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err: any) {
      setError('Failed to export data to Excel');
      console.error('Export error:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return `$${amount.toFixed(2)}`;
  };

  const formatDate = (dateString: string) => {
    // Ensure proper UTC to local timezone conversion
    return new Date(dateString + (dateString.includes('Z') ? '' : 'Z')).toLocaleDateString();
  };

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: '2rem'
      }}>
        <h1 style={{ margin: 0, color: '#333' }}>📊 Sales Reports</h1>
        <button
          onClick={handleExportToExcel}
          disabled={loading || salesData.length === 0}
          style={{
            padding: '0.75rem 1.5rem',
            backgroundColor: '#10B981',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: salesData.length === 0 ? 'not-allowed' : 'pointer',
            fontWeight: '500',
            fontSize: '0.95rem',
            opacity: salesData.length === 0 ? 0.5 : 1
          }}
        >
          📥 Export to Excel
        </button>
      </div>

      {/* Filter Section */}
      <div style={{ 
        backgroundColor: 'white',
        padding: '1.5rem',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        marginBottom: '2rem'
      }}>
        <h3 style={{ marginTop: 0, color: '#555' }}>Filter Options</h3>
        
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <button
            onClick={() => setFilterType('last3months')}
            style={{
              padding: '0.75rem 1.5rem',
              backgroundColor: filterType === 'last3months' ? '#3B82F6' : '#E5E7EB',
              color: filterType === 'last3months' ? 'white' : '#374151',
              border: 'none',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: '500'
            }}
          >
            Last 3 Months
          </button>
          
          <button
            onClick={() => setFilterType('last6months')}
            style={{
              padding: '0.75rem 1.5rem',
              backgroundColor: filterType === 'last6months' ? '#3B82F6' : '#E5E7EB',
              color: filterType === 'last6months' ? 'white' : '#374151',
              border: 'none',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: '500'
            }}
          >
            Last 6 Months
          </button>
          
          <button
            onClick={() => setFilterType('custom')}
            style={{
              padding: '0.75rem 1.5rem',
              backgroundColor: filterType === 'custom' ? '#3B82F6' : '#E5E7EB',
              color: filterType === 'custom' ? 'white' : '#374151',
              border: 'none',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: '500'
            }}
          >
            Custom Date Range
          </button>
        </div>

        {/* Custom Date Range Inputs */}
        {filterType === 'custom' && (
          <div style={{ 
            display: 'flex', 
            gap: '1rem', 
            alignItems: 'flex-end',
            marginTop: '1rem',
            padding: '1rem',
            backgroundColor: '#F9FAFB',
            borderRadius: '8px'
          }}>
            <div style={{ flex: 1 }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#555' }}>
                Start Date
              </label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.75rem',
                  border: '1px solid #D1D5DB',
                  borderRadius: '8px',
                  fontSize: '1rem'
                }}
              />
            </div>
            
            <div style={{ flex: 1 }}>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#555' }}>
                End Date
              </label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.75rem',
                  border: '1px solid #D1D5DB',
                  borderRadius: '8px',
                  fontSize: '1rem'
                }}
              />
            </div>
            
            <button
              onClick={handleCustomDateSearch}
              style={{
                padding: '0.75rem 1.5rem',
                backgroundColor: '#3B82F6',
                color: 'white',
                border: 'none',
                borderRadius: '8px',
                cursor: 'pointer',
                fontWeight: '500',
                whiteSpace: 'nowrap'
              }}
            >
              🔍 Search
            </button>
          </div>
        )}
      </div>

      {/* Summary Cards */}
      {salesData.length > 0 && (
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
          gap: '1rem',
          marginBottom: '2rem'
        }}>
          <div style={{
            backgroundColor: 'white',
            padding: '1.5rem',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            borderLeft: '4px solid #3B82F6'
          }}>
            <div style={{ fontSize: '0.875rem', color: '#6B7280', marginBottom: '0.5rem' }}>
              Total Revenue
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#1F2937' }}>
              {formatCurrency(totalRevenue)}
            </div>
          </div>

          <div style={{
            backgroundColor: 'white',
            padding: '1.5rem',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            borderLeft: '4px solid #10B981'
          }}>
            <div style={{ fontSize: '0.875rem', color: '#6B7280', marginBottom: '0.5rem' }}>
              Amount Collected
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#10B981' }}>
              {formatCurrency(totalPaid)}
            </div>
          </div>

          <div style={{
            backgroundColor: 'white',
            padding: '1.5rem',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            borderLeft: '4px solid #F59E0B'
          }}>
            <div style={{ fontSize: '0.875rem', color: '#6B7280', marginBottom: '0.5rem' }}>
              Pending Amount
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#F59E0B' }}>
              {formatCurrency(totalPending)}
            </div>
          </div>

          <div style={{
            backgroundColor: 'white',
            padding: '1.5rem',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            borderLeft: '4px solid #8B5CF6'
          }}>
            <div style={{ fontSize: '0.875rem', color: '#6B7280', marginBottom: '0.5rem' }}>
              Total Members
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#8B5CF6' }}>
              {totalMembers}
            </div>
          </div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div style={{
          padding: '1rem',
          backgroundColor: '#FEE2E2',
          color: '#DC2626',
          borderRadius: '8px',
          marginBottom: '1rem'
        }}>
          {error}
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div style={{ textAlign: 'center', padding: '3rem', color: '#6B7280' }}>
          Loading sales data...
        </div>
      )}

      {/* No Data State */}
      {!loading && salesData.length === 0 && !error && (
        <div style={{
          backgroundColor: 'white',
          padding: '3rem',
          borderRadius: '12px',
          textAlign: 'center',
          color: '#6B7280'
        }}>
          <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>📊</div>
          <div style={{ fontSize: '1.25rem', fontWeight: '500', marginBottom: '0.5rem' }}>
            No sales data found
          </div>
          <div>
            {filterType === 'custom' 
              ? 'Try selecting different dates' 
              : 'No memberships created in this period'}
          </div>
        </div>
      )}

      {/* Search Bar */}
      {!loading && salesData.length > 0 && (
        <div style={{ 
          backgroundColor: 'white',
          padding: '1.5rem',
          borderRadius: '12px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
          marginBottom: '1rem'
        }}>
          <SearchBar
            value={searchQuery}
            onChange={setSearchQuery}
            placeholder="Search by member name, email, or plan..."
            onClear={() => setSearchQuery('')}
          />
          <div style={{ marginTop: '0.5rem', color: '#6B7280', fontSize: '0.875rem' }}>
            Showing <strong>{filteredSalesData.length}</strong> of <strong>{salesData.length}</strong> records
          </div>
        </div>
      )}

      {/* Sales Data Table */}
      {!loading && filteredSalesData.length > 0 && (
        <div style={{ 
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
          overflow: 'hidden'
        }}>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ 
              width: '100%', 
              borderCollapse: 'collapse',
              fontSize: '0.95rem'
            }}>
              <thead>
                <tr style={{ backgroundColor: '#F9FAFB' }}>
                  <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Date
                  </th>
                  <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Member Name
                  </th>
                  <th style={{ padding: '1rem', textAlign: 'left', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Plan
                  </th>
                  <th style={{ padding: '1rem', textAlign: 'right', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Total Amount
                  </th>
                  <th style={{ padding: '1rem', textAlign: 'right', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Paid
                  </th>
                  <th style={{ padding: '1rem', textAlign: 'right', fontWeight: '600', color: '#374151', borderBottom: '2px solid #E5E7EB' }}>
                    Pending
                  </th>
                </tr>
              </thead>
              <tbody>
                {paginatedSalesData.map((entry, index) => (
                  <tr 
                    key={index}
                    style={{ 
                      borderBottom: '1px solid #E5E7EB',
                      transition: 'background-color 0.2s'
                    }}
                    onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#F9FAFB'}
                    onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'white'}
                  >
                    <td style={{ padding: '1rem', color: '#374151' }}>
                      {formatDate(entry.date)}
                    </td>
                    <td style={{ padding: '1rem', color: '#374151', fontWeight: '500' }}>
                      {entry.memberName || entry.enquiryName}
                    </td>
                    <td style={{ padding: '1rem', color: '#6B7280' }}>
                      {entry.planName}
                    </td>
                    <td style={{ padding: '1rem', textAlign: 'right', color: '#374151', fontWeight: '500' }}>
                      {formatCurrency(entry.totalAmount)}
                    </td>
                    <td style={{ padding: '1rem', textAlign: 'right', color: '#10B981', fontWeight: '500' }}>
                      {formatCurrency(entry.paidAmount)}
                    </td>
                    <td style={{ padding: '1rem', textAlign: 'right', color: entry.remainingAmount > 0 ? '#F59E0B' : '#6B7280', fontWeight: '500' }}>
                      {formatCurrency(entry.remainingAmount)}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr style={{ backgroundColor: '#F3F4F6', fontWeight: 'bold' }}>
                  <td colSpan={3} style={{ padding: '1rem', color: '#374151' }}>
                    Total ({totalMembers} memberships)
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'right', color: '#374151' }}>
                    {formatCurrency(totalRevenue)}
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'right', color: '#10B981' }}>
                    {formatCurrency(totalPaid)}
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'right', color: '#F59E0B' }}>
                    {formatCurrency(totalPending)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
          
          {/* Pagination */}
          {filteredSalesData.length > 0 && (
            <div style={{ padding: '1rem', backgroundColor: '#F9FAFB' }}>
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalItems={filteredSalesData.length}
                itemsPerPage={itemsPerPage}
                onPageChange={handlePageChange}
                onItemsPerPageChange={handleItemsPerPageChange}
              />
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default SalesReports;

