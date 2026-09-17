import { cn } from './cn';

type FormErrorSummaryProps = {
  errors: Record<string, string | undefined>;
  /** Key used for server / form-level errors (default `_form`) */
  formErrorKey?: string;
  className?: string;
};

/** Optional list of all field errors at the top of a form. */
export function FormErrorSummary({
  errors,
  formErrorKey = '_form',
  className,
}: FormErrorSummaryProps) {
  const formError = errors[formErrorKey];
  const fieldErrors = Object.entries(errors).filter(
    ([key, msg]) => key !== formErrorKey && msg,
  );

  if (formError) {
    return (
      <p
        role="alert"
        className={cn(
          'mb-4 text-sm text-destructive border border-destructive/30 rounded-md p-3',
          className,
        )}
      >
        {formError}
      </p>
    );
  }

  if (fieldErrors.length === 0) return null;

  return (
    <div
      role="alert"
      className={cn(
        'mb-4 text-sm text-destructive border border-destructive/30 rounded-md p-3 space-y-1',
        className,
      )}
    >
      <p className="font-medium">
        Please fix {fieldErrors.length} issue{fieldErrors.length > 1 ? 's' : ''} below:
      </p>
      <ul className="list-disc pl-5 text-xs">
        {fieldErrors.map(([key, msg]) => (
          <li key={key}>{msg}</li>
        ))}
      </ul>
    </div>
  );
}
