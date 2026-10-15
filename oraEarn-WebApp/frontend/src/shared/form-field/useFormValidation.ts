import { useCallback, useMemo, useState } from 'react';

export type FieldErrors<T extends string = string> = Partial<Record<T, string>>;

/**
 * Lightweight form validation state — no external deps.
 * Pair with `validators` and `FormField` for per-input errors below each control.
 */
export function useFormValidation<T extends string = string>() {
  const [errors, setErrors] = useState<FieldErrors<T>>({});

  const clearError = useCallback((key: T) => {
    setErrors((current) => {
      const next = { ...current };
      delete next[key];
      return next;
    });
  }, []);

  const clearAll = useCallback(() => setErrors({}), []);

  // Replaces all field errors with one form-level message (e.g. a server error), shown by FormErrorSummary.
  const setFormError = useCallback((message: string, formErrorKey = '_form' as T) => {
    setErrors({ [formErrorKey]: message } as FieldErrors<T>);
  }, []);

  const activeErrors = useMemo(
    () => Object.entries(errors).filter(([, msg]) => msg) as Array<[T, string]>,
    [errors],
  );

  const errorCount = activeErrors.length;
  const hasErrors = errorCount > 0;

  /** Run validator, store errors, return true when the form is valid. */
  // validate() replaces the whole error map, so errors fixed since the last submit disappear.
  const validate = useCallback((run: () => FieldErrors<T>) => {
    const next = run();
    setErrors(next);
    return Object.keys(next).length === 0;
  }, []);

  return {
    errors,
    setErrors,
    clearError,
    clearAll,
    setFormError,
    activeErrors,
    errorCount,
    hasErrors,
    validate,
  };
}
