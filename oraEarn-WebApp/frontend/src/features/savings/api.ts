import { api } from '@/shared/api/client';
import type { AdvisorChatResponse, SavingsGoal, SavingsGoalAnalytics } from '@/shared/types';

export type SavingsGoalInput = {
  title: string;
  targetAmount: number;
  currentAmount?: number;
  allocationPercentage: number;
  targetDate?: string | null;
};

export const getSavingsGoals = () =>
  api.get<SavingsGoal[]>('/savingsgoals').then((r) => r.data);

export const getSavingsGoalAnalytics = () =>
  api.get<SavingsGoalAnalytics>('/savingsgoals/analytics').then((r) => r.data);

export const advisorChat = (message: string) =>
  api.post<AdvisorChatResponse>('/savingsgoals/advisor/chat', { message }).then((r) => r.data);

export const createSavingsGoal = (payload: SavingsGoalInput) =>
  api.post<SavingsGoal>('/savingsgoals', {
    ...payload,
    currentAmount: payload.currentAmount ?? 0,
    targetDate: payload.targetDate ?? null,
  }).then((r) => r.data);

export const updateSavingsGoal = (id: string, payload: SavingsGoalInput) =>
  api.put<SavingsGoal>(`/savingsgoals/${id}`, {
    ...payload,
    currentAmount: payload.currentAmount ?? 0,
    targetDate: payload.targetDate ?? null,
  }).then((r) => r.data);

export const contributeToSavingsGoal = (id: string, amount: number) =>
  api.post<SavingsGoal>(`/savingsgoals/${id}/contribute`, { amount }).then((r) => r.data);

export const deleteSavingsGoal = (id: string) => api.delete(`/savingsgoals/${id}`);

export const savingsStatusLabel: Record<string, string> = {
  Pending: 'Pending',
  InProgress: 'In progress',
  Completed: 'Completed',
};
