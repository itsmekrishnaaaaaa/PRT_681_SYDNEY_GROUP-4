import type { ShiftFormValues } from '@/features/shifts/components/ShiftModal';
import type { ShiftInput } from '@/features/shifts/api';

/** Minutes from midnight for HH:mm or HH:mm:ss strings. */
export function parseTimeToMinutes(value: string): number {
  const [h, m] = value.slice(0, 5).split(':').map(Number);
  return h * 60 + m;
}

/** True when end is after start on the same day, or end is on the next day (overnight). */
export function isValidShiftTimeRange(startTime: string, endTime: string): boolean {
  if (!startTime || !endTime) return false;
  const start = parseTimeToMinutes(startTime);
  const end = parseTimeToMinutes(endTime);
  return start !== end;
}

export function toShiftPayload(values: ShiftFormValues): ShiftInput {
  const normalizeTime = (t: string) => (t.length === 5 ? `${t}:00` : t.slice(0, 8));
  const breakMinutes = Number.parseInt(values.breakMinutes, 10);

  return {
    jobId: values.jobId,
    workDate: values.workDate,
    startTime: normalizeTime(values.startTime),
    endTime: normalizeTime(values.endTime),
    breakMinutes: Number.isNaN(breakMinutes) ? 0 : Math.max(0, breakMinutes),
  };
}

export function validateShiftForm(form: ShiftFormValues): Partial<Record<string, string>> {
  const errors: Partial<Record<string, string>> = {};

  if (!form.jobId?.trim()) errors.jobId = 'Select a job.';
  if (!form.workDate) errors.workDate = 'Work date is required.';
  if (!form.startTime) errors.startTime = 'Start time is required.';
  if (!form.endTime) errors.endTime = 'End time is required.';
  else if (!isValidShiftTimeRange(form.startTime, form.endTime)) {
    errors.endTime = 'End time must be different from start time.';
  }

  const breakMinutes = Number.parseInt(form.breakMinutes, 10);
  if (Number.isNaN(breakMinutes) || breakMinutes < 0) {
    errors.breakMinutes = 'Break cannot be negative.';
  }

  return errors;
}
