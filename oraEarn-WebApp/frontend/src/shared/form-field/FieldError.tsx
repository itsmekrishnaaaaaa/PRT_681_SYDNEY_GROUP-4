import { cn } from './cn';

type FieldErrorProps = {
  message?: string;
  className?: string;
};

/** Validation message shown directly below an input. */
export function FieldError({ message, className }: FieldErrorProps) {
  if (!message) return null;
  return (
    <p role="alert" className={cn('text-xs font-medium text-destructive', className)}>
      {message}
    </p>
  );
}
