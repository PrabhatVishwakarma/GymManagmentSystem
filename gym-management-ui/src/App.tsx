import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import Dashboard from './components/dashboard/Dashboard';
import Enquiries from './components/enquiries/Enquiries';
import MembershipPlans from './components/membership/MembershipPlans';
import Members from './components/members/Members';
import Users from './components/users/Users';
import SalesReports from './components/reports/SalesReports';
import ActivityLog from './components/activity/ActivityLog';
import Layout from './components/layout/Layout';
import './App.css';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div className="loading">Loading...</div>;
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
};

const AppRoutes: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={!isAuthenticated ? <Login /> : <Navigate to="/dashboard" />} />
      <Route path="/register" element={!isAuthenticated ? <Register /> : <Navigate to="/dashboard" />} />
      <Route path="/" element={<Navigate to="/dashboard" />} />
      
      <Route path="/dashboard" element={
        <ProtectedRoute>
          <Layout>
            <Dashboard />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/enquiries" element={
        <ProtectedRoute>
          <Layout>
            <Enquiries />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/membership-plans" element={
        <ProtectedRoute>
          <Layout>
            <MembershipPlans />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/members" element={
        <ProtectedRoute>
          <Layout>
            <Members />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/users" element={
        <ProtectedRoute>
          <Layout>
            <Users />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/reports" element={
        <ProtectedRoute>
          <Layout>
            <SalesReports />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/activity" element={
        <ProtectedRoute>
          <Layout>
            <ActivityLog />
          </Layout>
        </ProtectedRoute>
      } />
    </Routes>
  );
};

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <AppRoutes />
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;