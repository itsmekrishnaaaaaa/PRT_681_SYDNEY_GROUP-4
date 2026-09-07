import { api } from '@/shared/api/client';
import type { NotificationInbox } from '@/shared/types';

export const getNotifications = () =>
  api.get<NotificationInbox>('/notifications').then((r) => r.data);

export const markNotificationRead = (id: string) =>
  api.post(`/notifications/${encodeURIComponent(id)}/read`);

export const markAllNotificationsRead = (notificationIds: string[]) =>
  api.post('/notifications/read-all', { notificationIds });
