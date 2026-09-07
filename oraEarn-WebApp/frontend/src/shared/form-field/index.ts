/**
 * Portable form validation kit — copy the entire `form-field/` folder into any React project.
 *
 * Usage:
 *   const { errors, clearError, validate, setFormError } = useFormValidation<'email' | 'password'>();
 *
 *   const ok = validate(() => pickErrors([
 *     ['email', validators.email(form.email)],
 *     ['password', validators.required(form.password)],
 *   ]));
 *
 *   <form noValidate onSubmit={(e) => { e.preventDefault(); if (!ok) return; ... }}>
 *     <FormErrorSummary errors={errors} />
 *     <FormField label="Email" error={errors.email} required>
 *       <Input className={fieldInvalidClass(errors.email)} ... />
 *     </FormField>
 *   </form>
 */
export { FieldError } from './FieldError';
export { FormField, fieldInvalidClass } from './FormField';
export type { FormFieldProps } from './FormField';
export { FormErrorSummary } from './FormErrorSummary';
export { useFormValidation } from './useFormValidation';
export type { FieldErrors } from './useFormValidation';
export { validators, pickErrors } from './validators';
