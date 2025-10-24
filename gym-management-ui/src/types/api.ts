// API Configuration
// Use environment variable if available, otherwise fallback to default
export const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:5202/api';

// Types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  gender: string;
  address: string;
  dateOfBirth: string;
  occupation: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  gender: string;
  address: string;
  dateOfBirth: string;
  occupation: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  roles: string[];
}

export interface Enquiry {
  enquiryId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  isWhatsappNumber: boolean;
  address: string;
  city: string;
  gender: string;
  dateOfBirth: string;
  occupation: string;
  isConverted: boolean;
  convertedDate?: string;
  createdby: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
}

export interface MembershipPlan {
  membershipPlanId: number;
  planName: string;
  planType: string;
  durationInMonths: number;
  price: number;
  description: string;
  isActive: boolean;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
}

export interface MembersMembership {
  membersMembershipId: number;
  enquiryId: number;
  membershipPlanId: number;
  startDate: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  nextPaymentDueDate: string;
  isInactive?: boolean;
  isActive: boolean;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
  enquiry?: Enquiry;
  membershipPlan?: MembershipPlan;
}
