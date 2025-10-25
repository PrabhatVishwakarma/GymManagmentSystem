import React, { useState, useEffect } from 'react';
import { AlertCircle, CheckCircle, RefreshCw, Wifi, WifiOff } from 'lucide-react';

interface RetryStatus {
  isRetrying: boolean;
  attempt: number;
  maxAttempts: number;
  message: string;
}

const GlobalRetryNotification: React.FC = () => {
  const [retryStatus, setRetryStatus] = useState<RetryStatus | null>(null);
  const [showSuccess, setShowSuccess] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const handleRetrying = (event: Event) => {
      const customEvent = event as CustomEvent;
      const { attempt, maxAttempts } = customEvent.detail;
      
      setRetryStatus({
        isRetrying: true,
        attempt,
        maxAttempts,
        message: `Connecting to server... (Attempt ${attempt}/${maxAttempts})`
      });
    };

    const handleRetrySuccess = (event: Event) => {
      const customEvent = event as CustomEvent;
      const { attempts } = customEvent.detail;
      
      // Clear retry status
      setRetryStatus(null);
      
      // Show success message
      setSuccessMessage(`Connection restored after ${attempts} attempts!`);
      setShowSuccess(true);
      
      // Hide success message after 4 seconds
      setTimeout(() => {
        setShowSuccess(false);
      }, 4000);
    };

    const handleRetryFailed = () => {
      setRetryStatus({
        isRetrying: false,
        attempt: 0,
        maxAttempts: 0,
        message: 'Unable to connect to server. Please check your connection and try again.'
      });
      
      // Hide error message after 10 seconds
      setTimeout(() => {
        setRetryStatus(null);
      }, 10000);
    };

    // Add event listeners
    window.addEventListener('apiRetrying', handleRetrying);
    window.addEventListener('apiRetrySuccess', handleRetrySuccess);
    window.addEventListener('apiRetryFailed', handleRetryFailed);

    // Cleanup
    return () => {
      window.removeEventListener('apiRetrying', handleRetrying);
      window.removeEventListener('apiRetrySuccess', handleRetrySuccess);
      window.removeEventListener('apiRetryFailed', handleRetryFailed);
    };
  }, []);

  // Success notification
  if (showSuccess) {
    return (
      <div style={{
        position: 'fixed',
        top: '20px',
        right: '20px',
        zIndex: 10000,
        minWidth: '320px',
        maxWidth: '450px',
        backgroundColor: '#D1FAE5',
        border: '2px solid #10B981',
        borderRadius: '8px',
        padding: '16px',
        boxShadow: '0 10px 25px rgba(0, 0, 0, 0.15)',
        animation: 'slideInRight 0.3s ease-out'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <div style={{
            backgroundColor: '#10B981',
            borderRadius: '50%',
            padding: '8px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <CheckCircle size={24} style={{ color: 'white' }} />
          </div>
          <div style={{ flex: 1 }}>
            <p style={{ 
              margin: 0, 
              color: '#065F46', 
              fontWeight: '600',
              fontSize: '14px'
            }}>
              {successMessage}
            </p>
            <p style={{ 
              margin: '4px 0 0 0', 
              color: '#047857', 
              fontSize: '12px'
            }}>
              All data will now load automatically
            </p>
          </div>
          <Wifi size={20} style={{ color: '#10B981' }} />
        </div>
      </div>
    );
  }

  // Retry or error notification
  if (retryStatus) {
    const isError = !retryStatus.isRetrying && retryStatus.attempt === 0;
    
    return (
      <div style={{
        position: 'fixed',
        top: '20px',
        right: '20px',
        zIndex: 10000,
        minWidth: '320px',
        maxWidth: '450px',
        backgroundColor: isError ? '#FEE2E2' : '#FEF3C7',
        border: `2px solid ${isError ? '#EF4444' : '#F59E0B'}`,
        borderRadius: '8px',
        padding: '16px',
        boxShadow: '0 10px 25px rgba(0, 0, 0, 0.15)',
        animation: 'slideInRight 0.3s ease-out'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <div style={{
            backgroundColor: isError ? '#EF4444' : '#F59E0B',
            borderRadius: '50%',
            padding: '8px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            {isError ? (
              <WifiOff size={24} style={{ color: 'white' }} />
            ) : (
              <RefreshCw 
                size={24} 
                style={{ 
                  color: 'white',
                  animation: 'spin 1s linear infinite'
                }} 
              />
            )}
          </div>
          <div style={{ flex: 1 }}>
            <p style={{ 
              margin: 0, 
              color: isError ? '#991B1B' : '#92400E', 
              fontWeight: '600',
              fontSize: '14px'
            }}>
              {retryStatus.message}
            </p>
            {!isError && (
              <p style={{ 
                margin: '4px 0 0 0', 
                color: '#B45309', 
                fontSize: '12px'
              }}>
                Please wait while we reconnect...
              </p>
            )}
            {isError && (
              <p style={{ 
                margin: '4px 0 0 0', 
                color: '#DC2626', 
                fontSize: '12px'
              }}>
                Please refresh the page to try again
              </p>
            )}
          </div>
          {isError ? (
            <AlertCircle size={20} style={{ color: '#EF4444' }} />
          ) : (
            <div style={{
              width: '20px',
              height: '20px',
              border: '3px solid #FDE68A',
              borderTopColor: '#F59E0B',
              borderRadius: '50%',
              animation: 'spin 1s linear infinite'
            }} />
          )}
        </div>
      </div>
    );
  }

  return (
    <>
      <style>{`
        @keyframes slideInRight {
          from {
            transform: translateX(100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }
        
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </>
  );
};

export default GlobalRetryNotification;


