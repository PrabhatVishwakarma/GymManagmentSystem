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

  getOpen: async (): Promise<Enquiry[]> => {
    const response = await api.get('/Enquiry/Open');
    return response.data;
  },

  getClosed: async (): Promise<Enquiry[]> => {
    const response = await api.get('/Enquiry/Closed');
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

  exportToExcel: async (): Promise<Blob> => {
    const response = await api.get('/MembersMembership/ExportToExcel', {
      responseType: 'blob'
    });
    return response.data;
  },

  upgrade: async (id: number, newMembershipPlanId: number, paidAmount: number, updatedBy: string): Promise<any> => {
    const response = await api.put(`/MembersMembership/${id}/Upgrade`, {
      newMembershipPlanId,
      paidAmount,
      updatedBy
    });
    return response.data;
  },

  toggleStatus: async (id: number, isInactive: boolean, updatedBy: string): Promise<any> => {
    const response = await api.put(`/MembersMembership/${id}/ToggleStatus`, {
      isInactive,
      updatedBy
    });
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/MembersMembership/${id}`);
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

// Sales Reports API
export const reportsAPI = {
  getSalesLast3Months: async (): Promise<any[]> => {
    const response = await api.get('/Reports/Sales/Last3Months');
    return response.data;
  },

  getSalesLast6Months: async (): Promise<any[]> => {
    const response = await api.get('/Reports/Sales/Last6Months');
    return response.data;
  },

  getSalesByDateRange: async (startDate?: string, endDate?: string): Promise<any[]> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    const response = await api.get(`/Reports/Sales?${params.toString()}`);
    return response.data;
  },

  exportSalesToExcel: async (startDate?: string, endDate?: string): Promise<Blob> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    const response = await api.get(`/Reports/Sales/ExportToExcel?${params.toString()}`, {
      responseType: 'blob'
    });
    return response.data;
  },
};

// Activity API
export const activityAPI = {
  getAll: async (limit?: number, activityType?: string, entityType?: string): Promise<any[]> => {
    const params = new URLSearchParams();
    if (limit) params.append('limit', limit.toString());
    if (activityType) params.append('activityType', activityType);
    if (entityType) params.append('entityType', entityType);
    
    const response = await api.get(`/Activity?${params.toString()}`);
    return response.data;
  },

  getRecent: async (limit: number = 50): Promise<any[]> => {
    const response = await api.get(`/Activity/Recent?limit=${limit}`);
    return response.data;
  },

  getByEntity: async (entityType: string, entityId: number): Promise<any[]> => {
    const response = await api.get(`/Activity/ByEntity/${entityType}/${entityId}`);
    return response.data;
  },

  getStats: async (): Promise<any> => {
    const response = await api.get('/Activity/Stats');
    return response.data;
  },
};

// Payment Receipts API
export const paymentReceiptAPI = {
  getByMember: async (membershipId: number): Promise<any[]> => {
    const response = await api.get(`/PaymentReceipt/member/${membershipId}`);
    return response.data;
  },

  getById: async (id: number): Promise<any> => {
    const response = await api.get(`/PaymentReceipt/${id}`);
    return response.data;
  },

  downloadReceipt: async (id: number): Promise<Blob> => {
    const response = await api.get(`/PaymentReceipt/${id}/download`, {
      responseType: 'blob'
    });
    return response.data;
  },

  viewReceiptHtml: async (id: number): Promise<string> => {
    const response = await api.get(`/PaymentReceipt/${id}/html`, {
      headers: {
        'Accept': 'text/html'
      }
    });
    return response.data;
  },

  getAll: async (): Promise<any[]> => {
    const response = await api.get('/PaymentReceipt/all');
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/PaymentReceipt/${id}`);
  },
};

export default api;
