import React, { useState, useEffect } from 'react';
import { DollarSign, Calendar, User } from 'lucide-react';
import { MembersMembership } from '../../types/api';
import { membersMembershipAPI } from '../../services/api';

const Members: React.FC = () => {
  const [members, setMembers] = useState<MembersMembership[]>([]);
  const [loading, setLoading] = useState(true);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [selectedMember, setSelectedMember] = useState<MembersMembership | null>(null);

  useEffect(() => {
    fetchMembers();
  }, []);

  const fetchMembers = async () => {
    try {
      const data = await membersMembershipAPI.getAll();
      setMembers(data);
    } catch (error) {
      console.error('Error fetching members:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePayment = (member: MembersMembership) => {
    setSelectedMember(member);
    setShowPaymentModal(true);
  };

  const processPayment = async (amount: number) => {
    if (!selectedMember) return;
    
    try {
      await membersMembershipAPI.processPayment(selectedMember.membersMembershipId, amount);
      setShowPaymentModal(false);
      fetchMembers();
    } catch (error) {
      console.error('Error processing payment:', error);
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
                  <th>Total Amount</th>
                  <th>Paid</th>
                  <th>Remaining</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {members.map((member) => (
                  <tr key={member.membersMembershipId}>
                    <td>
                      {member.enquiry?.firstName} {member.enquiry?.lastName}
                    </td>
                    <td>{member.membershipPlan?.planName}</td>
                    <td>{new Date(member.startDate).toLocaleDateString()}</td>
                    <td>${member.totalAmount}</td>
                    <td>${member.paidAmount}</td>
                    <td>${member.remainingAmount}</td>
                    <td>{getStatusBadge(member)}</td>
                    <td>
                      {member.remainingAmount > 0 && (
                        <button 
                          className="btn btn-success"
                          onClick={() => handlePayment(member)}
                          style={{ padding: '0.25rem 0.5rem' }}
                        >
                          <DollarSign size={16} />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
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
                    alert('Please enter a valid payment amount');
                  }
                }}
              >
                Process Payment
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Members;
