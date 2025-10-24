import React, { useState, useEffect, useRef } from 'react';
import { User, Lock, Mail, Phone, MapPin, Calendar, Briefcase, Save, Camera, Upload, X, Crop } from 'lucide-react';
import { userAPI } from '../../services/api';
import ReactCrop, { Crop as CropType, PixelCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

const Profile: React.FC = () => {
  const [profile, setProfile] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [profilePhoto, setProfilePhoto] = useState<string>('');
  const [photoPreview, setPhotoPreview] = useState<string>('');
  const [showCropModal, setShowCropModal] = useState(false);
  const [imageToCrop, setImageToCrop] = useState<string>('');
  const [crop, setCrop] = useState<CropType>({
    unit: '%',
    width: 50,
    height: 50,
    x: 25,
    y: 25
  });
  const [completedCrop, setCompletedCrop] = useState<PixelCrop | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const imgRef = useRef<HTMLImageElement>(null);

  const [editMode, setEditMode] = useState(false);
  
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    address: '',
    dateOfBirth: '',
    occupation: '',
    gender: ''
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      setLoading(true);
      setError(''); // Clear any previous errors
      
      console.log('Fetching profile...');
      const data = await userAPI.getProfile();
      console.log('Profile data received:', data);
      
      setProfile(data);
      
      // Load profile photo from database
      if (data.profilePhotoUrl) {
        console.log('Loading photo from database');
        setProfilePhoto(data.profilePhotoUrl);
        setPhotoPreview(data.profilePhotoUrl);
        
        // Also save to localStorage as backup
        localStorage.setItem(`profilePhoto_${data.id}`, data.profilePhotoUrl);
      } else {
        console.log('No photo in database, checking localStorage');
        // Fallback to localStorage if not in database
        const savedPhoto = localStorage.getItem(`profilePhoto_${data.id}`);
        if (savedPhoto) {
          console.log('Found photo in localStorage');
          setProfilePhoto(savedPhoto);
          setPhotoPreview(savedPhoto);
        }
      }
      
      setFormData({
        firstName: data.firstName || '',
        lastName: data.lastName || '',
        email: data.email || '',
        phoneNumber: data.phoneNumber || '',
        address: data.address || '',
        dateOfBirth: data.dateOfBirth ? new Date(data.dateOfBirth).toISOString().split('T')[0] : '',
        occupation: data.occupation || '',
        gender: data.gender || ''
      });
      
      console.log('Profile loaded successfully');
      
    } catch (err: any) {
      console.error('Error fetching profile:', err);
      console.error('Error details:', err.response?.data);
      
      const errorMessage = err.response?.data?.message 
        || err.response?.data?.title
        || err.message 
        || 'Failed to load profile. The system will automatically retry...';
      
      setError(errorMessage);
      
    } finally {
      setLoading(false);
    }
  };

  const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Check file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        setError('Image size should be less than 5MB');
        return;
      }

      // Check file type
      if (!file.type.startsWith('image/')) {
        setError('Please select an image file');
        return;
      }

      const reader = new FileReader();
      reader.onloadend = () => {
        const base64String = reader.result as string;
        setImageToCrop(base64String);
        setShowCropModal(true);
        // Reset crop
        setCrop({
          unit: '%',
          width: 50,
          height: 50,
          x: 25,
          y: 25
        });
      };
      reader.readAsDataURL(file);
    }
    
    // Reset file input
    if (e.target) {
      e.target.value = '';
    }
  };

  const getCroppedImage = async (): Promise<string> => {
    if (!completedCrop || !imgRef.current) {
      return imageToCrop;
    }

    const image = imgRef.current;
    const canvas = document.createElement('canvas');
    const scaleX = image.naturalWidth / image.width;
    const scaleY = image.naturalHeight / image.height;
    
    canvas.width = completedCrop.width;
    canvas.height = completedCrop.height;
    const ctx = canvas.getContext('2d');

    if (!ctx) {
      return imageToCrop;
    }

    ctx.drawImage(
      image,
      completedCrop.x * scaleX,
      completedCrop.y * scaleY,
      completedCrop.width * scaleX,
      completedCrop.height * scaleY,
      0,
      0,
      completedCrop.width,
      completedCrop.height
    );

    return canvas.toDataURL('image/jpeg', 0.9);
  };

  const handleCropComplete = async () => {
    try {
      const croppedImage = await getCroppedImage();
      setPhotoPreview(croppedImage);
      setShowCropModal(false);
      setImageToCrop('');
    } catch (err) {
      setError('Failed to crop image');
      console.error('Crop error:', err);
    }
  };

  const handleCropCancel = () => {
    setShowCropModal(false);
    setImageToCrop('');
    setCompletedCrop(null);
  };

  const handleSavePhoto = async () => {
    try {
      setSaving(true);
      setError('');
      
      if (photoPreview) {
        // Save to database
        await userAPI.updateProfilePhoto(profile.id, photoPreview);
        
        // Also save to localStorage as cache
        localStorage.setItem(`profilePhoto_${profile.id}`, photoPreview);
        
        setProfilePhoto(photoPreview);
        setSuccess('Profile photo updated successfully!');
        
        // Notify other components about the photo update
        window.dispatchEvent(new CustomEvent('profilePhotoUpdated', { 
          detail: { userId: profile.id, photoUrl: photoPreview } 
        }));
        
        // Reload profile to get updated data
        await fetchProfile();
        
        setTimeout(() => setSuccess(''), 3000);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update profile photo');
      console.error('Error saving photo:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleRemovePhoto = async () => {
    try {
      setSaving(true);
      setError('');
      
      // Remove from database
      await userAPI.updateProfilePhoto(profile.id, '');
      
      // Remove from localStorage
      if (profile?.id) {
        localStorage.removeItem(`profilePhoto_${profile.id}`);
      }
      
      setPhotoPreview('');
      setProfilePhoto('');
      setSuccess('Profile photo removed');
      
      // Notify other components about the photo removal
      window.dispatchEvent(new CustomEvent('profilePhotoUpdated', { 
        detail: { userId: profile.id, photoUrl: '' } 
      }));
      
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: any) {
      setError('Failed to remove profile photo');
      console.error('Error removing photo:', err);
    } finally {
      setSaving(false);
    }
  };

  const getInitials = () => {
    if (!profile) return '';
    const firstInitial = profile.firstName?.[0] || '';
    const lastInitial = profile.lastName?.[0] || '';
    return `${firstInitial}${lastInitial}`.toUpperCase();
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setSaving(true);
      setError('');
      setSuccess('');

      await userAPI.update(profile.id, {
        ...formData,
        updatedBy: profile.email
      });

      setSuccess('Profile updated successfully!');
      setEditMode(false);
      
      // Notify other components about profile update
      window.dispatchEvent(new CustomEvent('profileUpdated', { 
        detail: { userId: profile.id, name: `${formData.firstName} ${formData.lastName}` } 
      }));
      
      await fetchProfile();
      
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update profile');
      console.error('Error updating profile:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setError('New passwords do not match');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    try {
      setSaving(true);
      setError('');
      setSuccess('');

      await userAPI.changePassword(
        profile.id,
        passwordData.currentPassword,
        passwordData.newPassword
      );

      setSuccess('Password changed successfully!');
      setShowChangePassword(false);
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
      
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to change password');
      console.error('Error changing password:', err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div style={{ 
        display: 'flex', 
        flexDirection: 'column',
        alignItems: 'center', 
        justifyContent: 'center', 
        minHeight: '400px',
        padding: '2rem'
      }}>
        <div style={{
          width: '48px',
          height: '48px',
          border: '4px solid #E5E7EB',
          borderTopColor: '#4F46E5',
          borderRadius: '50%',
          animation: 'spin 1s linear infinite'
        }}></div>
        <p style={{ marginTop: '1rem', color: '#6B7280', fontSize: '0.875rem' }}>
          Loading your profile...
        </p>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4 m-4">
        <p className="text-red-800">Failed to load profile</p>
      </div>
    );
  }

  return (
    <>
      <style>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
      
      {/* Crop Modal */}
      {showCropModal && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.75)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: '12px',
            padding: '2rem',
            maxWidth: '600px',
            width: '90%',
            maxHeight: '90vh',
            overflow: 'auto'
          }}>
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center',
              marginBottom: '1.5rem'
            }}>
              <h2 style={{ 
                fontSize: '1.5rem', 
                fontWeight: '600', 
                color: '#111827',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem'
              }}>
                <Crop size={24} />
                Crop Profile Photo
              </h2>
              <button
                onClick={handleCropCancel}
                style={{
                  backgroundColor: 'transparent',
                  border: 'none',
                  cursor: 'pointer',
                  padding: '0.5rem',
                  borderRadius: '6px'
                }}
                onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#F3F4F6'}
                onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
              >
                <X size={24} style={{ color: '#6B7280' }} />
              </button>
            </div>

            <div style={{ marginBottom: '1.5rem' }}>
              <p style={{ color: '#6B7280', fontSize: '0.875rem', marginBottom: '1rem' }}>
                Drag to adjust the crop area. The selected area will be your profile photo.
              </p>
              
              <div style={{ 
                display: 'flex', 
                justifyContent: 'center',
                backgroundColor: '#F9FAFB',
                padding: '1rem',
                borderRadius: '8px',
                maxHeight: '400px',
                overflow: 'auto'
              }}>
                <ReactCrop
                  crop={crop}
                  onChange={(c) => setCrop(c)}
                  onComplete={(c) => setCompletedCrop(c)}
                  aspect={1}
                  circularCrop
                >
                  <img
                    ref={imgRef}
                    src={imageToCrop}
                    alt="Crop preview"
                    style={{ maxWidth: '100%', maxHeight: '350px' }}
                  />
                </ReactCrop>
              </div>
            </div>

            <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
              <button
                onClick={handleCropCancel}
                style={{
                  padding: '0.75rem 1.5rem',
                  backgroundColor: 'white',
                  color: '#374151',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: '500',
                  fontSize: '0.875rem'
                }}
              >
                Cancel
              </button>
              <button
                onClick={handleCropComplete}
                style={{
                  padding: '0.75rem 1.5rem',
                  backgroundColor: '#4F46E5',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: '500',
                  fontSize: '0.875rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.5rem'
                }}
              >
                <Crop size={16} />
                Apply Crop
              </button>
            </div>
          </div>
        </div>
      )}

      <div style={{ padding: '2rem', maxWidth: '900px', margin: '0 auto' }}>
        {/* Profile Header with Photo */}
        <div style={{ 
          marginBottom: '2rem',
          display: 'flex',
          alignItems: 'center',
          gap: '2rem',
          backgroundColor: 'white',
          padding: '2rem',
          borderRadius: '8px',
          boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)'
        }}>
        {/* Profile Photo */}
        <div style={{ position: 'relative' }}>
          <div style={{
            width: '120px',
            height: '120px',
            borderRadius: '50%',
            overflow: 'hidden',
            backgroundColor: '#4F46E5',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'white',
            fontSize: '2.5rem',
            fontWeight: 'bold',
            border: '4px solid #E5E7EB',
            boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
          }}>
            {photoPreview || profilePhoto ? (
              <img 
                src={photoPreview || profilePhoto} 
                alt="Profile" 
                style={{ width: '100%', height: '100%', objectFit: 'cover' }}
              />
            ) : (
              <span>{getInitials()}</span>
            )}
          </div>
          
          {/* Upload Button */}
          <button
            onClick={() => fileInputRef.current?.click()}
            style={{
              position: 'absolute',
              bottom: '0',
              right: '0',
              backgroundColor: '#4F46E5',
              color: 'white',
              border: 'none',
              borderRadius: '50%',
              width: '36px',
              height: '36px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              boxShadow: '0 2px 4px rgba(0, 0, 0, 0.2)'
            }}
          >
            <Camera size={18} />
          </button>
          
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={handlePhotoChange}
            style={{ display: 'none' }}
          />
        </div>

        {/* Profile Info */}
        <div style={{ flex: 1 }}>
          <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: '#111827', marginBottom: '0.5rem' }}>
            {profile?.firstName} {profile?.lastName}
          </h1>
          <p style={{ color: '#6B7280', marginBottom: '1rem' }}>{profile?.email}</p>
          
          {/* Photo Actions */}
          {(photoPreview && photoPreview !== profilePhoto) && (
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
              <button
                onClick={handleSavePhoto}
                disabled={saving}
                style={{
                  padding: '0.5rem 1rem',
                  backgroundColor: '#10B981',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: saving ? 'not-allowed' : 'pointer',
                  fontWeight: '500',
                  fontSize: '0.875rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.5rem'
                }}
              >
                <Upload size={16} />
                {saving ? 'Saving...' : 'Save Photo'}
              </button>
              <button
                onClick={() => setPhotoPreview(profilePhoto)}
                style={{
                  padding: '0.5rem 1rem',
                  backgroundColor: 'white',
                  color: '#374151',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: '500',
                  fontSize: '0.875rem'
                }}
              >
                Cancel
              </button>
            </div>
          )}
          
          {(profilePhoto || photoPreview) && (photoPreview === profilePhoto) && (
            <button
              onClick={handleRemovePhoto}
              style={{
                padding: '0.5rem 1rem',
                backgroundColor: '#EF4444',
                color: 'white',
                border: 'none',
                borderRadius: '6px',
                cursor: 'pointer',
                fontWeight: '500',
                fontSize: '0.875rem',
                marginTop: '1rem'
              }}
            >
              Remove Photo
            </button>
          )}
        </div>
      </div>

      {/* Success Message */}
      {success && (
        <div style={{
          backgroundColor: '#D1FAE5',
          border: '1px solid #10B981',
          borderRadius: '6px',
          padding: '1rem',
          marginBottom: '1rem'
        }}>
          <p style={{ color: '#065F46', fontWeight: '500' }}>{success}</p>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div style={{
          backgroundColor: '#FEE2E2',
          border: '1px solid #EF4444',
          borderRadius: '6px',
          padding: '1rem',
          marginBottom: '1rem'
        }}>
          <p style={{ color: '#991B1B', fontWeight: '500' }}>{error}</p>
        </div>
      )}

      {/* Profile Information Card */}
      <div style={{
        backgroundColor: 'white',
        borderRadius: '8px',
        boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
        padding: '2rem',
        marginBottom: '1.5rem'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <h2 style={{ fontSize: '1.25rem', fontWeight: '600', color: '#111827', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <User size={20} />
            Personal Information
          </h2>
          {!editMode && (
            <button
              onClick={() => setEditMode(true)}
              style={{
                padding: '0.5rem 1rem',
                backgroundColor: '#4F46E5',
                color: 'white',
                border: 'none',
                borderRadius: '6px',
                cursor: 'pointer',
                fontWeight: '500'
              }}
            >
              Edit Profile
            </button>
          )}
        </div>

        {editMode ? (
          <form onSubmit={handleUpdateProfile}>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  First Name
                </label>
                <input
                  type="text"
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Last Name
                </label>
                <input
                  type="text"
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Email
                </label>
                <input
                  type="email"
                  value={formData.email}
                  disabled
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px',
                    backgroundColor: '#F3F4F6'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Phone Number
                </label>
                <input
                  type="tel"
                  value={formData.phoneNumber}
                  onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                />
              </div>

              <div style={{ gridColumn: '1 / -1' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Address
                </label>
                <input
                  type="text"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Date of Birth
                </label>
                <input
                  type="date"
                  value={formData.dateOfBirth}
                  onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Gender
                </label>
                <select
                  value={formData.gender}
                  onChange={(e) => setFormData({ ...formData, gender: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                >
                  <option value="">Select Gender</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </div>

              <div style={{ gridColumn: '1 / -1' }}>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Occupation
                </label>
                <input
                  type="text"
                  value={formData.occupation}
                  onChange={(e) => setFormData({ ...formData, occupation: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                />
              </div>
            </div>

            <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
              <button
                type="submit"
                disabled={saving}
                style={{
                  padding: '0.5rem 1.5rem',
                  backgroundColor: '#4F46E5',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: saving ? 'not-allowed' : 'pointer',
                  fontWeight: '500',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.5rem'
                }}
              >
                <Save size={16} />
                {saving ? 'Saving...' : 'Save Changes'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setEditMode(false);
                  setError('');
                  fetchProfile();
                }}
                style={{
                  padding: '0.5rem 1.5rem',
                  backgroundColor: 'white',
                  color: '#374151',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: '500'
                }}
              >
                Cancel
              </button>
            </div>
          </form>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1.5rem' }}>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <User size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Name</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>{profile.firstName} {profile.lastName}</p>
            </div>

            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <Mail size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Email</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>{profile.email}</p>
            </div>

            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <Phone size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Phone</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>{profile.phoneNumber || 'Not provided'}</p>
            </div>

            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <Calendar size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Date of Birth</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>
                {profile.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : 'Not provided'}
              </p>
            </div>

            <div style={{ gridColumn: '1 / -1' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <MapPin size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Address</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>{profile.address || 'Not provided'}</p>
            </div>

            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                <Briefcase size={16} style={{ color: '#6B7280' }} />
                <span style={{ color: '#6B7280', fontSize: '0.875rem' }}>Occupation</span>
              </div>
              <p style={{ fontWeight: '500', color: '#111827' }}>{profile.occupation || 'Not provided'}</p>
            </div>
          </div>
        )}
      </div>

      {/* Change Password Card */}
      <div style={{
        backgroundColor: 'white',
        borderRadius: '8px',
        boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
        padding: '2rem'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <h2 style={{ fontSize: '1.25rem', fontWeight: '600', color: '#111827', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Lock size={20} />
            Security
          </h2>
          {!showChangePassword && (
            <button
              onClick={() => setShowChangePassword(true)}
              style={{
                padding: '0.5rem 1rem',
                backgroundColor: '#4F46E5',
                color: 'white',
                border: 'none',
                borderRadius: '6px',
                cursor: 'pointer',
                fontWeight: '500'
              }}
            >
              Change Password
            </button>
          )}
        </div>

        {showChangePassword ? (
          <form onSubmit={handleChangePassword}>
            <div style={{ display: 'grid', gap: '1rem', maxWidth: '400px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Current Password
                </label>
                <input
                  type="password"
                  value={passwordData.currentPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  New Password
                </label>
                <input
                  type="password"
                  value={passwordData.newPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                  required
                  minLength={6}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#374151' }}>
                  Confirm New Password
                </label>
                <input
                  type="password"
                  value={passwordData.confirmPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    border: '1px solid #D1D5DB',
                    borderRadius: '6px'
                  }}
                  required
                  minLength={6}
                />
              </div>
            </div>

            <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
              <button
                type="submit"
                disabled={saving}
                style={{
                  padding: '0.5rem 1.5rem',
                  backgroundColor: '#4F46E5',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: saving ? 'not-allowed' : 'pointer',
                  fontWeight: '500'
                }}
              >
                {saving ? 'Changing...' : 'Change Password'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowChangePassword(false);
                  setPasswordData({
                    currentPassword: '',
                    newPassword: '',
                    confirmPassword: ''
                  });
                  setError('');
                }}
                style={{
                  padding: '0.5rem 1.5rem',
                  backgroundColor: 'white',
                  color: '#374151',
                  border: '1px solid #D1D5DB',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: '500'
                }}
              >
                Cancel
              </button>
            </div>
          </form>
        ) : (
          <p style={{ color: '#6B7280' }}>
            Keep your account secure by using a strong password and changing it regularly.
          </p>
        )}
      </div>
      </div>
    </>
  );
};

export default Profile;

