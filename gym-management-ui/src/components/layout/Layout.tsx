import React, { ReactNode, useState, useRef, useEffect } from 'react';
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
  Activity as ActivityIcon,
  User as UserIcon,
  ChevronDown
} from 'lucide-react';

interface LayoutProps {
  children: ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [profilePhoto, setProfilePhoto] = useState<string>('');
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Load profile photo
  useEffect(() => {
    const loadProfilePhoto = async () => {
      if (user?.id) {
        try {
          // Try to get from user object first (if it includes profilePhotoUrl)
          if ((user as any).profilePhotoUrl) {
            setProfilePhoto((user as any).profilePhotoUrl);
            localStorage.setItem(`profilePhoto_${user.id}`, (user as any).profilePhotoUrl);
          } else {
            // Fallback to localStorage
            const savedPhoto = localStorage.getItem(`profilePhoto_${user.id}`);
            if (savedPhoto) {
              setProfilePhoto(savedPhoto);
            }
          }
        } catch (error) {
          console.error('Error loading profile photo:', error);
        }
      }
    };

    loadProfilePhoto();

    // Listen for photo updates from Profile component
    const handlePhotoUpdate = (e: any) => {
      console.log('Photo update event received:', e.detail);
      if (e.detail && user?.id && e.detail.userId === user.id) {
        setProfilePhoto(e.detail.photoUrl);
        if (e.detail.photoUrl) {
          localStorage.setItem(`profilePhoto_${user.id}`, e.detail.photoUrl);
        }
      }
    };

    // Listen for storage changes (for cross-tab updates)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === `profilePhoto_${user?.id}`) {
        setProfilePhoto(e.newValue || '');
      }
    };

    window.addEventListener('profilePhotoUpdated', handlePhotoUpdate);
    window.addEventListener('storage', handleStorageChange);
    
    return () => {
      window.removeEventListener('profilePhotoUpdated', handlePhotoUpdate);
      window.removeEventListener('storage', handleStorageChange);
    };
  }, [user]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const getInitials = () => {
    if (!user) return '';
    const firstInitial = user.firstName?.[0] || '';
    const lastInitial = user.lastName?.[0] || '';
    return `${firstInitial}${lastInitial}`.toUpperCase();
  };

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
      {/* Mobile Backdrop Overlay */}
      {sidebarOpen && (
        <div 
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            zIndex: 150,
            display: 'none'
          }}
          className="mobile-backdrop"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Top Header Bar */}
      <div className="top-header" style={{
        position: 'fixed',
        top: '0',
        left: '250px',
        right: '0',
        height: '64px',
        backgroundColor: 'white',
        borderBottom: '1px solid #E5E7EB',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 2rem',
        zIndex: '100',
        boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
        marginLeft: '0'
      }}>
        {/* Mobile Menu Button */}
        <button 
          className="mobile-menu-btn"
          onClick={() => setSidebarOpen(!sidebarOpen)}
          style={{
            background: '#4F46E5',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            padding: '0.5rem',
            display: 'none',
            cursor: 'pointer'
          }}
        >
          {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
        </button>

        {/* Page Title (optional) */}
        <div style={{ flex: 1 }}></div>

        {/* Profile Dropdown */}
        <div style={{ position: 'relative' }} ref={dropdownRef}>
          <button
            onClick={() => setDropdownOpen(!dropdownOpen)}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem',
              padding: '0.5rem 1rem',
              backgroundColor: 'transparent',
              border: 'none',
              borderRadius: '8px',
              cursor: 'pointer',
              transition: 'background-color 0.2s'
            }}
            onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#F3F4F6'}
            onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
          >
            {/* Profile Photo */}
            <div style={{
              width: '40px',
              height: '40px',
              borderRadius: '50%',
              overflow: 'hidden',
              backgroundColor: '#4F46E5',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontSize: '0.875rem',
              fontWeight: 'bold',
              border: '2px solid #E5E7EB'
            }}>
              {profilePhoto ? (
                <img 
                  src={profilePhoto} 
                  alt="Profile" 
                  style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                />
              ) : (
                <span>{getInitials()}</span>
              )}
            </div>

            {/* User Name */}
            <div style={{ textAlign: 'left' }}>
              <div style={{ 
                fontWeight: '600', 
                color: '#111827',
                fontSize: '0.875rem'
              }}>
                {user?.firstName} {user?.lastName}
              </div>
            </div>

            {/* Dropdown Icon */}
            <ChevronDown 
              size={16} 
              style={{ 
                color: '#6B7280',
                transform: dropdownOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                transition: 'transform 0.2s'
              }} 
            />
          </button>

          {/* Dropdown Menu */}
          {dropdownOpen && (
            <div style={{
              position: 'absolute',
              top: '100%',
              right: 0,
              marginTop: '0.5rem',
              width: '220px',
              backgroundColor: 'white',
              borderRadius: '8px',
              boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
              border: '1px solid #E5E7EB',
              overflow: 'hidden',
              zIndex: 1000
            }}>
              {/* User Info Header */}
              <div style={{
                padding: '1rem',
                borderBottom: '1px solid #E5E7EB',
                backgroundColor: '#F9FAFB'
              }}>
                <div style={{ fontWeight: '600', color: '#111827', fontSize: '0.875rem' }}>
                  {user?.firstName} {user?.lastName}
                </div>
                <div style={{ color: '#6B7280', fontSize: '0.75rem', marginTop: '0.25rem' }}>
                  {user?.email}
                </div>
              </div>

              {/* Menu Items */}
              <div style={{ padding: '0.5rem' }}>
                <Link
                  to="/profile"
                  onClick={() => setDropdownOpen(false)}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.75rem',
                    padding: '0.75rem 1rem',
                    color: '#374151',
                    textDecoration: 'none',
                    borderRadius: '6px',
                    fontSize: '0.875rem',
                    fontWeight: '500',
                    transition: 'background-color 0.2s'
                  }}
                  onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#F3F4F6'}
                  onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                >
                  <UserIcon size={18} />
                  My Profile
                </Link>

                <button
                  onClick={() => {
                    setDropdownOpen(false);
                    handleLogout();
                  }}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.75rem',
                    padding: '0.75rem 1rem',
                    width: '100%',
                    backgroundColor: 'transparent',
                    border: 'none',
                    color: '#DC2626',
                    textAlign: 'left',
                    borderRadius: '6px',
                    fontSize: '0.875rem',
                    fontWeight: '500',
                    cursor: 'pointer',
                    transition: 'background-color 0.2s'
                  }}
                  onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#FEE2E2'}
                  onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                >
                  <LogOut size={18} />
                  Logout
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

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
        
      </div>

      {/* Main Content */}
      <div className="main-content">
        {children}
      </div>
    </div>
  );
};

export default Layout;
