import { type FormEvent, useEffect, useState } from 'react';
import type { Job, Shift } from '@/shared/types';
import {
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  useFormValidation,
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
import { validateShiftForm } from '@/features/shifts/utils/shiftForm';
import { Loader2 } from 'lucide-react';

export type ShiftFormValues = {
  jobId: string;
  workDate: string;
  startTime: string;
  endTime: string;
  breakMinutes: string;
};

type ShiftFields = 'jobId' | 'workDate' | 'startTime' | 'endTime' | 'breakMinutes' | '_form';

type ShiftModalProps = {
  open: boolean;
  selectedDate: Date | null;
  shift: Shift | null;
  jobs: Job[];
  saving: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (values: ShiftFormValues) => Promise<void>;
  onDelete?: () => Promise<void>;
};

const formatDateInput = (date: Date) => {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
};

const defaultValues = (date: Date, jobs: Job[]): ShiftFormValues => ({
  jobId: jobs[0]?.id ?? '',
  workDate: formatDateInput(date),
  startTime: '09:00',
  endTime: '17:00',
  breakMinutes: '30',
});

const valuesFromShift = (shift: Shift): ShiftFormValues => ({
  jobId: shift.jobId,
  workDate: shift.workDate.slice(0, 10),
  startTime: shift.startTime.slice(0, 5),
  endTime: shift.endTime.slice(0, 5),
  breakMinutes: String(shift.breakMinutes),
});

export function ShiftModal({
  open,
  selectedDate,
  shift,
  jobs,
  saving,
  error,
  onClose,
  onSubmit,
  onDelete,
}: ShiftModalProps) {
  const [form, setForm] = useState<ShiftFormValues>(() => defaultValues(selectedDate ?? new Date(), jobs));
  const { errors, clearError, validate, setErrors, clearAll } = useFormValidation<ShiftFields>();

  useEffect(() => {
    if (!open) return;
    setForm(shift ? valuesFromShift(shift) : defaultValues(selectedDate ?? new Date(), jobs));
    clearAll();
  }, [open, shift, selectedDate, jobs, clearAll]);

  useEffect(() => {
    if (open && error) {
      setErrors((prev) => ({ ...prev, _form: error }));
    }
  }, [error, open, setErrors]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const fieldErrors = validateShiftForm(form);
    if (!validate(() => fieldErrors)) return;

    try {
      clearAll();
      await onSubmit(form);
    } catch {
      // Parent sets error string; kept for safety if onSubmit throws
    }
  };

  const canSave = jobs.length > 0 && form.jobId && !saving;

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && !saving && onClose()}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{shift ? 'Edit shift' : 'Add shift'}</DialogTitle>
          <DialogDescription>
            {selectedDate?.toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}
          </DialogDescription>
        </DialogHeader>

        {jobs.length === 0 && (
          <p className="text-sm text-destructive">Create a job first on the Jobs page.</p>
        )}
        <FormErrorSummary errors={errors} />

        <form className="space-y-4" noValidate onSubmit={(e) => void handleSubmit(e)}>
          <FormField label="Job" error={errors.jobId} required>
            <Select
              value={form.jobId || undefined}
              onValueChange={(jobId) => {
                setForm({ ...form, jobId });
                clearError('jobId');
              }}
              disabled={jobs.length === 0 || saving}
            >
              <SelectTrigger className={fieldInvalidClass(errors.jobId)}><SelectValue placeholder="Select job" /></SelectTrigger>
              <SelectContent>
                {jobs.map((job) => (
                  <SelectItem key={job.id} value={job.id}>{job.title} (${job.hourlyRate}/hr)</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>

          <FormField label="Work date" htmlFor="workDate" error={errors.workDate} required>
            <Input
              id="workDate"
              type="date"
              disabled={saving}
              className={fieldInvalidClass(errors.workDate)}
              value={form.workDate}
              onChange={(e) => {
                setForm({ ...form, workDate: e.target.value });
                clearError('workDate');
              }}
            />
          </FormField>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Start" htmlFor="startTime" error={errors.startTime} required hint="Overnight? Set end before start (e.g. 22:00 → 06:00)">
              <Input
                id="startTime"
                type="time"
                disabled={saving}
                className={fieldInvalidClass(errors.startTime)}
                value={form.startTime}
                onChange={(e) => {
                  setForm({ ...form, startTime: e.target.value });
                  clearError('startTime');
                  clearError('endTime');
                }}
              />
            </FormField>
            <FormField label="End" htmlFor="endTime" error={errors.endTime} required>
              <Input
                id="endTime"
                type="time"
                disabled={saving}
                className={fieldInvalidClass(errors.endTime)}
                value={form.endTime}
                onChange={(e) => {
                  setForm({ ...form, endTime: e.target.value });
                  clearError('endTime');
                }}
              />
            </FormField>
          </div>

          <FormField label="Break (minutes)" htmlFor="break" error={errors.breakMinutes} required>
            <Input
              id="break"
              type="number"
              min="0"
              disabled={saving}
              className={fieldInvalidClass(errors.breakMinutes)}
              value={form.breakMinutes}
              onChange={(e) => {
                setForm({ ...form, breakMinutes: e.target.value });
                clearError('breakMinutes');
              }}
            />
          </FormField>

          <div className="flex flex-wrap justify-end gap-2 pt-2">
            {shift && onDelete && (
              <Button type="button" variant="destructive" disabled={saving} onClick={() => void onDelete()}>
                Delete
              </Button>
            )}
            <Button type="button" variant="outline" disabled={saving} onClick={onClose}>Cancel</Button>
            <Button type="submit" disabled={!canSave} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              {saving ? 'Saving…' : shift ? 'Update shift' : 'Save shift'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
