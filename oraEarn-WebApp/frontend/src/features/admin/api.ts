import { api, getAccessToken } from '@/shared/api/client';
import { API_URL } from '@/shared/api/config';
import type { AdminAnalytics, AdminUser, PendingRegistration, RegistrationAiReview } from '@/shared/types';

export const getAdminAnalytics = () =>
  api.get<AdminAnalytics>('/admin/analytics').then((r) => r.data);

export const getAdminUsers = () => api.get<AdminUser[]>('/admin/users').then((r) => r.data);

export const updateUserRoles = (userId: string, roles: string[]) =>
  api.put(`/admin/users/${userId}/roles`, { roles });

export const getPendingRegistrations = () =>
  api.get<PendingRegistration[]>('/admin/registrations/pending').then((r) => r.data);

export const approveRegistration = (userId: string, notes?: string) =>
  api.post(`/admin/registrations/${userId}/approve`, { notes: notes ?? null });

export const rejectRegistration = (userId: string, notes?: string) =>
  api.post(`/admin/registrations/${userId}/reject`, { notes: notes ?? null });

export const aiReviewRegistration = (userId: string) =>
  api.post<RegistrationAiReview>(`/admin/registrations/${userId}/ai-review`).then((r) => r.data);

export const registrationDocumentUrl = (userId: string) =>
  `${API_URL}/admin/registrations/${userId}/document?token=${encodeURIComponent(getAccessToken() ?? '')}`;

export const openRegistrationDocument = async (userId: string) => {
  const token = getAccessToken();
  const res = await fetch(`${API_URL}/admin/registrations/${userId}/document`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) throw new Error('Could not load visa document.');
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  window.open(url, '_blank', 'noopener,noreferrer');
};
