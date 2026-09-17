import { type FormEvent, useEffect, useState } from 'react';
import type { Job } from '@/shared/types';
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

type JobFields = 'title' | 'hourlyRate' | 'dateOfJoin' | '_form';

const blankForm = () => ({
  title: '',
  hourlyRate: '25',
  dateOfJoin: new Date().toISOString().slice(0, 10),
});

type JobModalProps = {
  open: boolean;
  job: Job | null;
  saving: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (values: { title: string; hourlyRate: number; dateOfJoin: string }) => Promise<void>;
};

export function JobModal({ open, job, saving, error, onClose, onSubmit }: JobModalProps) {
  const [form, setForm] = useState(blankForm);
  const { errors, clearError, validate, setErrors, clearAll } = useFormValidation<JobFields>();

  useEffect(() => {
    if (!open) return;
    setForm(
      job
        ? { title: job.title, hourlyRate: String(job.hourlyRate), dateOfJoin: job.dateOfJoin.slice(0, 10) }
        : blankForm(),
    );
    clearAll();
  }, [open, job, clearAll]);

  useEffect(() => {
    if (open && error) setErrors((prev) => ({ ...prev, _form: error }));
  }, [error, open, setErrors]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['title', validators.required(form.title, 'Job title is required.')],
          ['hourlyRate', validators.minNumber(form.hourlyRate, 0.01, 'Enter a valid hourly rate.')],
          ['dateOfJoin', validators.dateRequired(form.dateOfJoin, 'Date of join is required.')],
        ]),
      )
    ) {
      return;
    }
    await onSubmit({ title: form.title, hourlyRate: Number(form.hourlyRate), dateOfJoin: form.dateOfJoin });
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && !saving && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{job ? 'Edit job' : 'Add a job'}</DialogTitle>
          <DialogDescription>Job title, pay rate, and start date — used to calculate shift pay.</DialogDescription>
        </DialogHeader>
        <FormErrorSummary errors={errors} />
        <form className="space-y-4" noValidate onSubmit={(e) => void handleSubmit(e)}>
          <FormField label="Job title" error={errors.title} required hint="e.g. Café barista, Warehouse picker">
            <Input
              placeholder="Where do you work?"
              disabled={saving}
              className={fieldInvalidClass(errors.title)}
              value={form.title}
              onChange={(e) => {
                setForm({ ...form, title: e.target.value });
                clearError('title');
              }}
            />
          </FormField>
          <FormField label="Hourly rate ($)" error={errors.hourlyRate} required hint="Before tax">
            <Input
              type="number"
              min="0"
              step="0.01"
              disabled={saving}
              className={fieldInvalidClass(errors.hourlyRate)}
              value={form.hourlyRate}
              onChange={(e) => {
                setForm({ ...form, hourlyRate: e.target.value });
                clearError('hourlyRate');
              }}
            />
          </FormField>
          <FormField label="Start date" error={errors.dateOfJoin} required>
            <Input
              type="date"
              disabled={saving}
              className={fieldInvalidClass(errors.dateOfJoin)}
              value={form.dateOfJoin}
              onChange={(e) => {
                setForm({ ...form, dateOfJoin: e.target.value });
                clearError('dateOfJoin');
              }}
            />
          </FormField>
          <div className="flex flex-col-reverse gap-2 pt-2 sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" disabled={saving} onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              {saving ? 'Saving…' : job ? 'Update job' : 'Save job'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
