import type { ReactNode } from 'react';
import { FieldError } from './FieldError';
import { cn } from './cn';

export type FormFieldProps = {
  /** Field label shown above the control */
  label?: ReactNode;
  /** Links label to control — pass the input's id */
  htmlFor?: string;
  /** Validation message — rendered below the control */
  error?: string;
  /** Helper text when there is no error */
  hint?: string;
  required?: boolean;
  className?: string;
  children: ReactNode;
};

/** Red border class — pass to Input / SelectTrigger when the field has an error. */
export function fieldInvalidClass(error?: string) {
  return error ? 'border-destructive focus-visible:ring-destructive/30' : '';
}

/**
 * Wraps any input control with label + error below.
 * Copy `components/form-field/` into another project and use with your own inputs.
 */
export function FormField({
  label,
  htmlFor,
  error,
  hint,
  required,
  className,
  children,
}: FormFieldProps) {
  return (
    <div className={cn('space-y-2', className)}>
      {label && (
        <label
          htmlFor={htmlFor}
          className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
        >
          {label}
          {required && <span className="text-destructive ml-0.5">*</span>}
        </label>
      )}
      {children}
      <FieldError message={error} />
      {!error && hint && <p className="text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}
