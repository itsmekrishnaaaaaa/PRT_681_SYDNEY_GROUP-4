import { type FormEvent, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/client';
import type { RegisterPayload } from './api';
import { useAuth } from './AuthContext';
import { AustralianPhoneInput } from '@/auth/components/AustralianPhoneInput';
import {
  FieldError,
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  pickErrors,
  useFormValidation,
  validators,
} from '@/shared/form-field';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { cn } from '@/shared/lib/utils';
import { nationalityOptions } from '@/auth/data/nationalities';
import { identityDocumentOptions, type IdentityDocumentType } from '@/shared/types';
import { AU_PHONE_PREFIX, australianPhoneValidationError } from '@/auth/utils/australianPhone';
import { FileUp, ShieldCheck } from 'lucide-react';

const visaTypes = [
  'Subclass 500 – Student',
  'Subclass 482 – Temporary Skill Shortage',
  'Subclass 417/462 – Working Holiday',
  'Subclass 186/189 – Skilled (Permanent)',
  'Bridging visa',
  'Other',
];

const visaStatuses = [
  { value: 'Citizen', label: 'Australian citizen' },
  { value: 'TR', label: 'Permanent resident (TR)' },
  { value: 'Student', label: 'Temporary visa – Student' },
  { value: 'Other', label: 'Other temporary visa' },
];

const workingRightsOptions = [
  {
    value: 'Full Time',
    label: 'Full work rights',
    hint: 'Unlimited work hours (citizens, permanent residents, and most unrestricted visas).',
  },
  {
    value: 'Student',
    label: 'Student work limit',
    hint: 'Up to 48 hours per fortnight during study periods; unlimited during scheduled breaks (Home Affairs).',
  },
];

const allowedDocTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/webp'];

type RegisterFields =
  | 'firstName' | 'lastName' | 'email' | 'phoneNumber' | 'dateOfBirth' | 'visaType' | 'visaStatus'
  | 'workingRights' | 'visaExpiryDate' | 'identityDocumentType' | 'visaDocument' | 'password'
  | 'confirmPassword' | 'privacyConsent' | 'accurateDeclaration' | '_form';

const initialForm = {
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: AU_PHONE_PREFIX,
  dateOfBirth: '',
  nationality: 'Australian',
  visaType: '',
  visaStatus: '',
  workingRights: '',
  visaExpiryDate: '',
  identityDocumentType: '',
  password: '',
  confirmPassword: '',
};

function validateRegisterForm(
  form: typeof initialForm,
  visaDocument: File | null,
  privacyConsent: boolean,
  accurateDeclaration: boolean,
) {
  const needsExpiry = form.visaStatus && form.visaStatus !== 'Citizen';
  const studentRightsError =
    form.visaStatus === 'Student' && form.workingRights && form.workingRights !== 'Student'
      ? 'Student visa → choose Student work limit.'
      : undefined;

  let dateOfBirthError = validators.dateRequired(form.dateOfBirth, 'Date of birth is required.');
  if (!dateOfBirthError && form.dateOfBirth) {
    const age = new Date().getFullYear() - new Date(form.dateOfBirth).getFullYear();
    if (age < 15) dateOfBirthError = 'Must be at least 15 years old.';
  }

  let visaExpiryError: string | undefined;
  if (needsExpiry) {
    visaExpiryError = validators.dateRequired(form.visaExpiryDate, 'Visa expiry date is required.');
    if (!visaExpiryError && form.visaExpiryDate) {
      visaExpiryError = validators.futureDate(form.visaExpiryDate, 'Visa must not be expired.');
    }
  }

  return pickErrors<RegisterFields>([
    ['firstName', validators.required(form.firstName, 'First name is required.')],
    ['lastName', validators.required(form.lastName, 'Surname is required.')],
    ['email', validators.email(form.email, 'Valid email is required.')],
    ['phoneNumber', australianPhoneValidationError(form.phoneNumber)],
    ['dateOfBirth', dateOfBirthError],
    ['visaType', validators.required(form.visaType, 'Select a visa subclass.')],
    ['visaStatus', validators.required(form.visaStatus, 'Select residency status.')],
    ['workingRights', studentRightsError ?? validators.required(form.workingRights, 'Select work rights.')],
    ['visaExpiryDate', visaExpiryError],
    ['identityDocumentType', validators.required(form.identityDocumentType, 'Select document type.')],
    ['visaDocument', validators.fileRequired(visaDocument, 'Upload your ID document.')
      ?? validators.fileTypes(visaDocument, allowedDocTypes, 'Use PDF, JPG, PNG, or WEBP.')
      ?? validators.fileMaxSize(visaDocument, 5 * 1024 * 1024, 'File must be under 5 MB.')],
    ['password', validators.minLength(form.password, 8, 'At least 8 characters.')],
    ['confirmPassword', validators.match(form.password, form.confirmPassword, 'Passwords must match.')],
    ['privacyConsent', validators.checked(privacyConsent, 'Required — tick privacy consent.')],
    ['accurateDeclaration', validators.checked(accurateDeclaration, 'Required — tick the declaration.')],
  ]);
}

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [visaDocument, setVisaDocument] = useState<File | null>(null);
  const [privacyConsent, setPrivacyConsent] = useState(false);
  const [accurateDeclaration, setAccurateDeclaration] = useState(false);
  const { errors, clearError, validate, setFormError } = useFormValidation<RegisterFields>();
  const [success, setSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const docLabel = useMemo(() => visaDocument?.name ?? 'No file chosen', [visaDocument]);
  const workingRightsHint = useMemo(
    () => workingRightsOptions.find((o) => o.value === form.workingRights)?.hint ?? 'Select work rights per your visa conditions.',
    [form.workingRights],
  );
  const docHint = useMemo(
    () => identityDocumentOptions.find((o) => o.value === form.identityDocumentType)?.hint ?? 'Choose a 100-point ID document and upload a clear PDF or photo.',
    [form.identityDocumentType],
  );

  const handleVisaStatusChange = (visaStatus: string) => {
    const workingRights =
      visaStatus === 'Student' ? 'Student' :
      visaStatus === 'Citizen' || visaStatus === 'TR' ? 'Full Time' :
      form.workingRights;
    setForm({ ...form, visaStatus, workingRights });
    clearError('visaStatus');
    clearError('workingRights');
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSuccess('');
    if (!validate(() => validateRegisterForm(form, visaDocument, privacyConsent, accurateDeclaration))) {
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    setSubmitting(true);
    try {
      const payload: RegisterPayload = {
        ...form,
        visaExpiryDate: form.visaExpiryDate || (form.visaStatus === 'Citizen' ? '2099-12-31' : form.visaExpiryDate),
        identityDocumentType: form.identityDocumentType as IdentityDocumentType,
        visaDocument: visaDocument!,
      };
      const res = await register(payload);
      setSuccess(res.message);
      setTimeout(() => navigate('/login'), 2500);
    } catch (err) {
      setFormError(getErrorMessage(err, 'Registration failed'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="grid min-h-dvh place-items-center bg-background p-3 sm:p-4">
      <Card className="w-full max-w-2xl border-t-4 border-t-foreground">
        <CardHeader className="space-y-3">
          <div className="flex items-center gap-3">
            <span className="text-3xl leading-none" aria-hidden>🇦🇺</span>
            <div>
              <CardTitle className="flex items-center gap-2 text-foreground">
                <ShieldCheck className="h-5 w-5" /> Register — Australia
              </CardTitle>
              <p className="text-xs text-muted-foreground mt-0.5">OraEarn · Identity verification required</p>            </div>
          </div>
          <CardDescription>
            Provide accurate details under Australian identity rules (100-point ID check). Your account needs manual admin approval before you can sign in.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <FormErrorSummary errors={errors} />
          {success && <p className="mb-4 text-sm text-foreground border border-border bg-muted rounded-md p-3">{success}</p>}

          <form className="space-y-6" noValidate onSubmit={handleSubmit}>
            <section className="space-y-4">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-foreground">Personal details</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Legal first name" error={errors.firstName} required>
                  <Input className={fieldInvalidClass(errors.firstName)} value={form.firstName} onChange={(e) => { setForm({ ...form, firstName: e.target.value }); clearError('firstName'); }} autoComplete="given-name" />
                </FormField>
                <FormField label="Legal surname" error={errors.lastName} required>
                  <Input className={fieldInvalidClass(errors.lastName)} value={form.lastName} onChange={(e) => { setForm({ ...form, lastName: e.target.value }); clearError('lastName'); }} autoComplete="family-name" />
                </FormField>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Email address" error={errors.email} required>
                  <Input type="email" className={fieldInvalidClass(errors.email)} value={form.email} onChange={(e) => { setForm({ ...form, email: e.target.value }); clearError('email'); }} autoComplete="email" />
                </FormField>
                <FormField label="Australian mobile" required>
                  <AustralianPhoneInput value={form.phoneNumber} onChange={(v) => { setForm({ ...form, phoneNumber: v }); clearError('phoneNumber'); }} error={errors.phoneNumber} />
                </FormField>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Date of birth" error={errors.dateOfBirth} required>
                  <Input type="date" className={fieldInvalidClass(errors.dateOfBirth)} value={form.dateOfBirth} onChange={(e) => { setForm({ ...form, dateOfBirth: e.target.value }); clearError('dateOfBirth'); }} />
                </FormField>
                <FormField label={<>Nationality <span aria-hidden>🇦🇺</span></>} required>
                  <Select value={form.nationality} onValueChange={(v) => setForm({ ...form, nationality: v })}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>{nationalityOptions.map((n) => <SelectItem key={n} value={n}>{n}</SelectItem>)}</SelectContent>
                  </Select>
                </FormField>
              </div>
            </section>

            <section className="space-y-4 rounded-lg border p-4 bg-muted/30">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-foreground">Visa & work rights</h2>
              <p className="text-xs text-muted-foreground">Per Department of Home Affairs and Fair Work Act requirements.</p>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Visa subclass / type" error={errors.visaType} required>
                  <Select value={form.visaType || undefined} onValueChange={(v) => { setForm({ ...form, visaType: v }); clearError('visaType'); }}>
                    <SelectTrigger className={fieldInvalidClass(errors.visaType)}><SelectValue placeholder="Select visa type" /></SelectTrigger>
                    <SelectContent>{visaTypes.map((t) => <SelectItem key={t} value={t}>{t}</SelectItem>)}</SelectContent>
                  </Select>
                </FormField>
                <FormField label="Residency status" error={errors.visaStatus} required>
                  <Select value={form.visaStatus || undefined} onValueChange={handleVisaStatusChange}>
                    <SelectTrigger className={fieldInvalidClass(errors.visaStatus)}><SelectValue placeholder="Citizen, PR, Student…" /></SelectTrigger>
                    <SelectContent>
                      {visaStatuses.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
                    </SelectContent>
                  </Select>
                </FormField>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Work rights in Australia" error={errors.workingRights} hint={workingRightsHint} required>
                  <Select value={form.workingRights || undefined} onValueChange={(v) => { setForm({ ...form, workingRights: v }); clearError('workingRights'); }}>
                    <SelectTrigger className={fieldInvalidClass(errors.workingRights)}><SelectValue placeholder="Full or student limit" /></SelectTrigger>
                    <SelectContent>
                      {workingRightsOptions.map((o) => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                    </SelectContent>
                  </Select>
                </FormField>
                <FormField
                  label={form.visaStatus === 'Citizen' ? 'Visa expiry (not required for citizens)' : 'Visa expiry'}
                  error={errors.visaExpiryDate}
                  required={form.visaStatus !== 'Citizen'}
                >
                  <Input type="date" className={fieldInvalidClass(errors.visaExpiryDate)} value={form.visaExpiryDate} onChange={(e) => { setForm({ ...form, visaExpiryDate: e.target.value }); clearError('visaExpiryDate'); }} disabled={form.visaStatus === 'Citizen'} />
                </FormField>
              </div>
            </section>

            <section className="space-y-4 rounded-lg border p-4 bg-muted/20">
              <h2 className="text-sm font-semibold uppercase tracking-wide">100-point identity check</h2>
              <p className="text-xs text-muted-foreground">Upload one primary ID document. Admin will verify before approval.</p>
              <FormField label="Document type" error={errors.identityDocumentType} required>
                <Select value={form.identityDocumentType || undefined} onValueChange={(v) => { setForm({ ...form, identityDocumentType: v }); clearError('identityDocumentType'); }}>
                  <SelectTrigger className={fieldInvalidClass(errors.identityDocumentType)}><SelectValue placeholder="Passport, licence, or visa notice" /></SelectTrigger>
                  <SelectContent>
                    {identityDocumentOptions.map((o) => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                  </SelectContent>
                </Select>
              </FormField>
              <FormField label="Upload document" error={errors.visaDocument} hint={docHint} required>
                <Input type="file" className={fieldInvalidClass(errors.visaDocument)} accept=".pdf,.jpg,.jpeg,.png,.webp,application/pdf,image/*" onChange={(e) => { setVisaDocument(e.target.files?.[0] ?? null); clearError('visaDocument'); }} />
                <p className="text-xs text-muted-foreground flex items-center gap-1 pt-1"><FileUp className="h-3 w-3" /> {docLabel} · max 5 MB</p>
              </FormField>
            </section>

            <section className="space-y-4">
              <h2 className="text-sm font-semibold uppercase tracking-wide">Account security</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <FormField label="Password" error={errors.password} required>
                  <Input type="password" className={fieldInvalidClass(errors.password)} value={form.password} onChange={(e) => { setForm({ ...form, password: e.target.value }); clearError('password'); }} autoComplete="new-password" />
                </FormField>
                <FormField label="Confirm password" error={errors.confirmPassword} required>
                  <Input type="password" className={fieldInvalidClass(errors.confirmPassword)} value={form.confirmPassword} onChange={(e) => { setForm({ ...form, confirmPassword: e.target.value }); clearError('confirmPassword'); }} autoComplete="new-password" />
                </FormField>
              </div>
            </section>

            <section className="space-y-3 rounded-lg border bg-muted p-4 text-sm">
              <h2 className="font-semibold">Privacy & declaration (Australian Privacy Act 1988)</h2>
              <p className="text-xs text-muted-foreground">
                We collect your personal information to verify identity and work eligibility. It is stored securely and used only for account approval and shift tracking. You may request access or correction of your data.
              </p>
              <div>
                <label className={cn('flex items-start gap-2 cursor-pointer rounded p-1', errors.privacyConsent && 'ring-1 ring-destructive')}>
                  <input type="checkbox" className="mt-1" checked={privacyConsent} onChange={(e) => { setPrivacyConsent(e.target.checked); clearError('privacyConsent'); }} />
                  <span className="text-xs">I consent to the collection of my personal information as described above. *</span>
                </label>
                <FieldError message={errors.privacyConsent} />
              </div>
              <div>
                <label className={cn('flex items-start gap-2 cursor-pointer rounded p-1', errors.accurateDeclaration && 'ring-1 ring-destructive')}>
                  <input type="checkbox" className="mt-1" checked={accurateDeclaration} onChange={(e) => { setAccurateDeclaration(e.target.checked); clearError('accurateDeclaration'); }} />
                  <span className="text-xs">I declare that the information provided is true and correct. *</span>
                </label>
                <FieldError message={errors.accurateDeclaration} />
              </div>
            </section>

            <Button className="w-full" type="submit" disabled={submitting}>
              {submitting ? 'Submitting…' : 'Submit for admin approval'}
            </Button>
          </form>
          <p className="mt-4 text-sm text-muted-foreground text-center">
            Already registered? <Link className="text-foreground underline font-medium" to="/login">Sign in</Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
