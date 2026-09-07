import { api } from '@/shared/api/client';
import type { Expense, ExpenseCalculatorResult, ExpenseSummary, ExpenseKind } from '@/shared/types';

export type ExpenseInput = {
  categoryId: string;
  amount: number;
  date: string;
  expenseKind: ExpenseKind;
  paymentMethod: string;
};

export const getExpenses = (from?: string, to?: string) =>
  api.get<Expense[]>('/expenses', { params: { from, to } }).then((r) => r.data);

export const getExpenseSummary = (from?: string, to?: string) =>
  api.get<ExpenseSummary>('/expenses/summary', { params: { from, to } }).then((r) => r.data);

export const createExpense = (payload: ExpenseInput) =>
  api.post<Expense>('/expenses', payload).then((r) => r.data);

export const updateExpense = (id: string, payload: ExpenseInput) =>
  api.put<Expense>(`/expenses/${id}`, payload).then((r) => r.data);

export const deleteExpense = (id: string) => api.delete(`/expenses/${id}`);

export const calculateExpenses = (monthlyIncome: number, expenses: ExpenseInput[]) =>
  api
    .post<ExpenseCalculatorResult>('/expenses/calculate', { monthlyIncome, expenses })
    .then((r) => r.data);

export const getPaymentMethods = () =>
  api.get<{ id: string; name: string }[]>('/expenses/payment-methods').then((r) => r.data);
