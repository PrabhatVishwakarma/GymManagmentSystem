import React, { useState, useEffect } from 'react';
import { DollarSign, Download, ArrowUpCircle, ToggleLeft, ToggleRight, Trash2, Filter, Receipt, Eye } from 'lucide-react';
import { MembersMembership, MembershipPlan } from '../../types/api';
import { membersMembershipAPI, membershipPlanAPI, paymentReceiptAPI } from '../../services/api';
import SearchBar from '../common/SearchBar';
import Pagination from '../common/Pagination';

const Members: React.FC = () => {
  const [members, setMembers] = useState<MembersMembership[]>([]);
  const [filteredMembers, setFilteredMembers] = useState<MembersMembership[]>([]);
  const [paginatedMembers, setPaginatedMembers] = useState<MembersMembership[]>([]);
  const [membershipPlans, setMembershipPlans] = useState<MembershipPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [showReceiptsModal, setShowReceiptsModal] = useState(false);
  const [selectedMember, setSelectedMember] = useState<MembersMembership | null>(null);
  const [memberReceipts, setMemberReceipts] = useState<any[]>([]);
  
  // Search and Filter state
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [activeTab, setActiveTab] = useState<string>('all'); // all, active, expiringSoon, expired, pendingPayments
  const [statusFilter, setStatusFilter] = useState<string>('all'); // all, active, inactive, expired
  const [paymentFilter, setPaymentFilter] = useState<string>('all'); // all, paid, pending
  const [planFilter, setPlanFilter] = useState<string>('all'); // all, or plan ID
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [itemsPerPage, setItemsPerPage] = useState<number>(25);

  useEffect(() => {
    fetchMembers();
    fetchMembershipPlans();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  // Apply filters and search
  useEffect(() => {
    applyFilters();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [members, searchQuery, activeTab, statusFilter, paymentFilter, planFilter]);

  // Apply pagination
  useEffect(() => {
    applyPagination();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filteredMembers, currentPage, itemsPerPage]);

  const fetchMembers = async () => {
    try {
      let data;
      // Fetch based on active tab
      switch (activeTab) {
        case 'active':
          data = await membersMembershipAPI.getActive();
          break;
        case 'expiringSoon':
          data = await membersMembershipAPI.getExpiringSoon();
          break;
        case 'expired':
          data = await membersMembershipAPI.getExpired();
          break;
        case 'pendingPayments':
          data = await membersMembershipAPI.getPendingPayments();
          break;
        default:
          data = await membersMembershipAPI.getAll();
      }
      setMembers(data);
    } catch (error) {
      console.error('Error fetching members:', error);
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...members];

    // Search filter
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(m =>
        `${m.enquiry?.firstName} ${m.enquiry?.lastName}`.toLowerCase().includes(query) ||
        m.enquiry?.email?.toLowerCase().includes(query) ||
        m.enquiry?.phone?.toLowerCase().includes(query) ||
        m.membershipPlan?.planName?.toLowerCase().includes(query)
      );
    }

    // Status filter
    if (statusFilter !== 'all') {
      if (statusFilter === 'active') {
        filtered = filtered.filter(m => m.isActive);
      } else if (statusFilter === 'inactive') {
        filtered = filtered.filter(m => !m.isActive);
      } else if (statusFilter === 'expired') {
        const now = new Date();
        filtered = filtered.filter(m => {
          const endDate = new Date(m.startDate);
          endDate.setMonth(endDate.getMonth() + (m.membershipPlan?.durationInMonths || 0));
          return endDate < now && !m.isInactive;
        });
      }
    }

    // Payment filter
    if (paymentFilter !== 'all') {
      if (paymentFilter === 'paid') {
        filtered = filtered.filter(m => m.remainingAmount <= 0);
      } else if (paymentFilter === 'pending') {
        filtered = filtered.filter(m => m.remainingAmount > 0);
      }
    }

    // Plan filter
    if (planFilter !== 'all') {
      filtered = filtered.filter(m => m.membershipPlanId.toString() === planFilter);
    }

    setFilteredMembers(filtered);
    setCurrentPage(1); // Reset to first page when filters change
  };

  const applyPagination = () => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    setPaginatedMembers(filteredMembers.slice(startIndex, endIndex));
  };

  const clearFilters = () => {
    setSearchQuery('');
    setStatusFilter('all');
    setPaymentFilter('all');
    setPlanFilter('all');
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (items: number) => {
    setItemsPerPage(items);
    setCurrentPage(1);
  };

  const totalPages = Math.ceil(filteredMembers.length / itemsPerPage);

  const fetchMembershipPlans = async () => {
    try {
      const data = await membershipPlanAPI.getActive();
      setMembershipPlans(data);
    } catch (error) {
      console.error('Error fetching membership plans:', error);
    }
  };

  const handlePayment = (member: MembersMembership) => {
    setSelectedMember(member);
    setShowPaymentModal(true);
  };

  const handleUpgrade = (member: MembersMembership) => {
    setSelectedMember(member);
    setShowUpgradeModal(true);
  };

  const handleToggleStatus = async (member: MembersMembership) => {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const userName = `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Admin';
    const newStatus = !member.isActive;
    
    if (window.confirm(`Are you sure you want to ${newStatus ? 'activate' : 'deactivate'} this membership?`)) {
      try {
        await membersMembershipAPI.toggleStatus(member.membersMembershipId, !newStatus, userName);
        window.alert(`Membership ${newStatus ? 'activated' : 'deactivated'} successfully`);
        fetchMembers();
      } catch (error: any) {
        window.alert(error.response?.data?.message || 'Failed to toggle membership status');
        console.error('Error toggling status:', error);
      }
    }
  };

  const handleDelete = async (member: MembersMembership) => {
    const memberName = `${member.enquiry?.firstName} ${member.enquiry?.lastName}`;
    
    if (window.confirm(`Are you sure you want to DELETE the membership for ${memberName}?\n\nThis action cannot be undone!`)) {
      try {
        await membersMembershipAPI.delete(member.membersMembershipId);
        window.alert('Membership deleted successfully');
        fetchMembers();
      } catch (error: any) {
        window.alert(error.response?.data?.message || 'Failed to delete membership');
        console.error('Error deleting membership:', error);
      }
    }
  };

  const processPayment = async (amount: number) => {
    if (!selectedMember) return;
    
    try {
      await membersMembershipAPI.processPayment(selectedMember.membersMembershipId, amount);
      setShowPaymentModal(false);
      fetchMembers();
      window.alert('Payment processed successfully! Receipt has been sent via email.');
    } catch (error) {
      console.error('Error processing payment:', error);
      window.alert('Failed to process payment');
    }
  };

  const handleViewReceipts = async (member: MembersMembership) => {
    setSelectedMember(member);
    try {
      const receipts = await paymentReceiptAPI.getByMember(member.membersMembershipId);
      setMemberReceipts(receipts);
      setShowReceiptsModal(true);
    } catch (error) {
      console.error('Error fetching receipts:', error);
      window.alert('Failed to load receipts');
    }
  };

  const handleDownloadReceipt = async (receiptId: number, receiptNumber: string) => {
    try {
      const blob = await paymentReceiptAPI.downloadReceipt(receiptId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Receipt_${receiptNumber}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading receipt:', error);
      window.alert('Failed to download receipt');
    }
  };

  const handleViewReceiptInBrowser = async (receiptId: number) => {
    try {
      const htmlContent = await paymentReceiptAPI.viewReceiptHtml(receiptId);
      
      // Create a new window and write the HTML content to it
      const newWindow = window.open('', '_blank');
      if (newWindow) {
        newWindow.document.write(htmlContent);
        newWindow.document.close();
      } else {
        window.alert('Please allow pop-ups to view the receipt');
      }
    } catch (error: any) {
      console.error('Error viewing receipt:', error);
      window.alert('Failed to load receipt. Please try again.');
    }
  };

  const processUpgrade = async (newPlanId: number, paidAmount: number) => {
    if (!selectedMember) return;
    
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const userName = `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Admin';
    
    try {
      await membersMembershipAPI.upgrade(selectedMember.membersMembershipId, newPlanId, paidAmount, userName);
      window.alert('Membership upgraded successfully!');
      setShowUpgradeModal(false);
      fetchMembers();
    } catch (error: any) {
      window.alert(error.response?.data?.message || 'Failed to upgrade membership');
      console.error('Error upgrading membership:', error);
    }
  };

  const handleExportToExcel = async () => {
    try {
      const response = await membersMembershipAPI.exportToExcel();
      const url = window.URL.createObjectURL(new Blob([response]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Members_${new Date().toISOString().split('T')[0]}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting to Excel:', error);
      window.alert('Failed to export members to Excel');
    }
  };

  const getStatusBadge = (member: MembersMembership) => {
    if (member.remainingAmount <= 0) {
      return <span className="badge badge-success">Paid</span>;
    } else if (member.isActive) {
      return <span className="badge badge-warning">Active</span>;
    } else {
      return <span className="badge badge-danger">Expired</span>;
    }
  };

  if (loading) {
    return <div className="loading">Loading members...</div>;
  }

  return (
    <div>
      <div className="header">
        <h1 className="header-title">Members</h1>
        <button className="btn btn-success" onClick={handleExportToExcel}>
          <Download size={20} style={{ marginRight: '0.5rem' }} />
          Export to Excel
        </button>
      </div>

      {/* Tabs */}
      <div style={{ 
        borderBottom: '2px solid #E5E7EB', 
        marginBottom: '1.5rem',
        display: 'flex',
        gap: '2rem',
        backgroundColor: 'white',
        padding: '0 1rem'
      }}>
        {[
          { id: 'all', label: 'All Members' },
          { id: 'active', label: 'Active' },
          { id: 'expiringSoon', label: 'Expiring Soon' },
          { id: 'expired', label: 'Expired' },
          { id: 'pendingPayments', label: 'Pending Payments' },
        ].map((tab) => (
          <button
            key={tab.id}
            onClick={() => {
              setActiveTab(tab.id);
              setCurrentPage(1);
            }}
            style={{
              padding: '0.75rem 1rem',
              backgroundColor: 'transparent',
              border: 'none',
              borderBottom: activeTab === tab.id ? '2px solid #4F46E5' : '2px solid transparent',
              color: activeTab === tab.id ? '#4F46E5' : '#6B7280',
              fontWeight: activeTab === tab.id ? '600' : '500',
              cursor: 'pointer',
              transition: 'all 0.2s',
              fontSize: '0.875rem',
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Search and Filters */}
      <div className="card" style={{ marginBottom: '1rem' }}>
        <div className="card-body">
          <h3 style={{ marginTop: 0, marginBottom: '1rem', color: '#374151', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Filter size={20} />
            Search & Filters
          </h3>

          {/* Search Bar */}
          <div style={{ marginBottom: '1rem' }}>
            <SearchBar
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search by name, email, phone, or plan..."
              onClear={() => setSearchQuery('')}
            />
          </div>

          {/* Filter Options */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
            {/* Status Filter */}
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', fontSize: '0.875rem', color: '#374151' }}>
                Status
              </label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.5rem',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  fontSize: '0.875rem',
                  cursor: 'pointer'
                }}
              >
                <option value="all">All Status</option>
                <option value="active">Active</option>
                <option value="inactive">Inactive</option>
                <option value="expired">Expired</option>
              </select>
            </div>

            {/* Payment Filter */}
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', fontSize: '0.875rem', color: '#374151' }}>
                Payment Status
              </label>
              <select
                value={paymentFilter}
                onChange={(e) => setPaymentFilter(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.5rem',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  fontSize: '0.875rem',
                  cursor: 'pointer'
                }}
              >
                <option value="all">All Payments</option>
                <option value="paid">Fully Paid</option>
                <option value="pending">Payment Pending</option>
              </select>
            </div>

            {/* Plan Filter */}
            <div>
              <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', fontSize: '0.875rem', color: '#374151' }}>
                Membership Plan
              </label>
              <select
                value={planFilter}
                onChange={(e) => setPlanFilter(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.5rem',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  fontSize: '0.875rem',
                  cursor: 'pointer'
                }}
              >
                <option value="all">All Plans</option>
                {membershipPlans.map((plan) => (
                  <option key={plan.membershipPlanId} value={plan.membershipPlanId}>
                    {plan.planName}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Results Summary and Clear Button */}
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            paddingTop: '1rem',
            borderTop: '1px solid #E5E7EB'
          }}>
            <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>
              Showing <strong>{filteredMembers.length}</strong> of <strong>{members.length}</strong> members
            </span>
            {(searchQuery || statusFilter !== 'all' || paymentFilter !== 'all' || planFilter !== 'all') && (
              <button
                onClick={clearFilters}
                style={{
                  padding: '0.5rem 1rem',
                  backgroundColor: '#EF4444',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontSize: '0.875rem',
                  fontWeight: '500'
                }}
              >
                Clear All Filters
              </button>
            )}
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Member</th>
                  <th>Plan</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Total Amount</th>
                  <th>Paid</th>
                  <th>Remaining</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {paginatedMembers.map((member) => {
                  const endDate = new Date(member.startDate);
                  endDate.setMonth(endDate.getMonth() + (member.membershipPlan?.durationInMonths || 0));
                  
                  return (
                    <tr key={member.membersMembershipId}>
                      <td>
                        {member.enquiry?.firstName} {member.enquiry?.lastName}
                      </td>
                      <td>{member.membershipPlan?.planName}</td>
                      <td>{new Date(member.startDate).toLocaleDateString()}</td>
                      <td>{endDate.toLocaleDateString()}</td>
                      <td>${member.totalAmount}</td>
                      <td>${member.paidAmount}</td>
                      <td>${member.remainingAmount}</td>
                      <td>{getStatusBadge(member)}</td>
                      <td>
                        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                          {member.remainingAmount > 0 && (
                            <button 
                              className="btn btn-success"
                              onClick={() => handlePayment(member)}
                              style={{ padding: '0.25rem 0.5rem' }}
                              title="Process Payment"
                            >
                              <DollarSign size={16} />
                            </button>
                          )}
                          <button 
                            className="btn btn-info"
                            onClick={() => handleViewReceipts(member)}
                            style={{ padding: '0.25rem 0.5rem' }}
                            title="View Payment Receipts"
                          >
                            <Receipt size={16} />
                          </button>
                          <button 
                            className="btn btn-primary"
                            onClick={() => handleUpgrade(member)}
                            style={{ padding: '0.25rem 0.5rem' }}
                            title="Upgrade Membership"
                          >
                            <ArrowUpCircle size={16} />
                          </button>
                          <button
                            className={member.isActive ? "btn btn-warning" : "btn btn-info"}
                            onClick={() => handleToggleStatus(member)}
                            style={{ padding: '0.25rem 0.5rem' }}
                            title={member.isActive ? "Deactivate" : "Activate"}
                          >
                            {member.isActive ? <ToggleRight size={16} /> : <ToggleLeft size={16} />}
                          </button>
                          <button 
                            className="btn btn-danger"
                            onClick={() => handleDelete(member)}
                            style={{ padding: '0.25rem 0.5rem' }}
                            title="Delete Membership"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          
          {/* Pagination */}
          {filteredMembers.length > 0 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              totalItems={filteredMembers.length}
              itemsPerPage={itemsPerPage}
              onPageChange={handlePageChange}
              onItemsPerPageChange={handleItemsPerPageChange}
            />
          )}
        </div>
      </div>

      {/* Payment Modal */}
      {showPaymentModal && selectedMember && (
        <div className="modal-overlay" onClick={() => setShowPaymentModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Process Payment</h2>
              <button className="modal-close" onClick={() => setShowPaymentModal(false)}>×</button>
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
              <strong>Member:</strong> {selectedMember.enquiry?.firstName} {selectedMember.enquiry?.lastName}<br />
              <strong>Plan:</strong> {selectedMember.membershipPlan?.planName}<br />
              <strong>Remaining Amount:</strong> ${selectedMember.remainingAmount}
            </div>
            
            <div className="form-group">
              <label className="form-label">Payment Amount</label>
              <input
                type="number"
                className="form-input"
                id="paymentAmount"
                placeholder="0.00"
                step="0.01"
                max={selectedMember.remainingAmount}
              />
            </div>
            
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={() => setShowPaymentModal(false)}>
                Cancel
              </button>
              <button 
                type="button" 
                className="btn btn-success"
                onClick={() => {
                  const amount = parseFloat((document.getElementById('paymentAmount') as HTMLInputElement).value) || 0;
                  if (amount > 0 && amount <= selectedMember.remainingAmount) {
                    processPayment(amount);
                  } else {
                    window.alert('Please enter a valid payment amount');
                  }
                }}
              >
                Process Payment
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Upgrade Modal */}
      {showUpgradeModal && selectedMember && (
        <div className="modal-overlay" onClick={() => setShowUpgradeModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Upgrade Membership</h2>
              <button className="modal-close" onClick={() => setShowUpgradeModal(false)}>×</button>
            </div>
            
            <div style={{ marginBottom: '1rem' }}>
              <strong>Member:</strong> {selectedMember.enquiry?.firstName} {selectedMember.enquiry?.lastName}<br />
              <strong>Current Plan:</strong> {selectedMember.membershipPlan?.planName}<br />
              <strong>Current Amount:</strong> ${selectedMember.totalAmount}
            </div>
            
            <div className="form-group">
              <label className="form-label">New Membership Plan</label>
              <select
                className="form-input"
                id="newPlanId"
              >
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
                id="upgradePaidAmount"
                placeholder="0.00"
                step="0.01"
              />
            </div>
            
            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={() => setShowUpgradeModal(false)}>
                Cancel
              </button>
              <button 
                type="button" 
                className="btn btn-primary"
                onClick={() => {
                  const planId = parseInt((document.getElementById('newPlanId') as HTMLSelectElement).value);
                  const paidAmount = parseFloat((document.getElementById('upgradePaidAmount') as HTMLInputElement).value) || 0;
                  
                  if (!planId) {
                    window.alert('Please select a membership plan');
                    return;
                  }
                  
                  if (paidAmount < 0) {
                    window.alert('Payment amount cannot be negative');
                    return;
                  }
                  
                  processUpgrade(planId, paidAmount);
                }}
              >
                Upgrade Membership
              </button>
            </div>
          </div>
        </div>
      )}
      {/* Receipts Modal */}
      {showReceiptsModal && selectedMember && (
        <div className="modal-overlay" onClick={() => setShowReceiptsModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '800px' }}>
            <div className="modal-header">
              <h2 className="modal-title">
                Payment Receipts - {selectedMember.enquiry?.firstName} {selectedMember.enquiry?.lastName}
              </h2>
              <button className="modal-close" onClick={() => setShowReceiptsModal(false)}>×</button>
            </div>
            
            <div style={{ padding: '1.5rem' }}>
              {memberReceipts.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '2rem', color: '#666' }}>
                  <Receipt size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
                  <p>No payment receipts found for this member.</p>
                </div>
              ) : (
                <div>
                  <p style={{ marginBottom: '1rem', color: '#666' }}>
                    Total Receipts: <strong>{memberReceipts.length}</strong>
                  </p>
                  
                  <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
                    <table className="table" style={{ marginBottom: 0 }}>
                      <thead>
                        <tr>
                          <th>Receipt #</th>
                          <th>Date</th>
                          <th>Amount Paid</th>
                          <th>Method</th>
                          <th>Email Sent</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {memberReceipts.map((receipt) => (
                          <tr key={receipt.paymentReceiptId}>
                            <td><strong>{receipt.receiptNumber}</strong></td>
                            <td>{new Date(receipt.paymentDate + (receipt.paymentDate.includes('Z') ? '' : 'Z')).toLocaleString()}</td>
                            <td><strong style={{ color: '#28a745' }}>${receipt.amountPaid.toFixed(2)}</strong></td>
                            <td>{receipt.paymentMethod || 'Cash'}</td>
                            <td>
                              {receipt.emailSent ? (
                                <span style={{ color: '#28a745' }}>✓ Sent</span>
                              ) : (
                                <span style={{ color: '#dc3545' }}>✗ Not Sent</span>
                              )}
                            </td>
                            <td>
                              <div style={{ display: 'flex', gap: '0.5rem' }}>
                                <button
                                  className="btn btn-primary"
                                  onClick={() => handleViewReceiptInBrowser(receipt.paymentReceiptId)}
                                  style={{ padding: '0.25rem 0.5rem', fontSize: '0.875rem' }}
                                  title="View Receipt"
                                >
                                  <Eye size={14} />
                                </button>
                                <button
                                  className="btn btn-success"
                                  onClick={() => handleDownloadReceipt(receipt.paymentReceiptId, receipt.receiptNumber)}
                                  style={{ padding: '0.25rem 0.5rem', fontSize: '0.875rem' }}
                                  title="Download Receipt"
                                >
                                  <Download size={14} />
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  
                  <div style={{ 
                    marginTop: '1.5rem', 
                    padding: '1rem', 
                    backgroundColor: '#f8f9fa',
                    borderRadius: '6px'
                  }}>
                    <h4 style={{ marginTop: 0, marginBottom: '0.5rem' }}>Payment Summary</h4>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
                      <div>
                        <small style={{ color: '#666' }}>Total Paid</small>
                        <div style={{ fontSize: '1.25rem', fontWeight: 'bold', color: '#28a745' }}>
                          ${memberReceipts.reduce((sum, r) => sum + r.amountPaid, 0).toFixed(2)}
                        </div>
                      </div>
                      <div>
                        <small style={{ color: '#666' }}>Total Amount</small>
                        <div style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>
                          ${selectedMember.totalAmount.toFixed(2)}
                        </div>
                      </div>
                      <div>
                        <small style={{ color: '#666' }}>Remaining</small>
                        <div style={{ fontSize: '1.25rem', fontWeight: 'bold', color: '#dc3545' }}>
                          ${selectedMember.remainingAmount.toFixed(2)}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Members;
