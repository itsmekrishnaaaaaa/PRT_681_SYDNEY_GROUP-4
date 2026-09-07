import { type FormEvent, useState } from 'react';
import { Link } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/client';
import { useAuth } from './AuthContext';
import {
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
import { cn } from '@/shared/lib/utils';
import { Briefcase, CalendarDays, Lock, LogIn, Mail, PiggyBank, Receipt, Sparkles } from 'lucide-react';

type LoginFields = 'email' | 'password' | '_form';

const features = [
  { icon: CalendarDays, text: 'Track shifts & visa hours' },
  { icon: Receipt, text: 'Smart expense tracking' },
  { icon: PiggyBank, text: 'Savings goals with Aura AI' },
  { icon: Briefcase, text: 'Multi-job pay calculator' },
];

export function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { errors, clearError, validate, setFormError } = useFormValidation<LoginFields>();
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['email', validators.email(email)],
          ['password', validators.required(password, 'Password is required.')],
        ]),
      )
    ) {
      return;
    }

    setSubmitting(true);
    try {
      await login(email, password);
    } catch (err) {
      setFormError(getErrorMessage(err, 'Login failed'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen grid lg:grid-cols-2">
      <div className="hidden flex-col justify-between bg-gradient-to-br from-violet-600 to-indigo-800 p-10 text-white lg:flex">
        <div>
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-white/15 font-bold text-lg">OE</div>
            <div>
              <p className="font-bold text-xl">OraEarn</p>
              <p className="flex items-center gap-1 text-sm text-white/80"><Sparkles className="h-3.5 w-3.5" /> Earn smarter in Australia</p>
            </div>
          </div>
          <h2 className="mt-12 text-3xl font-bold leading-tight">Your shift money,<br />under control.</h2>
          <p className="mt-4 max-w-md text-white/80 leading-relaxed">
            Built for students and workers — track hours, stay visa-safe, manage expenses, and hit savings goals with Aura AI by your side.
          </p>
        </div>
        <ul className="grid grid-cols-2 gap-3">
          {features.map(({ icon: Icon, text }) => (
            <li key={text} className="flex items-center gap-2 rounded-lg bg-white/10 px-3 py-2.5 text-sm backdrop-blur">
              <Icon className="h-4 w-4 shrink-0" />
              {text}
            </li>
          ))}
        </ul>
      </div>

      <div className="grid place-items-center p-6">
        <Card className="w-full max-w-md border-0 shadow-xl lg:border">
          <CardHeader className="text-center lg:text-left">
            <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-violet-600 to-indigo-600 text-white lg:mx-0">
              <LogIn className="h-6 w-6" />
            </div>
            <CardTitle className="text-2xl">Welcome back</CardTitle>
            <CardDescription>Sign in to your OraEarn account</CardDescription>
          </CardHeader>
          <CardContent>
            <FormErrorSummary errors={errors} />
            <form className="space-y-4" noValidate onSubmit={handleSubmit}>
              <FormField label="Email" htmlFor="email" error={errors.email} required>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="email"
                    type="email"
                    className={cn('pl-9', fieldInvalidClass(errors.email))}
                    value={email}
                    onChange={(e) => {
                      setEmail(e.target.value);
                      clearError('email');
                    }}
                    autoComplete="email"
                    placeholder="you@example.com"
                  />
                </div>
              </FormField>
              <FormField label="Password" htmlFor="password" error={errors.password} required>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="password"
                    type="password"
                    className={cn('pl-9', fieldInvalidClass(errors.password))}
                    value={password}
                    onChange={(e) => {
                      setPassword(e.target.value);
                      clearError('password');
                    }}
                    autoComplete="current-password"
                  />
                </div>
              </FormField>
              <Button className="w-full gap-2" type="submit" disabled={submitting}>
                <LogIn className="h-4 w-4" />
                {submitting ? 'Signing in…' : 'Sign in'}
              </Button>
            </form>
            <p className="mt-6 text-center text-sm text-muted-foreground">
              New here?{' '}
              <Link className="font-medium text-violet-600 underline underline-offset-2" to="/register">
                Create an account
              </Link>
            </p>
            {import.meta.env.DEV && (
              <p className="mt-3 text-center text-muted-foreground text-xs">
                Local demo: alice@oraearn.local / User123!
              </p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
