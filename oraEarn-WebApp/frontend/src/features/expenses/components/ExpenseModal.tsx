import { type FormEvent, useEffect, useState } from 'react';
import type { Expense, ExpenseKind } from '@/shared/types';
import { expenseKindOptions } from '@/shared/types';
import {
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  pickErrors,
  useFormValidation,
  validators,
} from '@/shared/form-field';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Loader2 } from 'lucide-react';

type ExpenseFields = 'categoryId' | 'amount' | 'date' | '_form';

type Category = { id: string; title: string };

const toKind = (kind: ExpenseKind | number): ExpenseKind =>
  typeof kind === 'number' ? expenseKindOptions[kind]?.value ?? 'Fixed' : kind;

type ExpenseModalProps = {
  open: boolean;
  expense: Expense | null;
  categories: Category[];
  paymentMethods: string[];
  defaultCategoryId?: string;
  saving: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (values: {
    categoryId: string;
    amount: number;
    date: string;
    expenseKind: ExpenseKind;
    paymentMethod: string;
  }) => Promise<void>;
};

export function ExpenseModal({
  open,
  expense,
  categories,
  paymentMethods,
  defaultCategoryId,
  saving,
  error,
  onClose,
  onSubmit,
}: ExpenseModalProps) {
  const [form, setForm] = useState({
    categoryId: '',
    amount: '',
    date: new Date().toISOString().slice(0, 10),
    expenseKind: 'Fixed' as ExpenseKind,
    paymentMethod: 'Card',
  });
  const { errors, clearError, validate, setErrors, clearAll } = useFormValidation<ExpenseFields>();

  useEffect(() => {
    if (!open) return;
    if (expense) {
      setForm({
        categoryId: expense.categoryId,
        amount: String(expense.amount),
        date: expense.date.slice(0, 10),
        expenseKind: toKind(expense.expenseKind),
        paymentMethod: expense.paymentMethod,
      });
    } else {
      setForm({
        categoryId: defaultCategoryId || categories[0]?.id || '',
        amount: '',
        date: new Date().toISOString().slice(0, 10),
        expenseKind: 'Fixed',
        paymentMethod: paymentMethods[0] ?? 'Card',
      });
    }
    clearAll();
  }, [open, expense, categories, defaultCategoryId, paymentMethods, clearAll]);

  useEffect(() => {
    if (open && error) setErrors((prev) => ({ ...prev, _form: error }));
  }, [error, open, setErrors]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['categoryId', validators.required(form.categoryId, 'Select a category.')],
          ['amount', validators.minNumber(form.amount, 0.01, 'Enter a valid amount.')],
          ['date', validators.dateRequired(form.date, 'Date is required.')],
        ]),
      )
    ) {
      return;
    }
    await onSubmit({
      categoryId: form.categoryId,
      amount: Number(form.amount),
      date: form.date,
      expenseKind: form.expenseKind,
      paymentMethod: form.paymentMethod,
    });
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && !saving && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{expense ? 'Edit expense' : 'Add expense'}</DialogTitle>
          <DialogDescription>Pick the date, amount, and how you paid.</DialogDescription>
        </DialogHeader>
        {categories.length === 0 && (
          <p className="text-sm text-destructive">Add a category first, then log an expense.</p>
        )}
        <FormErrorSummary errors={errors} />
        <form className="grid gap-4 sm:grid-cols-2" noValidate onSubmit={(e) => void handleSubmit(e)}>
          <FormField label="Category" error={errors.categoryId} required className="sm:col-span-2">
            <Select
              value={form.categoryId || undefined}
              onValueChange={(v) => {
                setForm({ ...form, categoryId: v });
                clearError('categoryId');
              }}
              disabled={categories.length === 0 || saving}
            >
              <SelectTrigger className={fieldInvalidClass(errors.categoryId)}>
                <SelectValue placeholder="Select category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.title}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <FormField label="Date" error={errors.date} required>
            <Input
              type="date"
              disabled={saving}
              className={fieldInvalidClass(errors.date)}
              value={form.date}
              onChange={(e) => {
                setForm({ ...form, date: e.target.value });
                clearError('date');
              }}
            />
          </FormField>
          <FormField label="Amount" error={errors.amount} required>
            <Input
              type="number"
              min="0"
              step="0.01"
              disabled={saving}
              className={fieldInvalidClass(errors.amount)}
              value={form.amount}
              onChange={(e) => {
                setForm({ ...form, amount: e.target.value });
                clearError('amount');
              }}
            />
          </FormField>
          <FormField label="Kind">
            <Select value={form.expenseKind} onValueChange={(v) => setForm({ ...form, expenseKind: v as ExpenseKind })} disabled={saving}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {expenseKindOptions.map((o) => (
                  <SelectItem key={o.value} value={o.value}>
                    {o.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <FormField label="Payment">
            <Select value={form.paymentMethod} onValueChange={(v) => setForm({ ...form, paymentMethod: v })} disabled={saving}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {paymentMethods.map((m) => (
                  <SelectItem key={m} value={m}>
                    {m}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <div className="flex flex-col-reverse gap-2 sm:col-span-2 sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" disabled={saving} onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving || categories.length === 0} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              {saving ? 'Saving…' : expense ? 'Update expense' : 'Save expense'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
