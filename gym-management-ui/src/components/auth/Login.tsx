import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { LogIn } from 'lucide-react';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-title">
          <LogIn size={32} style={{ marginBottom: '0.5rem' }} />
          <div>Gym Management System</div>
        </div>
        
        <form onSubmit={handleSubmit} className="auth-form">
          {error && (
            <div style={{ color: '#dc3545', fontSize: '0.9rem', textAlign: 'center' }}>
              {error}
            </div>
          )}
          
          <div className="form-group">
            <label className="form-label">Email</label>
            <input
              type="email"
              className="form-input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          
          <div className="form-group">
            <label className="form-label">Password</label>
            <input
              type="password"
              className="form-input"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          
          <button 
            type="submit" 
            className="btn btn-primary"
            disabled={isLoading}
          >
            {isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>
        
        <div className="auth-link">
          Don't have an account? <Link to="/register">Register here</Link>
        </div>
        
        <div style={{ marginTop: '1rem', padding: '1rem', background: '#f8f9fa', borderRadius: '8px', fontSize: '0.9rem' }}>
          <strong>Demo Credentials:</strong><br />
          Admin: admin@gym.com / AdminPassword123!
        </div>
      </div>
    </div>
  );
};

export default Login;
