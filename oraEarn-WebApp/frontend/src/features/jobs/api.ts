import { api } from '@/shared/api/client';
import type { Job } from '@/shared/types';

export type JobInput = {
  title: string;
  hourlyRate: number;
  dateOfJoin: string;
};

export const getJobs = () => api.get<Job[]>('/jobs').then((r) => r.data);

export const createJob = (payload: JobInput) =>
  api.post<Job>('/jobs', payload).then((r) => r.data);

export const updateJob = (id: string, payload: JobInput) =>
  api.put<Job>(`/jobs/${id}`, payload).then((r) => r.data);

export const deleteJob = (id: string) => api.delete(`/jobs/${id}`);
