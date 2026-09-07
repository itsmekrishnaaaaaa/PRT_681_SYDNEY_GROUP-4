import { api } from '@/shared/api/client';
import type { ExpenseCategory } from '@/shared/types';

export const getExpenseCategories = () =>
  api.get<ExpenseCategory[]>('/expense-categories').then((r) => r.data);

export const createExpenseCategory = (title: string) =>
  api.post<ExpenseCategory>('/expense-categories', { title }).then((r) => r.data);

export const deleteExpenseCategory = (id: string) =>
  api.delete(`/expense-categories/${id}`);
