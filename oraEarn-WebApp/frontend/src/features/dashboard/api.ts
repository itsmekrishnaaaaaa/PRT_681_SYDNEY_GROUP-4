import { api } from '@/shared/api/client';
import type { AnalyticsPeriodKey, UserAnalytics } from '@/shared/types';

export const getMyAnalytics = (period: AnalyticsPeriodKey = '7d') =>
  api.get<UserAnalytics>('/analytics/me', { params: { period } }).then((r) => r.data);
