import React, { useState, useEffect } from 'react';
import { Users as UsersIcon, UserPlus, Edit, Trash2 } from 'lucide-react';
import { User } from '../../types/api';
import { useAuth } from '../../contexts/AuthContext';
import { userAPI } from '../../services/api';

const Users: React.FC = () => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [availableRoles, setAvailableRoles] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    gender: '',
    address: '',
    dateOfBirth: '',
    occupation: '',
    role: '',
    createdBy: currentUser?.email || 'admin'
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const [usersData, rolesData] = await Promise.all([
        userAPI.getAll(),
        userAPI.getAvailableRoles()
      ]);
      setUsers(usersData);
      setAvailableRoles(rolesData);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      gender: '',
      address: '',
      dateOfBirth: '',
      occupation: '',
      role: '',
      createdBy: currentUser?.email || 'admin'
    });
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await userAPI.register(formData);
      setShowModal(false);
      fetchData();
    } catch (error) {
      console.error('Error creating user:', error);
      alert('Failed to create user. Please check the form and try again.');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await userAPI.delete(id);
        fetchData();
      } catch (error) {
        console.error('Error deleting user:', error);
        alert('Failed to delete user');
      }
    }
  };

  if (loading) {
    return <div className="loading">Loading users...</div>;
  }

  return (
    <div>
      <div className="header">
        <h1 className="header-title">Users</h1>
        <button className="btn btn-primary" onClick={handleCreate}>
          <UserPlus size={20} style={{ marginRight: '0.5rem' }} />
          Add User
        </button>
      </div>

      <div className="card">
        <div className="card-body">
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Gender</th>
                  <th>Occupation</th>
                  <th>Address</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.id}>
                    <td>{user.firstName} {user.lastName}</td>
                    <td>{user.email}</td>
                    <td>{user.gender}</td>
                    <td>{user.occupation}</td>
                    <td>{user.address}</td>
                    <td>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button 
                          className="btn btn-secondary"
                          style={{ padding: '0.25rem 0.5rem' }}
                        >
                          <Edit size={16} />
                        </button>
                        {user.id !== currentUser?.id && (
                          <button 
                            className="btn btn-danger"
                            onClick={() => handleDelete(user.id)}
                            style={{ padding: '0.25rem 0.5rem' }}
                          >
                            <Trash2 size={16} />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Create User Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Add User</h2>
              <button className="modal-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            
            <form onSubmit={handleSubmit} className="form">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="form-input"
                    value={formData.firstName}
                    onChange={(e) => setFormData({...formData, firstName: e.target.value})}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="form-input"
                    value={formData.lastName}
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
                    value={formData.email}
                    onChange={(e) => setFormData({...formData, email: e.target.value})}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Password</label>
                  <input
                    type="password"
                    className="form-input"
                    value={formData.password}
                    onChange={(e) => setFormData({...formData, password: e.target.value})}
                    required
                    minLength={6}
                  />
                </div>
              </div>
              
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Gender</label>
                  <select
                    className="form-input"
                    value={formData.gender}
                    onChange={(e) => setFormData({...formData, gender: e.target.value})}
                    required
                  >
                    <option value="">Select Gender</option>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Role</label>
                  <select
                    className="form-input"
                    value={formData.role}
                    onChange={(e) => setFormData({...formData, role: e.target.value})}
                    required
                  >
                    <option value="">Select Role</option>
                    {availableRoles.map((role) => (
                      <option key={role} value={role}>{role}</option>
                    ))}
                  </select>
                </div>
              </div>
              
              <div className="form-group">
                <label className="form-label">Date of Birth</label>
                <input
                  type="date"
                  className="form-input"
                  value={formData.dateOfBirth}
                  onChange={(e) => setFormData({...formData, dateOfBirth: e.target.value})}
                  required
                />
              </div>
              
              <div className="form-group">
                <label className="form-label">Occupation</label>
                <input
                  type="text"
                  className="form-input"
                  value={formData.occupation}
                  onChange={(e) => setFormData({...formData, occupation: e.target.value})}
                />
              </div>
              
              <div className="form-group">
                <label className="form-label">Address</label>
                <input
                  type="text"
                  className="form-input"
                  value={formData.address}
                  onChange={(e) => setFormData({...formData, address: e.target.value})}
                />
              </div>
              
              <div className="form-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Create User
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Users;
