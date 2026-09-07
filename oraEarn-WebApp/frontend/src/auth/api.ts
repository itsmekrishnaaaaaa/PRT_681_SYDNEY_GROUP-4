import { api } from '@/shared/api/client';
import type { RegisterResponse } from '@/shared/types';

export type RegisterPayload = {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  nationality: string;
  visaType: string;
  visaStatus: string;
  workingRights: string;
  visaExpiryDate: string;
  identityDocumentType: string;
  visaDocument: File;
};

export const login = (email: string, password: string) =>
  api.post('/auth/login', { email, password }).then((r) => r.data);

export const register = (payload: RegisterPayload) => {
  const form = new FormData();
  form.append('email', payload.email);
  form.append('password', payload.password);
  form.append('confirmPassword', payload.confirmPassword);
  form.append('firstName', payload.firstName);
  form.append('lastName', payload.lastName);
  form.append('phoneNumber', payload.phoneNumber);
  form.append('dateOfBirth', payload.dateOfBirth);
  form.append('nationality', payload.nationality);
  form.append('visaType', payload.visaType);
  form.append('visaStatus', payload.visaStatus);
  form.append('workingRights', payload.workingRights);
  form.append('visaExpiryDate', payload.visaExpiryDate);
  form.append('identityDocumentType', payload.identityDocumentType);
  form.append('visaDocument', payload.visaDocument);

  return api.post<RegisterResponse>('/auth/register', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }).then((r) => r.data);
};

export const logout = () => api.post('/auth/logout');

export const getMe = () => api.get('/auth/me').then((r) => r.data);
