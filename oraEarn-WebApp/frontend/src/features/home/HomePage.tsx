import { useEffect, useState, type ComponentType } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowRight,
  BarChart3,
  Briefcase,
  CalendarDays,
  Clock,
  FileSpreadsheet,
  FileText,
  LayoutDashboard,
  LogIn,
  Menu,
  MessageCircle,
  PiggyBank,
  Receipt,
  ShieldCheck,
  Sparkles,
  UserPlus,
  X,
} from 'lucide-react';
import { useAuth } from '@/auth';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';

const nav = [
  { href: '#features', label: 'Features', short: 'Features', icon: Sparkles },
  { href: '#how', label: 'How it works', short: 'How', icon: ArrowRight },
  { href: '#visa', label: 'Visa hours', short: 'Visa', icon: Clock },
  { href: '#aura', label: 'Aura', short: 'Aura', icon: MessageCircle },
  { href: '#about', label: 'About', short: 'About', icon: FileText },
];

const heroChips: { icon: ComponentType<{ className?: string }>; label: string }[] = [
  { icon: Briefcase, label: 'Jobs & pay rates' },
  { icon: CalendarDays, label: 'Shift calendar + clock-in' },
  { icon: Clock, label: '48-hour visa bar' },
  { icon: Receipt, label: 'Expenses & savings' },
  { icon: FileSpreadsheet, label: 'Analytics export' },
  { icon: MessageCircle, label: 'Aura chat helper' },
];

const features = [
  {
    icon: Briefcase,
    title: 'Jobs',
    text: 'Add every employer and hourly rate. Cafe, retail, or two jobs at once — pay is calculated from the job, not typed in by hand.',
  },
  {
    icon: CalendarDays,
    title: 'Shifts',
    text: 'Log a shift on the calendar or clock in live. Breaks, hours this month, and pay sit on one screen.',
  },
  {
    icon: Clock,
    title: 'Visa 48-hour bar',
    text: 'Student visa fortnight hours are added up for you. A save that would go over 48 hours is blocked.',
  },
  {
    icon: Receipt,
    title: 'Expenses',
    text: 'Record rent, food and travel in categories. Add and edit in a pop-out so the list stays short.',
  },
  {
    icon: PiggyBank,
    title: 'Savings goals',
    text: 'Set a target and a % of leftover pay. Shift pay and expenses update the saved amount automatically.',
  },
  {
    icon: BarChart3,
    title: 'Dashboard',
    text: 'Net balance, earnings, spend, savings and fortnight hours. Filter 7 days, a month or a year.',
  },
  {
    icon: FileSpreadsheet,
    title: 'Excel and PDF',
    text: 'Export the same numbers you see on the dashboard. Columns match the tables, not extra made-up fields.',
  },
  {
    icon: MessageCircle,
    title: 'Aura chat',
    text: 'A small helper for hours, savings and visa limits. It sits beside the app; it is not the whole product.',
  },
  {
    icon: ShieldCheck,
    title: 'Register and admin',
    text: 'Sign up with visa details and an ID file. An admin reviews pending accounts before you can use the app.',
  },
];

const steps = [
  { icon: UserPlus, title: 'Create an account', text: 'Register with your visa type and upload ID. Wait for approval, then sign in.' },
  { icon: Briefcase, title: 'Add a job', text: 'Name the employer and the hourly rate. Every shift uses that rate.' },
  { icon: CalendarDays, title: 'Log shifts', text: 'Use the calendar or clock in. The visa bar updates as you save.' },
  { icon: LayoutDashboard, title: 'Track money', text: 'Log expenses and savings goals. Leftover shift pay fills each goal by its allocation %.' },
];

function scrollToId(id: string) {
  const el = document.querySelector(id);
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

export function HomePage() {
  const { user, isAdmin } = useAuth();
  const [open, setOpen] = useState(false);
  const appPath = user ? (isAdmin ? '/admin' : '/dashboard') : '/login';

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false);
    };
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', onKey);
    return () => {
      document.body.style.overflow = '';
      window.removeEventListener('keydown', onKey);
    };
  }, [open]);

  const onNav = (href: string) => {
    setOpen(false);
    scrollToId(href);
  };

  return (
    <div id="top" className="min-h-dvh overflow-x-hidden bg-background text-foreground">
      <header className="sticky top-0 z-50 border-b bg-background/95 pt-[env(safe-area-inset-top)] backdrop-blur">
        <div className="mx-auto flex h-14 max-w-6xl items-center gap-2 px-3 sm:h-16 sm:px-6">
          <a
            href="#top"
            className="flex shrink-0 items-center gap-2 no-underline"
            onClick={(e) => {
              e.preventDefault();
              window.scrollTo({ top: 0, behavior: 'smooth' });
            }}
          >
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary text-sm font-bold text-primary-foreground">
              OE
            </span>
            <span className="font-semibold tracking-tight">OraEarn</span>
          </a>

          <nav className="mx-2 hidden min-w-0 flex-1 items-center justify-center gap-0.5 overflow-x-auto lg:flex" aria-label="Page">
            {nav.map((item) => (
              <a
                key={item.href}
                href={item.href}
                className="inline-flex shrink-0 items-center gap-1.5 rounded-md px-2 py-2 text-sm text-muted-foreground no-underline transition-colors hover:bg-accent hover:text-foreground"
                onClick={(e) => {
                  e.preventDefault();
                  onNav(item.href);
                }}
              >
                <item.icon className="h-3.5 w-3.5" />
                {item.short}
              </a>
            ))}
          </nav>

          <div className="ml-auto hidden shrink-0 items-center gap-2 lg:flex">
            {user ? (
              <Button asChild>
                <Link to={appPath}>
                  <LayoutDashboard className="h-4 w-4" />
                  Open app
                </Link>
              </Button>
            ) : (
              <>
                <Button variant="outline" asChild>
                  <Link to="/login">
                    <LogIn className="h-4 w-4" />
                    Log in
                  </Link>
                </Button>
                <Button asChild>
                  <Link to="/register">
                    <UserPlus className="h-4 w-4" />
                    Register
                  </Link>
                </Button>
              </>
            )}
          </div>

          <Button
            type="button"
            variant="outline"
            size="icon"
            className="ml-auto h-11 w-11 lg:hidden"
            aria-label={open ? 'Close menu' : 'Open menu'}
            aria-expanded={open}
            onClick={() => setOpen((v) => !v)}
          >
            {open ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
          </Button>
        </div>

        {open && (
          <div className="oe-menu border-t bg-background px-3 py-3 sm:px-6 lg:hidden">
            <nav className="flex flex-col gap-1" aria-label="Mobile">
              {nav.map((item) => (
                <a
                  key={item.href}
                  href={item.href}
                  className="inline-flex min-h-11 items-center gap-2 rounded-md px-3 py-2.5 text-sm no-underline hover:bg-accent"
                  onClick={(e) => {
                    e.preventDefault();
                    onNav(item.href);
                  }}
                >
                  <item.icon className="h-4 w-4" />
                  {item.label}
                </a>
              ))}
            </nav>
            <div className="mt-3 flex flex-col gap-2 sm:flex-row">
              {user ? (
                <Button className="w-full" asChild>
                  <Link to={appPath}>
                    <LayoutDashboard className="h-4 w-4" />
                    Open app
                  </Link>
                </Button>
              ) : (
                <>
                  <Button variant="outline" className="w-full sm:flex-1" asChild>
                    <Link to="/login">
                      <LogIn className="h-4 w-4" />
                      Log in
                    </Link>
                  </Button>
                  <Button className="w-full sm:flex-1" asChild>
                    <Link to="/register">
                      <UserPlus className="h-4 w-4" />
                      Register
                    </Link>
                  </Button>
                </>
              )}
            </div>
          </div>
        )}
      </header>

      <main>
        <section className="mx-auto grid max-w-6xl items-center gap-8 px-4 py-10 sm:gap-10 sm:px-6 sm:py-16 lg:grid-cols-2 lg:py-20">
          <div className="oe-fade-up min-w-0">
            <p className="mb-3 flex items-center gap-1.5 text-sm text-muted-foreground">
              <Sparkles className="oe-icon-pulse h-4 w-4 shrink-0" />
              For students and casual workers in Australia
            </p>
            <h1 className="text-3xl font-bold leading-tight tracking-tight sm:text-4xl lg:text-5xl">
              Your shift money, under control.
            </h1>
            <p className="mt-4 max-w-xl text-sm leading-relaxed text-muted-foreground sm:text-base">
              OraEarn keeps jobs, shifts, visa hours, expenses and savings in one place. Stay under the 48-hour
              student fortnight, see what is left after rent, and ask Aura a simple money question.
            </p>
            <div className="mt-6 flex w-full flex-col gap-3 sm:mt-8 sm:w-auto sm:flex-row sm:flex-wrap">
              {user ? (
                <Button size="lg" className="w-full sm:w-auto" asChild>
                  <Link to={appPath}>
                    <LayoutDashboard className="h-4 w-4" />
                    Go to dashboard
                  </Link>
                </Button>
              ) : (
                <>
                  <Button size="lg" className="w-full sm:w-auto" asChild>
                    <Link to="/register">
                      <UserPlus className="h-4 w-4" />
                      Create an account
                    </Link>
                  </Button>
                  <Button size="lg" variant="outline" className="w-full sm:w-auto" asChild>
                    <Link to="/login">
                      <LogIn className="h-4 w-4" />
                      Log in
                    </Link>
                  </Button>
                </>
              )}
            </div>
          </div>
          <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {heroChips.map(({ icon: Icon, label }, i) => (
              <li
                key={label}
                className={cn(
                  'oe-fade-up oe-card flex items-center gap-3 rounded-xl border bg-card px-4 py-3.5 text-sm font-medium',
                  `oe-delay-${Math.min(i + 1, 5)}`,
                )}
              >
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                  <Icon className="h-4 w-4" />
                </span>
                <span className="leading-snug">{label}</span>
              </li>
            ))}
          </ul>
        </section>

        <section id="features" className="scroll-mt-24 border-t bg-secondary/60">
          <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16">
            <h2 className="oe-fade-up flex items-center gap-2 text-2xl font-bold tracking-tight sm:text-3xl">
              <Sparkles className="h-6 w-6 shrink-0" />
              All features
            </h2>
            <p className="oe-fade-up oe-delay-1 mt-2 max-w-2xl text-sm text-muted-foreground sm:text-base">
              Everything in the running app on your laptop — black and white, phone and PC.
            </p>
            <div className="mt-8 grid gap-4 sm:mt-10 sm:grid-cols-2 lg:grid-cols-3">
              {features.map(({ icon: Icon, title, text }, i) => (
                <article
                  key={title}
                  className={cn('oe-fade-up oe-card rounded-xl border bg-background p-5', `oe-delay-${Math.min((i % 5) + 1, 5)}`)}
                >
                  <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                    <Icon className="h-5 w-5" />
                  </span>
                  <h3 className="mt-3 font-semibold">{title}</h3>
                  <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section id="how" className="scroll-mt-24">
          <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16">
            <h2 className="flex items-center gap-2 text-2xl font-bold tracking-tight sm:text-3xl">
              <ArrowRight className="h-6 w-6 shrink-0" />
              How it works
            </h2>
            <ol className="mt-8 grid gap-4 sm:mt-10 sm:grid-cols-2 lg:grid-cols-4">
              {steps.map((step, i) => (
                <li key={step.title} className={cn('oe-fade-up oe-card rounded-xl border p-5', `oe-delay-${i + 1}`)}>
                  <span className="flex h-10 w-10 items-center justify-center rounded-md bg-primary text-primary-foreground">
                    <step.icon className="h-5 w-5" />
                  </span>
                  <p className="mt-3 text-xs font-medium text-muted-foreground">Step {i + 1}</p>
                  <h3 className="mt-1 font-semibold">{step.title}</h3>
                  <p className="mt-2 text-sm text-muted-foreground">{step.text}</p>
                </li>
              ))}
            </ol>
          </div>
        </section>

        <section id="visa" className="scroll-mt-24 border-t bg-secondary/60">
          <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16 lg:grid lg:grid-cols-2 lg:items-center lg:gap-12">
            <div className="oe-fade-up">
              <h2 className="flex items-center gap-2 text-2xl font-bold tracking-tight sm:text-3xl">
                <Clock className="h-6 w-6 shrink-0" />
                Visa hours, not a spreadsheet
              </h2>
              <p className="mt-4 text-sm leading-relaxed text-muted-foreground sm:text-base">
                International students on a student visa can usually work 48 hours per fortnight. OraEarn sums
                your saved shifts in that window and shows a bar on the calendar and dashboard. If a new shift
                would go over the cap, the API rejects the save.
              </p>
            </div>
            <div className="oe-fade-up oe-delay-2 mt-8 rounded-xl border bg-background p-5 sm:p-6 lg:mt-0">
              <p className="flex items-center gap-2 text-sm font-medium">
                <ShieldCheck className="h-4 w-4" />
                Fortnight example
              </p>
              <p className="mt-2 text-3xl font-bold sm:text-4xl">28 / 48 hours</p>
              <div className="mt-4 h-2.5 overflow-hidden rounded-full bg-muted">
                <div className="oe-bar h-full bg-primary" style={{ ['--oe-bar' as string]: '58%' }} />
              </div>
              <p className="mt-3 text-sm text-muted-foreground">20 hours still available this fortnight.</p>
            </div>
          </div>
        </section>

        <section id="aura" className="scroll-mt-24">
          <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16">
            <h2 className="oe-fade-up flex items-center gap-2 text-2xl font-bold tracking-tight sm:text-3xl">
              <MessageCircle className="h-6 w-6 shrink-0" />
              Aura
            </h2>
            <p className="oe-fade-up oe-delay-1 mt-4 max-w-2xl text-sm leading-relaxed text-muted-foreground sm:text-base">
              Aura is a small chat widget next to the app. Ask about remaining visa hours, a savings goal, or
              whether this week’s spend is high. It does not replace the calendar or the dashboard.
            </p>
            <div className="oe-fade-up oe-delay-2 oe-card mt-8 max-w-md rounded-xl border p-4">
              <p className="flex items-center gap-2 text-sm font-medium">
                <Sparkles className="oe-icon-pulse h-4 w-4" />
                Example
              </p>
              <p className="mt-2 rounded-lg bg-secondary px-3 py-2 text-sm">How many visa hours do I have left?</p>
              <p className="mt-2 rounded-lg border px-3 py-2 text-sm text-muted-foreground">
                You are at 28 of 48 hours this fortnight. 20 hours remain.
              </p>
            </div>
          </div>
        </section>

        <section id="about" className="scroll-mt-24 border-t bg-secondary/60">
          <div className="mx-auto max-w-6xl px-4 py-12 sm:px-6 sm:py-16">
            <h2 className="flex items-center gap-2 text-2xl font-bold tracking-tight sm:text-3xl">
              <FileText className="h-6 w-6 shrink-0" />
              About OraEarn
            </h2>
            <p className="mt-4 max-w-3xl text-sm leading-relaxed text-muted-foreground sm:text-base">
              OraEarn is a CDU PRT681 student project for Group 4 (Sydney). It runs locally: React on port 5173,
              the ASP.NET API on 5203, and Aura on 8000, with PostgreSQL on your machine. Nothing is hosted on
              the public internet yet.
            </p>
            <div className="mt-8 flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
              <Button className="w-full sm:w-auto" asChild>
                <Link to="/register">
                  <UserPlus className="h-4 w-4" />
                  Register
                </Link>
              </Button>
              <Button variant="outline" className="w-full sm:w-auto" asChild>
                <Link to="/login">
                  <LogIn className="h-4 w-4" />
                  Log in
                </Link>
              </Button>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t pb-[env(safe-area-inset-bottom)]">
        <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10 sm:flex-row sm:justify-between sm:px-6">
          <div>
            <p className="flex items-center gap-2 font-semibold">
              <span className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-xs text-primary-foreground">OE</span>
              OraEarn
            </p>
            <p className="mt-2 text-sm text-muted-foreground">Track shifts. Stay visa-safe. Save what is left.</p>
          </div>
          <div className="grid grid-cols-2 gap-x-8 gap-y-3 text-sm sm:gap-x-10">
            {nav.map((item) => (
              <a
                key={item.href}
                href={item.href}
                className="inline-flex min-h-10 items-center gap-2 text-muted-foreground no-underline hover:text-foreground"
                onClick={(e) => {
                  e.preventDefault();
                  onNav(item.href);
                }}
              >
                <item.icon className="h-3.5 w-3.5" />
                {item.label}
              </a>
            ))}
            <Link to="/login" className="inline-flex min-h-10 items-center gap-2 text-muted-foreground no-underline hover:text-foreground">
              <LogIn className="h-3.5 w-3.5" />
              Log in
            </Link>
            <Link to="/register" className="inline-flex min-h-10 items-center gap-2 text-muted-foreground no-underline hover:text-foreground">
              <UserPlus className="h-3.5 w-3.5" />
              Register
            </Link>
          </div>
        </div>
        <div className="border-t py-4 text-center text-xs text-muted-foreground">
          PRT681 Group 4 · local demo · no public hosting
        </div>
      </footer>
    </div>
  );
}
