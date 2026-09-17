import { api } from '@/shared/api/client';
import type { ActiveShiftSession, Shift, ShiftSummary } from '@/shared/types';

export type ShiftInput = {
  jobId: string;
  workDate: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
};

export const getShifts = (from?: string, to?: string) =>
  api.get<Shift[]>('/shifts', { params: { from, to } }).then((r) => r.data);

export const getShiftSummary = (from?: string, to?: string) =>
  api.get<ShiftSummary>('/shifts/summary', { params: { from, to } }).then((r) => r.data);

export const getActiveSession = () =>
  api.get<ActiveShiftSession>('/shifts/active').then((r) => r.data);

export const clockIn = (jobId: string) =>
  api.post<ActiveShiftSession>('/shifts/clock-in', { jobId }).then((r) => r.data);

export const clockOut = () => api.post<Shift>('/shifts/clock-out').then((r) => r.data);

export const startBreak = () =>
  api.post<ActiveShiftSession>('/shifts/break/start').then((r) => r.data);

export const endBreak = () =>
  api.post<ActiveShiftSession>('/shifts/break/end').then((r) => r.data);

export const createShift = (payload: ShiftInput) =>
  api.post<Shift>('/shifts', payload).then((r) => r.data);

export const updateShift = (id: string, payload: ShiftInput) =>
  api.put<Shift>(`/shifts/${id}`, payload).then((r) => r.data);

export const deleteShift = (id: string) => api.delete(`/shifts/${id}`);
