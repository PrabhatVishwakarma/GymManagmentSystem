import axios from 'axios';
import { 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  Enquiry, 
  MembershipPlan, 
  MembersMembership,
  API_BASE_URL 
} from '../types/api';

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth API
export const authAPI = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post('/Auth/Login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post('/Auth/Register', data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await api.post('/Auth/Logout');
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },
};

// Enquiry API
export const enquiryAPI = {
  getAll: async (): Promise<Enquiry[]> => {
    const response = await api.get('/Enquiry');
    return response.data;
  },

  getById: async (id: number): Promise<Enquiry> => {
    const response = await api.get(`/Enquiry/${id}`);
    return response.data;
  },

  create: async (data: Partial<Enquiry>): Promise<Enquiry> => {
    const response = await api.post('/Enquiry', data);
    return response.data;
  },

  update: async (id: number, data: Partial<Enquiry>): Promise<void> => {
    await api.put(`/Enquiry/${id}`, data);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/Enquiry/${id}`);
  },

  convertToMember: async (id: number, membershipPlanId: number, paidAmount: number, createdBy: string): Promise<MembersMembership> => {
    const response = await api.post(`/Enquiry/${id}/ConvertToMember`, {
      membershipPlanId,
      paidAmount,
      createdBy
    });
    return response.data;
  },

  exportToExcel: async (): Promise<Blob> => {
    const response = await api.get('/Enquiry/ExportToExcel', {
      responseType: 'blob'
    });
    return response.data;
  },
};

// Membership Plan API
export const membershipPlanAPI = {
  getAll: async (): Promise<MembershipPlan[]> => {
    const response = await api.get('/MembershipPlan');
    return response.data;
  },

  getActive: async (): Promise<MembershipPlan[]> => {
    const response = await api.get('/MembershipPlan/Active');
    return response.data;
  },

  getById: async (id: number): Promise<MembershipPlan> => {
    const response = await api.get(`/MembershipPlan/${id}`);
    return response.data;
  },

  create: async (data: Partial<MembershipPlan>): Promise<MembershipPlan> => {
    const response = await api.post('/MembershipPlan', data);
    return response.data;
  },

  update: async (id: number, data: Partial<MembershipPlan>): Promise<void> => {
    await api.put(`/MembershipPlan/${id}`, data);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/MembershipPlan/${id}`);
  },
};

// Members Membership API
export const membersMembershipAPI = {
  getAll: async (): Promise<MembersMembership[]> => {
    const response = await api.get('/MembersMembership');
    return response.data;
  },

  getActive: async (): Promise<MembersMembership[]> => {
    const response = await api.get('/MembersMembership/Active');
    return response.data;
  },

  getById: async (id: number): Promise<MembersMembership> => {
    const response = await api.get(`/MembersMembership/${id}`);
    return response.data;
  },

  create: async (data: Partial<MembersMembership>): Promise<MembersMembership> => {
    const response = await api.post('/MembersMembership', data);
    return response.data;
  },

  processPayment: async (id: number, amount: number): Promise<any> => {
    const response = await api.post(`/MembersMembership/${id}/Payment`, {
      amount,
      paymentMethod: 'Cash',
      notes: ''
    });
    return response.data;
  },
};

// User API
export const userAPI = {
  getAll: async (): Promise<any[]> => {
    const response = await api.get('/User');
    return response.data;
  },

  getAvailableRoles: async (): Promise<string[]> => {
    const response = await api.get('/User/AvailableRoles');
    return response.data;
  },

  register: async (data: any): Promise<any> => {
    const response = await api.post('/User/Register', data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/User/${id}`);
  },
};

export default api;
