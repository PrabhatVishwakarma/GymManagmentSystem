import React, { useState, useEffect } from 'react';
import { Plus, Edit, Trash2, UserCheck, Download } from 'lucide-react';
import { Enquiry } from '../../types/api';
import { enquiryAPI, membershipPlanAPI } from '../../services/api';
import SearchBar from '../common/SearchBar';
import Pagination from '../common/Pagination';

const Enquiries: React.FC = () => {
  const [openEnquiries, setOpenEnquiries] = useState<Enquiry[]>([]);
  const [closedEnquiries, setClosedEnquiries] = useState<Enquiry[]>([]);
  const [filteredOpenEnquiries, setFilteredOpenEnquiries] = useState<Enquiry[]>([]);
  const [filteredClosedEnquiries, setFilteredClosedEnquiries] = useState<Enquiry[]>([]);
  const [paginatedEnquiries, setPaginatedEnquiries] = useState<Enquiry[]>([]);
  const [membershipPlans, setMembershipPlans] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [showConvertModal, setShowConvertModal] = useState(false);
  const [selectedEnquiry, setSelectedEnquiry] = useState<Enquiry | null>(null);
  const [formData, setFormData] = useState<Partial<Enquiry>>({});
  const [activeTab, setActiveTab] = useState<'open' | 'closed'>('open');
  
  // Search and Pagination state
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [itemsPerPage, setItemsPerPage] = useState<number>(25);

  useEffect(() => {
    fetchData();
  }, []);

  // Apply search filter
  useEffect(() => {
    applySearch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [openEnquiries, closedEnquiries, searchQuery, activeTab]);

  // Apply pagination
  useEffect(() => {
    applyPagination();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filteredOpenEnquiries, filteredClosedEnquiries, activeTab, currentPage, itemsPerPage]);

  const fetchData = async () => {
    try {
      const [openData, closedData, plansData] = await Promise.all([
        enquiryAPI.getOpen(),
        enquiryAPI.getClosed(),
        membershipPlanAPI.getActive()
      ]);
      setOpenEnquiries(openData);
      setClosedEnquiries(closedData);
      setMembershipPlans(plansData);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  const applySearch = () => {
    const filterEnquiries = (enquiries: Enquiry[]) => {
      if (!searchQuery) return enquiries;
      
      const query = searchQuery.toLowerCase();
      return enquiries.filter(e =>
        `${e.firstName} ${e.lastName}`.toLowerCase().includes(query) ||
        e.email?.toLowerCase().includes(query) ||
        e.phone?.toLowerCase().includes(query) ||
        e.address?.toLowerCase().includes(query)
      );
    };

    setFilteredOpenEnquiries(filterEnquiries(openEnquiries));
    setFilteredClosedEnquiries(filterEnquiries(closedEnquiries));
    setCurrentPage(1); // Reset to first page when search changes
  };

  const applyPagination = () => {
    const currentData = activeTab === 'open' ? filteredOpenEnquiries : filteredClosedEnquiries;
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    setPaginatedEnquiries(currentData.slice(startIndex, endIndex));
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (items: number) => {
    setItemsPerPage(items);
    setCurrentPage(1);
  };

  const getCurrentData = () => activeTab === 'open' ? filteredOpenEnquiries : filteredClosedEnquiries;
  const totalPages = Math.ceil(getCurrentData().length / itemsPerPage);

  const handleCreate = () => {
    setFormData({});
    setShowModal(true);
  };

  const handleEdit = (enquiry: Enquiry) => {
    setFormData(enquiry);
    setShowModal(true);
  };

  const handleConvert = (enquiry: Enquiry) => {
    setSelectedEnquiry(enquiry);
    setShowConvertModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (formData.enquiryId) {
        await enquiryAPI.update(formData.enquiryId, formData);
      } else {
        await enquiryAPI.create(formData);
      }
      setShowModal(false);
      fetchData();
    } catch (error) {
      console.error('Error saving enquiry:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this enquiry?')) {
      try {
        await enquiryAPI.delete(id);
        fetchData();
      } catch (error: any) {
        console.error('Error deleting enquiry:', error);
        if (error.response?.data?.message) {
          alert(error.response.data.message);
        } else {
          alert('Failed to delete enquiry');
        }
      }
    }
  };

  const handleConvertToMember = async (membershipPlanId: number, paidAmount: number) => {
    if (!selectedEnquiry) return;
    
    try {
      await enquiryAPI.convertToMember(
        selectedEnquiry.enquiryId, 
        membershipPlanId, 
        paidAmount, 
        'admin@gym.com'
      );
      setShowConvertModal(false);
      fetchData();
      alert('Enquiry successfully converted to member!');
    } catch (error: any) {
      console.error('Error converting to member:', error);
      if (error.response?.data?.message) {
        alert(error.response.data.message);
      } else {
        alert('Failed to convert enquiry to member');
      }
    }
  };

  const handleExportToExcel = async () => {
    try {
      const response = await enquiryAPI.exportToExcel();
      const url = window.URL.createObjectURL(new Blob([response]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Enquiries_${new Date().toISOString().split('T')[0]}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting to Excel:', error);
      alert('Failed to export enquiries to Excel');
    }
  };

  if (loading) {
    return <div className="loading">Loading enquiries...</div>;
  }

  const enquiries = activeTab === 'open' ? openEnquiries : closedEnquiries;

  return (
    <div>
      <div className="header">
        <h1 className="header-title">Enquiries</h1>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn-success" onClick={handleExportToExcel}>
            <Download size={20} style={{ marginRight: '0.5rem' }} />
            Export to Excel
          </button>
          <button className="btn btn-primary" onClick={handleCreate}>
            <Plus size={20} style={{ marginRight: '0.5rem' }} />
            Add Enquiry
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div style={{ marginBottom: '1rem', borderBottom: '2px solid #e0e0e0' }}>
        <div style={{ display: 'flex', gap: '1rem' }}>
          <button
            onClick={() => {
              setActiveTab('open');
              setCurrentPage(1);
            }}
            style={{
              padding: '0.75rem 1.5rem',
              background: activeTab === 'open' ? '#007bff' : 'transparent',
              color: activeTab === 'open' ? 'white' : '#666',
              border: 'none',
              borderBottom: activeTab === 'open' ? '3px solid #007bff' : 'none',
              cursor: 'pointer',
              fontWeight: activeTab === 'open' ? 'bold' : 'normal',
              fontSize: '1rem'
            }}
          >
            Open Enquiries ({filteredOpenEnquiries.length}/{openEnquiries.length})
          </button>
          <button
            onClick={() => {
              setActiveTab('closed');
              setCurrentPage(1);
            }}
            style={{
              padding: '0.75rem 1.5rem',
              background: activeTab === 'closed' ? '#28a745' : 'transparent',
              color: activeTab === 'closed' ? 'white' : '#666',
              border: 'none',
              borderBottom: activeTab === 'closed' ? '3px solid #28a745' : 'none',
              cursor: 'pointer',
              fontWeight: activeTab === 'closed' ? 'bold' : 'normal',
              fontSize: '1rem'
            }}
          >
            Closed Enquiries ({filteredClosedEnquiries.length}/{closedEnquiries.length})
          </button>
        </div>
      </div>

      {/* Search Bar */}
      <div className="card" style={{ marginBottom: '1rem' }}>
        <div className="card-body">
          <SearchBar
            value={searchQuery}
            onChange={setSearchQuery}
            placeholder="Search by name, email, phone, or address..."
            onClear={() => setSearchQuery('')}
          />
          <div style={{ marginTop: '0.5rem', color: '#6B7280', fontSize: '0.875rem' }}>
            Showing <strong>{getCurrentData().length}</strong> {activeTab} enquiries
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>City</th>
                  <th>{activeTab === 'open' ? 'Created' : 'Converted'}</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {paginatedEnquiries.map((enquiry) => (
                  <tr key={enquiry.enquiryId}>
                    <td>{enquiry.firstName} {enquiry.lastName}</td>
                    <td>{enquiry.email}</td>
                    <td>{enquiry.phone}</td>
                    <td>{enquiry.city}</td>
                    <td>
                      {activeTab === 'open' 
                        ? new Date(enquiry.createdAt + (enquiry.createdAt.includes('Z') ? '' : 'Z')).toLocaleDateString()
                        : new Date(enquiry.convertedDate! + (enquiry.convertedDate!.includes('Z') ? '' : 'Z')).toLocaleDateString()}
                    </td>
                    <td>
                      {enquiry.isConverted ? (
                        <span className="badge badge-success">Converted</span>
                      ) : (
                        <span className="badge badge-warning">Open</span>
                      )}
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        {!enquiry.isConverted && (
                          <>
                            <button 
                              className="btn btn-secondary"
                              onClick={() => handleEdit(enquiry)}
                              style={{ padding: '0.25rem 0.5rem' }}
                            >
                              <Edit size={16} />
                            </button>
                            <button 
                              className="btn btn-success"
                              onClick={() => handleConvert(enquiry)}
                              style={{ padding: '0.25rem 0.5rem' }}
                              title="Convert to Member"
                            >
                              <UserCheck size={16} />
                            </button>
                          </>
                        )}
                        <button 
                          className="btn btn-danger"
                          onClick={() => handleDelete(enquiry.enquiryId)}
                          style={{ padding: '0.25rem 0.5rem' }}
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          
          {/* Pagination */}
          {getCurrentData().length > 0 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              totalItems={getCurrentData().length}
              itemsPerPage={itemsPerPage}
              onPageChange={handlePageChange}
              onItemsPerPageChange={handleItemsPerPageChange}
            />
          )}
        </div>
      </div>

      {/* Create/Edit Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">
                {formData.enquiryId ? 'Edit Enquiry' : 'Add Enquiry'}
              </h2>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            
            <form onSubmit={handleSubmit} className="form">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="form-input"
                    value={formData.firstName || ''}
                    onChange={(e) => setFormData({...formData, firstName: e.target.value})}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="form-input"
                    value={formData.lastName || ''}
                    onChange={(e) => setFormData({...formData, lastName: e.target.value})}
                    required
                  />
                </div>
              </div>
              
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Email</label>
                  <input
                    type="email"
                    className="form-input"
                    value={formData.email || ''}
                    onChange={(e) => setFormData({...formData, email: e.target.value})}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Phone</label>
                  <input
                    type="tel"
                    className="form-input"
                    value={formData.phone || ''}
                    onChange={(e) => setFormData({...formData, phone: e.target.value})}
                    required
                  />
                </div>
              </div>
              
              <div className="form-group">
                <label className="form-label">Address</label>
                <input
                  type="text"
                  className="form-input"
                  value={formData.address || ''}
                  onChange={(e) => setFormData({...formData, address: e.target.value})}
                />
              </div>
              
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">City</label>
                  <input
                    type="text"
                    className="form-input"
                    value={formData.city || ''}
                    onChange={(e) => setFormData({...formData, city: e.target.value})}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Gender</label>
                  <select
                    className="form-input"
                    value={formData.gender || ''}
                    onChange={(e) => setFormData({...formData, gender: e.target.value})}
                  >
                    <option value="">Select Gender</option>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              
              <div className="form-group">
                <label className="form-label">Occupation</label>
                <input
                  type="text"
                  className="form-input"
                  value={formData.occupation || ''}
                  onChange={(e) => setFormData({...formData, occupation: e.target.value})}
                />
              </div>
              
              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  {formData.enquiryId ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Convert to Member Modal */}
      {showConvertModal && selectedEnquiry && (
        <div className="modal-overlay" onClick={() => setShowConvertModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Convert to Member</h2>
              <button className="modal-close" onClick={() => setShowConvertModal(false)}>×</button>
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
              <strong>Enquiry:</strong> {selectedEnquiry.firstName} {selectedEnquiry.lastName}
            </div>
            
            <div className="form-group">
              <label className="form-label">Select Membership Plan</label>
              <select id="membershipPlan" className="form-input">
                <option value="">Select Plan</option>
                {membershipPlans.map((plan) => (
                  <option key={plan.membershipPlanId} value={plan.membershipPlanId}>
                    {plan.planName} - ${plan.price} ({plan.durationInMonths} months)
                  </option>
                ))}
              </select>
            </div>
            
            <div className="form-group">
              <label className="form-label">Initial Payment Amount</label>
              <input
                type="number"
                className="form-input"
                id="paidAmount"
                placeholder="0.00"
                step="0.01"
              />
            </div>
            
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={() => setShowConvertModal(false)}>
                Cancel
              </button>
              <button 
                type="button" 
                className="btn btn-success"
                onClick={() => {
                  const planId = parseInt((document.getElementById('membershipPlan') as HTMLSelectElement).value);
                  const amount = parseFloat((document.getElementById('paidAmount') as HTMLInputElement).value) || 0;
                  handleConvertToMember(planId, amount);
                }}
              >
                Convert to Member
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Enquiries;
