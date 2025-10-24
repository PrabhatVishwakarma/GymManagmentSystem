import React, { useState, useEffect } from 'react';
import { Users, CreditCard, UserCheck, TrendingUp } from 'lucide-react';
import { enquiryAPI, membershipPlanAPI, membersMembershipAPI } from '../../services/api';

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState({
    totalEnquiries: 0,
    activeMemberships: 0,
    totalMembers: 0,
    totalRevenue: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const [enquiries, memberships, plans] = await Promise.all([
          enquiryAPI.getAll(),
          membersMembershipAPI.getAll(),
          membershipPlanAPI.getAll()
        ]);

        const totalRevenue = memberships.reduce((sum, membership) => sum + membership.paidAmount, 0);

        setStats({
          totalEnquiries: enquiries.length,
          activeMemberships: memberships.filter(m => m.isActive).length,
          totalMembers: memberships.length,
          totalRevenue: totalRevenue
        });
      } catch (error) {
        console.error('Error fetching stats:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  const statCards = [
    {
      title: 'Total Enquiries',
      value: stats.totalEnquiries,
      icon: Users,
      color: '#667eea'
    },
    {
      title: 'Active Memberships',
      value: stats.activeMemberships,
      icon: UserCheck,
      color: '#28a745'
    },
    {
      title: 'Total Members',
      value: stats.totalMembers,
      icon: CreditCard,
      color: '#17a2b8'
    },
    {
      title: 'Total Revenue',
      value: `$${stats.totalRevenue.toFixed(2)}`,
      icon: TrendingUp,
      color: '#ffc107'
    }
  ];

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  return (
    <div>
      <div className="header">
        <h1 className="header-title">Dashboard</h1>
      </div>

      <div className="stats-grid">
        {statCards.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <div key={index} className="stat-card">
              <div className="stat-number" style={{ color: stat.color }}>
                {stat.value}
              </div>
              <div className="stat-label">{stat.title}</div>
              <Icon size={24} style={{ color: stat.color, marginTop: '0.5rem' }} />
            </div>
          );
        })}
      </div>

      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Welcome to Gym Management System</h2>
        </div>
        <div className="card-body">
          <p style={{ marginBottom: '1rem', color: '#666' }}>
            Manage your gym operations efficiently with our comprehensive management system.
          </p>
          
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem' }}>
            <div style={{ padding: '1rem', background: '#f8f9fa', borderRadius: '8px' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '0.5rem', color: '#333' }}>📋 Enquiries</h3>
              <p style={{ color: '#666', fontSize: '0.9rem' }}>
                Track potential members from initial inquiry to membership conversion.
              </p>
            </div>
            
            <div style={{ padding: '1rem', background: '#f8f9fa', borderRadius: '8px' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '0.5rem', color: '#333' }}>💳 Membership Plans</h3>
              <p style={{ color: '#666', fontSize: '0.9rem' }}>
                Create and manage different gym membership options and pricing.
              </p>
            </div>
            
            <div style={{ padding: '1rem', background: '#f8f9fa', borderRadius: '8px' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '0.5rem', color: '#333' }}>👥 Members</h3>
              <p style={{ color: '#666', fontSize: '0.9rem' }}>
                Manage active members, track payments, and monitor membership status.
              </p>
            </div>
            
            <div style={{ padding: '1rem', background: '#f8f9fa', borderRadius: '8px' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '0.5rem', color: '#333' }}>⚙️ Users</h3>
              <p style={{ color: '#666', fontSize: '0.9rem' }}>
                Manage gym staff accounts and user permissions.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
