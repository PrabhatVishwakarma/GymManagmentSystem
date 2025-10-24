import React, { useState, useEffect } from 'react';
import { Activity, Mail, MessageCircle, TrendingUp, UserCheck, XCircle, CheckCircle, Filter } from 'lucide-react';
import { activityAPI } from '../../services/api';
import Pagination from '../common/Pagination';

interface ActivityEntry {
  activityId: number;
  activityType: string;
  description: string;
  entityType: string;
  entityId: number | null;
  recipientName: string;
  recipientContact: string;
  messageContent: string;
  isSuccessful: boolean;
  errorMessage: string | null;
  performedBy: string;
  createdAt: string;
}

const ActivityLog: React.FC = () => {
  const [activities, setActivities] = useState<ActivityEntry[]>([]);
  const [filteredActivities, setFilteredActivities] = useState<ActivityEntry[]>([]);
  const [paginatedActivities, setPaginatedActivities] = useState<ActivityEntry[]>([]);
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [filterType, setFilterType] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [dateFrom, setDateFrom] = useState<string>('');
  const [dateTo, setDateTo] = useState<string>('');
  const [showSuccessOnly, setShowSuccessOnly] = useState<boolean>(false);
  const [selectedActivity, setSelectedActivity] = useState<ActivityEntry | null>(null);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [itemsPerPage, setItemsPerPage] = useState<number>(25);

  useEffect(() => {
    fetchActivities();
    fetchStats();
  }, []);

  // Apply filters whenever activities, search, or filters change
  useEffect(() => {
    applyFilters();
  }, [activities, filterType, searchQuery, dateFrom, dateTo, showSuccessOnly]);

  // Apply pagination whenever filtered activities or page changes
  useEffect(() => {
    applyPagination();
  }, [filteredActivities, currentPage, itemsPerPage]);

  const fetchActivities = async () => {
    try {
      setLoading(true);
      // Fetch more activities for better filtering
      const data = await activityAPI.getRecent(500);
      setActivities(data);
    } catch (error) {
      console.error('Error fetching activities:', error);
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...activities];

    // Filter by activity type
    if (filterType) {
      filtered = filtered.filter(a => a.activityType === filterType);
    }

    // Filter by search query (searches in multiple fields)
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(a => 
        a.description?.toLowerCase().includes(query) ||
        a.recipientName?.toLowerCase().includes(query) ||
        a.recipientContact?.toLowerCase().includes(query) ||
        a.activityType?.toLowerCase().includes(query) ||
        a.performedBy?.toLowerCase().includes(query) ||
        a.messageContent?.toLowerCase().includes(query)
      );
    }

    // Filter by date range
    if (dateFrom) {
      const fromDate = new Date(dateFrom);
      filtered = filtered.filter(a => new Date(a.createdAt) >= fromDate);
    }
    if (dateTo) {
      const toDate = new Date(dateTo);
      toDate.setHours(23, 59, 59, 999); // End of day
      filtered = filtered.filter(a => new Date(a.createdAt) <= toDate);
    }

    // Filter by success status
    if (showSuccessOnly) {
      filtered = filtered.filter(a => a.isSuccessful);
    }

    setFilteredActivities(filtered);
  };

  const applyPagination = () => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    setPaginatedActivities(filteredActivities.slice(startIndex, endIndex));
  };

  const clearFilters = () => {
    setFilterType('');
    setSearchQuery('');
    setDateFrom('');
    setDateTo('');
    setShowSuccessOnly(false);
    setCurrentPage(1); // Reset to first page
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    // Scroll to top of activity list
    window.scrollTo({ top: 400, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (items: number) => {
    setItemsPerPage(items);
    setCurrentPage(1); // Reset to first page when changing items per page
  };

  const totalPages = Math.ceil(filteredActivities.length / itemsPerPage);

  const fetchStats = async () => {
    try {
      const data = await activityAPI.getStats();
      setStats(data);
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'Email':
        return <Mail size={18} color="#3B82F6" />;
      case 'WhatsApp':
        return <MessageCircle size={18} color="#10B981" />;
      case 'MembershipUpgraded':
        return <TrendingUp size={18} color="#8B5CF6" />;
      case 'MembershipDeactivated':
      case 'MembershipActivated':
        return <UserCheck size={18} color="#F59E0B" />;
      case 'MembershipDeleted':
        return <XCircle size={18} color="#EF4444" />;
      default:
        return <Activity size={18} color="#6B7280" />;
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  };

  const viewDetails = (activity: ActivityEntry) => {
    setSelectedActivity(activity);
    setShowDetailsModal(true);
  };

  if (loading && activities.length === 0) {
    return <div className="loading">Loading activities...</div>;
  }

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ margin: 0, color: '#333', marginBottom: '0.5rem' }}>📋 Activity Log</h1>
        <p style={{ color: '#6B7280', margin: 0 }}>Track all system activities, notifications, and member actions</p>
      </div>

      {/* Stats Cards */}
      {stats && (
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
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
              Today's Activities
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#1F2937' }}>
              {stats.todayActivities}
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
              Emails Sent
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#10B981' }}>
              {stats.emailsSent}
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
              WhatsApp Sent
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#8B5CF6' }}>
              {stats.whatsAppSent}
            </div>
          </div>

          <div style={{
            backgroundColor: 'white',
            padding: '1.5rem',
            borderRadius: '12px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            borderLeft: '4px solid #EF4444'
          }}>
            <div style={{ fontSize: '0.875rem', color: '#6B7280', marginBottom: '0.5rem' }}>
              Failed Actions
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 'bold', color: '#EF4444' }}>
              {stats.failedActivities}
            </div>
          </div>
        </div>
      )}

      {/* Search and Filters Section */}
      <div style={{ 
        backgroundColor: 'white',
        padding: '1.5rem',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        marginBottom: '1.5rem'
      }}>
        <h3 style={{ marginTop: 0, marginBottom: '1rem', color: '#374151' }}>🔍 Search & Filters</h3>
        
        {/* Search Bar */}
        <div style={{ marginBottom: '1rem' }}>
          <input
            type="text"
            placeholder="Search by member name, email, description, action type..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '2px solid #D1D5DB',
              borderRadius: '8px',
              fontSize: '1rem',
              outline: 'none',
              transition: 'border-color 0.2s'
            }}
            onFocus={(e) => e.target.style.borderColor = '#3B82F6'}
            onBlur={(e) => e.target.style.borderColor = '#D1D5DB'}
          />
        </div>

        {/* Date Range Filters */}
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
          gap: '1rem',
          marginBottom: '1rem'
        }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151', fontSize: '0.875rem' }}>
              From Date
            </label>
            <input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #D1D5DB',
                borderRadius: '6px',
                fontSize: '0.875rem'
              }}
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374141', fontSize: '0.875rem' }}>
              To Date
            </label>
            <input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #D1D5DB',
                borderRadius: '6px',
                fontSize: '0.875rem'
              }}
            />
          </div>
          <div style={{ display: 'flex', alignItems: 'flex-end' }}>
            <label style={{ 
              display: 'flex', 
              alignItems: 'center', 
              gap: '0.5rem',
              cursor: 'pointer',
              padding: '0.5rem',
              fontSize: '0.875rem'
            }}>
              <input
                type="checkbox"
                checked={showSuccessOnly}
                onChange={(e) => setShowSuccessOnly(e.target.checked)}
                style={{ width: '18px', height: '18px', cursor: 'pointer' }}
              />
              <span style={{ color: '#374151', fontWeight: '500' }}>Show Successful Only</span>
            </label>
          </div>
        </div>

        {/* Activity Type Filter Buttons */}
        <div style={{ marginBottom: '1rem' }}>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151', fontSize: '0.875rem' }}>
            Activity Type
          </label>
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            {['All', 'Email', 'WhatsApp', 'MembershipUpgraded', 'MembershipDeactivated', 'MembershipActivated', 'MembershipDeleted', 'PaymentReceived'].map((type) => (
              <button
                key={type}
                onClick={() => setFilterType(type === 'All' ? '' : type)}
                style={{
                  padding: '0.5rem 1rem',
                  backgroundColor: (filterType === '' && type === 'All') || filterType === type ? '#3B82F6' : '#E5E7EB',
                  color: (filterType === '' && type === 'All') || filterType === type ? 'white' : '#374151',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontSize: '0.875rem',
                  fontWeight: '500',
                  transition: 'all 0.2s'
                }}
                onMouseEnter={(e) => {
                  if (!((filterType === '' && type === 'All') || filterType === type)) {
                    e.currentTarget.style.backgroundColor = '#D1D5DB';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!((filterType === '' && type === 'All') || filterType === type)) {
                    e.currentTarget.style.backgroundColor = '#E5E7EB';
                  }
                }}
              >
                {type}
              </button>
            ))}
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
            Showing <strong>{filteredActivities.length}</strong> of <strong>{activities.length}</strong> activities
          </span>
          {(filterType || searchQuery || dateFrom || dateTo || showSuccessOnly) && (
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

      {/* Activity Timeline */}
      <div style={{
        backgroundColor: 'white',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        overflow: 'hidden'
      }}>
        {filteredActivities.length === 0 ? (
          <div style={{ padding: '3rem', textAlign: 'center', color: '#6B7280' }}>
            <Activity size={48} color="#D1D5DB" style={{ margin: '0 auto 1rem' }} />
            <div style={{ fontSize: '1.125rem', fontWeight: '500' }}>No activities found</div>
            <div>{activities.length === 0 ? 'No activities recorded yet' : 'Try adjusting your filters or search terms'}</div>
          </div>
        ) : (
          <>
            <div style={{ maxHeight: '600px', overflowY: 'auto' }}>
              {paginatedActivities.map((activity) => (
              <div 
                key={activity.activityId}
                style={{
                  padding: '1rem 1.5rem',
                  borderBottom: '1px solid #E5E7EB',
                  transition: 'background-color 0.2s',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#F9FAFB'}
                onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'white'}
                onClick={() => viewDetails(activity)}
              >
                <div style={{ display: 'flex', alignItems: 'start', gap: '1rem' }}>
                  <div style={{ marginTop: '0.25rem' }}>
                    {getActivityIcon(activity.activityType)}
                  </div>
                  
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
                      <span style={{ fontWeight: '600', color: '#1F2937' }}>
                        {activity.activityType}
                      </span>
                      {activity.isSuccessful ? (
                        <CheckCircle size={16} color="#10B981" />
                      ) : (
                        <XCircle size={16} color="#EF4444" />
                      )}
                    </div>
                    
                    <div style={{ color: '#374151', marginBottom: '0.5rem' }}>
                      {activity.description}
                    </div>
                    
                    {activity.recipientName && (
                      <div style={{ fontSize: '0.875rem', color: '#6B7280' }}>
                        <strong>To:</strong> {activity.recipientName} ({activity.recipientContact})
                      </div>
                    )}
                    
                    {!activity.isSuccessful && activity.errorMessage && (
                      <div style={{ 
                        fontSize: '0.875rem', 
                        color: '#EF4444',
                        backgroundColor: '#FEE2E2',
                        padding: '0.5rem',
                        borderRadius: '6px',
                        marginTop: '0.5rem'
                      }}>
                        <strong>Error:</strong> {activity.errorMessage}
                      </div>
                    )}
                  </div>
                  
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '0.25rem' }}>
                    <span style={{ fontSize: '0.75rem', color: '#9CA3AF' }}>
                      {formatDate(activity.createdAt)}
                    </span>
                    <span style={{ fontSize: '0.75rem', color: '#9CA3AF' }}>
                      By: {activity.performedBy}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
            
            {/* Pagination */}
            {filteredActivities.length > 0 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalItems={filteredActivities.length}
                itemsPerPage={itemsPerPage}
                onPageChange={handlePageChange}
                onItemsPerPageChange={handleItemsPerPageChange}
              />
            )}
          </>
        )}
      </div>

      {/* Details Modal */}
      {showDetailsModal && selectedActivity && (
        <div className="modal-overlay" onClick={() => setShowDetailsModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '600px' }}>
            <div className="modal-header">
              <h2 className="modal-title">Activity Details</h2>
              <button className="modal-close" onClick={() => setShowDetailsModal(false)}>×</button>
            </div>
            
            <div style={{ padding: '1.5rem' }}>
              <div style={{ marginBottom: '1rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                  {getActivityIcon(selectedActivity.activityType)}
                  <strong style={{ fontSize: '1.125rem' }}>{selectedActivity.activityType}</strong>
                  {selectedActivity.isSuccessful ? (
                    <CheckCircle size={20} color="#10B981" />
                  ) : (
                    <XCircle size={20} color="#EF4444" />
                  )}
                </div>
                <div style={{ color: '#6B7280', fontSize: '0.875rem' }}>
                  {new Date(selectedActivity.createdAt).toLocaleString()}
                </div>
              </div>

              <div style={{ backgroundColor: '#F9FAFB', padding: '1rem', borderRadius: '8px', marginBottom: '1rem' }}>
                <div style={{ marginBottom: '0.75rem' }}>
                  <strong>Description:</strong>
                  <div style={{ marginTop: '0.25rem' }}>{selectedActivity.description}</div>
                </div>

                {selectedActivity.recipientName && (
                  <div style={{ marginBottom: '0.75rem' }}>
                    <strong>Recipient:</strong>
                    <div style={{ marginTop: '0.25rem' }}>
                      {selectedActivity.recipientName} - {selectedActivity.recipientContact}
                    </div>
                  </div>
                )}

                <div style={{ marginBottom: '0.75rem' }}>
                  <strong>Performed By:</strong>
                  <div style={{ marginTop: '0.25rem' }}>{selectedActivity.performedBy}</div>
                </div>

                {selectedActivity.entityType && (
                  <div style={{ marginBottom: '0.75rem' }}>
                    <strong>Related To:</strong>
                    <div style={{ marginTop: '0.25rem' }}>
                      {selectedActivity.entityType} 
                      {selectedActivity.entityId && ` (ID: ${selectedActivity.entityId})`}
                    </div>
                  </div>
                )}
              </div>

              {selectedActivity.messageContent && (
                <div>
                  <strong>Message Content:</strong>
                  <div style={{ 
                    marginTop: '0.5rem',
                    padding: '1rem',
                    backgroundColor: '#F3F4F6',
                    borderRadius: '8px',
                    maxHeight: '200px',
                    overflowY: 'auto',
                    whiteSpace: 'pre-wrap',
                    fontSize: '0.875rem',
                    lineHeight: '1.5'
                  }}>
                    {selectedActivity.messageContent}
                  </div>
                </div>
              )}

              {!selectedActivity.isSuccessful && selectedActivity.errorMessage && (
                <div style={{ 
                  marginTop: '1rem',
                  padding: '1rem',
                  backgroundColor: '#FEE2E2',
                  borderRadius: '8px',
                  color: '#DC2626'
                }}>
                  <strong>Error Message:</strong>
                  <div style={{ marginTop: '0.5rem' }}>{selectedActivity.errorMessage}</div>
                </div>
              )}
            </div>

            <div className="form-actions">
              <button 
                type="button" 
                className="btn btn-primary"
                onClick={() => setShowDetailsModal(false)}
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ActivityLog;

