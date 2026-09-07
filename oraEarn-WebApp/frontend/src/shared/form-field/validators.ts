/** Small validation helpers — return an error string or undefined when valid. */

export const validators = {
  required(value: string, message = 'This field is required.') {
    return value.trim() ? undefined : message;
  },

  email(value: string, message = 'Enter a valid email address.') {
    if (!value.trim()) return message;
    return value.includes('@') && value.includes('.') ? undefined : message;
  },

  minLength(value: string, min: number, message?: string) {
    return value.length >= min ? undefined : message ?? `At least ${min} characters.`;
  },

  match(a: string, b: string, message = 'Values must match.') {
    return a === b ? undefined : message;
  },

  minNumber(value: string | number, min: number, message?: string) {
    const n = typeof value === 'number' ? value : Number(value);
    if (Number.isNaN(n)) return message ?? 'Enter a valid number.';
    return n >= min ? undefined : message ?? `Must be at least ${min}.`;
  },

  maxNumber(value: string | number, max: number, message?: string) {
    const n = typeof value === 'number' ? value : Number(value);
    if (Number.isNaN(n)) return message ?? 'Enter a valid number.';
    return n <= max ? undefined : message ?? `Must be at most ${max}.`;
  },

  dateRequired(value: string, message = 'Date is required.') {
    return value ? undefined : message;
  },

  futureDate(value: string, message = 'Date must be in the future.') {
    if (!value) return message;
    return value > new Date().toISOString().slice(0, 10) ? undefined : message;
  },

  fileRequired(file: File | null | undefined, message = 'Choose a file.') {
    return file ? undefined : message;
  },

  fileTypes(file: File | null | undefined, types: string[], message?: string) {
    if (!file) return undefined;
    return types.includes(file.type)
      ? undefined
      : message ?? `Allowed types: ${types.join(', ')}.`;
  },

  fileMaxSize(file: File | null | undefined, bytes: number, message?: string) {
    if (!file) return undefined;
    return file.size <= bytes
      ? undefined
      : message ?? `File must be under ${Math.round(bytes / 1024 / 1024)} MB.`;
  },

  checked(value: boolean, message = 'This field is required.') {
    return value ? undefined : message;
  },
};

/** Collect only defined error strings into a FieldErrors object. */
export function pickErrors<T extends string>(
  entries: Array<[T, string | undefined]>,
): Partial<Record<T, string>> {
  const out: Partial<Record<T, string>> = {};
  for (const [key, msg] of entries) {
    if (msg) out[key] = msg;
  }
  return out;
}
