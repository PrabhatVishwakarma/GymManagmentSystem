import React, { ReactNode } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { 
  LayoutDashboard, 
  Users, 
  CreditCard, 
  UserCheck, 
  Settings,
  LogOut,
  Menu,
  X,
  TrendingUp,
  Activity as ActivityIcon
} from 'lucide-react';
import { useState } from 'react';

interface LayoutProps {
  children: ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { path: '/enquiries', label: 'Enquiries', icon: Users },
    { path: '/membership-plans', label: 'Membership Plans', icon: CreditCard },
    { path: '/members', label: 'Members', icon: UserCheck },
    { path: '/reports', label: 'Sales Reports', icon: TrendingUp },
    { path: '/activity', label: 'Activity Log', icon: ActivityIcon },
    { path: '/users', label: 'Users', icon: Settings },
  ];

  return (
    <div className="layout">
      {/* Mobile Menu Button */}
      <button 
        className="mobile-menu-btn"
        onClick={() => setSidebarOpen(!sidebarOpen)}
        style={{
          position: 'fixed',
          top: '1rem',
          left: '1rem',
          zIndex: 1001,
          background: '#2c3e50',
          color: 'white',
          border: 'none',
          borderRadius: '8px',
          padding: '0.5rem',
          display: 'none'
        }}
      >
        {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
      </button>

      {/* Sidebar */}
      <div className={`sidebar ${sidebarOpen ? 'sidebar-open' : ''}`}>
        <div className="sidebar-header">
          <div className="sidebar-title">🏋️ Gym Management</div>
        </div>
        
        <nav>
          <ul className="sidebar-nav">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path;
              
              return (
                <li key={item.path} className="nav-item">
                  <Link 
                    to={item.path} 
                    className={`nav-link ${isActive ? 'active' : ''}`}
                    onClick={() => setSidebarOpen(false)}
                  >
                    <Icon className="nav-icon" />
                    {item.label}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
        
        <div style={{ marginTop: 'auto', padding: '1rem' }}>
          <div style={{ padding: '0.75rem 1rem', color: '#bdc3c7', fontSize: '0.9rem' }}>
            Logged in as: {user?.firstName} {user?.lastName}
          </div>
          <button 
            onClick={handleLogout}
            className="nav-link"
            style={{ width: '100%', justifyContent: 'flex-start' }}
          >
            <LogOut className="nav-icon" />
            Logout
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="main-content">
        {children}
      </div>
    </div>
  );
};

export default Layout;
