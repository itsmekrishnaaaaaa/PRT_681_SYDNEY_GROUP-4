import { type FormEvent, useEffect, useState } from 'react';
import type { SavingsGoal } from '@/shared/types';
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
import { Loader2 } from 'lucide-react';

type GoalFields = 'title' | 'targetAmount' | 'currentAmount' | 'allocationPercentage' | '_form';

const blankForm = () => ({
  title: '',
  targetAmount: '',
  currentAmount: '0',
  allocationPercentage: '10',
  targetDate: '',
});

type SavingsGoalModalProps = {
  open: boolean;
  goal: SavingsGoal | null;
  saving: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (values: {
    title: string;
    targetAmount: number;
    currentAmount: number;
    allocationPercentage: number;
    targetDate: string | null;
  }) => Promise<void>;
};

export function SavingsGoalModal({ open, goal, saving, error, onClose, onSubmit }: SavingsGoalModalProps) {
  const [form, setForm] = useState(blankForm);
  const { errors, clearError, validate, setErrors, clearAll } = useFormValidation<GoalFields>();

  useEffect(() => {
    if (!open) return;
    setForm(
      goal
        ? {
            title: goal.title,
            targetAmount: String(goal.targetAmount),
            currentAmount: String(goal.currentAmount),
            allocationPercentage: String(goal.allocationPercentage),
            targetDate: goal.targetDate ? goal.targetDate.slice(0, 10) : '',
          }
        : blankForm(),
    );
    clearAll();
  }, [open, goal, clearAll]);

  useEffect(() => {
    if (open && error) setErrors((prev) => ({ ...prev, _form: error }));
  }, [error, open, setErrors]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['title', validators.required(form.title, 'Goal title is required.')],
          ['targetAmount', validators.minNumber(form.targetAmount, 0.01, 'Enter a valid target amount.')],
          ['currentAmount', form.currentAmount ? validators.minNumber(form.currentAmount, 0, 'Starting amount cannot be negative.') : undefined],
          [
            'allocationPercentage',
            validators.minNumber(form.allocationPercentage, 0, 'Allocation must be at least 0%.') ??
              validators.maxNumber(form.allocationPercentage, 100, 'Allocation cannot exceed 100%.'),
          ],
        ]),
      )
    ) {
      return;
    }
    await onSubmit({
      title: form.title,
      targetAmount: Number(form.targetAmount),
      currentAmount: Number(form.currentAmount),
      allocationPercentage: Number(form.allocationPercentage),
      targetDate: form.targetDate || null,
    });
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && !saving && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{goal ? 'Edit savings goal' : 'New savings goal'}</DialogTitle>
          <DialogDescription>
            Set a target and the % of leftover pay (shifts minus expenses) to put aside. The saved amount updates when you log shifts or expenses.
          </DialogDescription>
        </DialogHeader>
        <FormErrorSummary errors={errors} />
        <form className="space-y-4" noValidate onSubmit={(e) => void handleSubmit(e)}>
          <FormField label="Title" error={errors.title} required>
            <Input
              placeholder="Emergency fund"
              disabled={saving}
              className={fieldInvalidClass(errors.title)}
              value={form.title}
              onChange={(e) => {
                setForm({ ...form, title: e.target.value });
                clearError('title');
              }}
            />
          </FormField>
          <FormField label="Target amount" error={errors.targetAmount} required>
            <Input
              type="number"
              min="0"
              step="0.01"
              disabled={saving}
              className={fieldInvalidClass(errors.targetAmount)}
              value={form.targetAmount}
              onChange={(e) => {
                setForm({ ...form, targetAmount: e.target.value });
                clearError('targetAmount');
              }}
            />
          </FormField>
          {!goal && (
          <FormField label="Already saved (optional)" error={errors.currentAmount}>
            <Input
              type="number"
              min="0"
              step="0.01"
              disabled={saving}
              className={fieldInvalidClass(errors.currentAmount)}
              value={form.currentAmount}
              onChange={(e) => {
                setForm({ ...form, currentAmount: e.target.value });
                clearError('currentAmount');
              }}
            />
          </FormField>
          )}
          <FormField label="Allocation % of leftover pay" error={errors.allocationPercentage} required>
            <Input
              type="number"
              min="0"
              max="100"
              step="0.1"
              disabled={saving}
              className={fieldInvalidClass(errors.allocationPercentage)}
              value={form.allocationPercentage}
              onChange={(e) => {
                setForm({ ...form, allocationPercentage: e.target.value });
                clearError('allocationPercentage');
              }}
            />
          </FormField>
          <FormField label="Target date (optional)">
            <Input
              type="date"
              disabled={saving}
              value={form.targetDate}
              onChange={(e) => setForm({ ...form, targetDate: e.target.value })}
            />
          </FormField>
          <div className="flex flex-col-reverse gap-2 pt-2 sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" disabled={saving} onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              {saving ? 'Saving…' : goal ? 'Update goal' : 'Create goal'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
