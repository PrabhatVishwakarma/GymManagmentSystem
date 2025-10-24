import React, { useState, useEffect } from 'react';
import { Plus, Edit, Trash2, Eye } from 'lucide-react';
import { MembershipPlan } from '../../types/api';
import { membershipPlanAPI } from '../../services/api';

const MembershipPlans: React.FC = () => {
  const [plans, setPlans] = useState<MembershipPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState<Partial<MembershipPlan>>({});

  useEffect(() => {
    fetchPlans();
  }, []);

  const fetchPlans = async () => {
    try {
      const data = await membershipPlanAPI.getAll();
      setPlans(data);
    } catch (error) {
      console.error('Error fetching plans:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({});
    setShowModal(true);
  };

  const handleEdit = (plan: MembershipPlan) => {
    setFormData(plan);
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (formData.membershipPlanId) {
        await membershipPlanAPI.update(formData.membershipPlanId, formData);
      } else {
        await membershipPlanAPI.create(formData);
      }
      setShowModal(false);
      fetchPlans();
    } catch (error) {
      console.error('Error saving plan:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this plan?')) {
      try {
        await membershipPlanAPI.delete(id);
        fetchPlans();
      } catch (error) {
        console.error('Error deleting plan:', error);
      }
    }
  };

  if (loading) {
    return <div className="loading">Loading membership plans...</div>;
  }

  return (
    <div>
      <div className="header">
        <h1 className="header-title">Membership Plans</h1>
        <button className="btn btn-primary" onClick={handleCreate}>
          <Plus size={20} style={{ marginRight: '0.5rem' }} />
          Add Plan
        </button>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Plan Name</th>
                  <th>Type</th>
                  <th>Duration</th>
                  <th>Price</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {plans.map((plan) => (
                  <tr key={plan.membershipPlanId}>
                    <td>{plan.planName}</td>
                    <td>{plan.planType}</td>
                    <td>{plan.durationInMonths} months</td>
                    <td>${plan.price}</td>
                    <td>
                      <span className={`badge ${plan.isActive ? 'badge-success' : 'badge-danger'}`}>
                        {plan.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button 
                          className="btn btn-secondary"
                          onClick={() => handleEdit(plan)}
                          style={{ padding: '0.25rem 0.5rem' }}
                        >
                          <Edit size={16} />
                        </button>
                        <button 
                          className="btn btn-danger"
                          onClick={() => handleDelete(plan.membershipPlanId)}
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
        </div>
      </div>

      {/* Create/Edit Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">
                {formData.membershipPlanId ? 'Edit Plan' : 'Add Plan'}
              </h2>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            
            <form onSubmit={handleSubmit} className="form">
              <div className="form-group">
                <label className="form-label">Plan Name</label>
                <input
                  type="text"
                  className="form-input"
                  value={formData.planName || ''}
                  onChange={(e) => setFormData({...formData, planName: e.target.value})}
                  required
                />
              </div>
              
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Plan Type</label>
                  <select
                    className="form-input"
                    value={formData.planType || ''}
                    onChange={(e) => setFormData({...formData, planType: e.target.value})}
                    required
                  >
                    <option value="">Select Type</option>
                    <option value="Monthly">Monthly</option>
                    <option value="Quarterly">Quarterly</option>
                    <option value="Yearly">Yearly</option>
                    <option value="Custom">Custom</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Duration (Months)</label>
                  <input
                    type="number"
                    className="form-input"
                    value={formData.durationInMonths || ''}
                    onChange={(e) => setFormData({...formData, durationInMonths: parseInt(e.target.value)})}
                    required
                    min="1"
                    max="24"
                  />
                </div>
              </div>
              
              <div className="form-group">
                <label className="form-label">Price ($)</label>
                <input
                  type="number"
                  className="form-input"
                  value={formData.price || ''}
                  onChange={(e) => setFormData({...formData, price: parseFloat(e.target.value)})}
                  required
                  step="0.01"
                  min="0"
                />
              </div>
              
              <div className="form-group">
                <label className="form-label">Description</label>
                <textarea
                  className="form-input"
                  value={formData.description || ''}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                />
              </div>
              
              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  {formData.membershipPlanId ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default MembershipPlans;
