import { Briefcase, CalendarDays, PiggyBank, Receipt, Sparkles } from 'lucide-react';

const steps = [
  { icon: Briefcase, label: 'Add a job', detail: 'Set your hourly rate' },
  { icon: CalendarDays, label: 'Log shifts', detail: 'Track hours & pay' },
  { icon: Receipt, label: 'Record expenses', detail: 'See where money goes' },
  { icon: PiggyBank, label: 'Save smarter', detail: 'Hit your goals' },
];

export function QuickStartGuide() {
  return (
    <div className="rounded-xl border bg-secondary p-3">
      <p className="mb-2 flex items-center gap-1.5 font-medium text-foreground text-xs">
        <Sparkles className="h-3.5 w-3.5" />
        Quick start
      </p>
      <ul className="space-y-2">
        {steps.map(({ icon: Icon, label, detail }) => (
          <li key={label} className="flex items-start gap-2 text-xs">
            <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-md bg-background text-foreground">
              <Icon className="h-3.5 w-3.5" />
            </span>
            <span>
              <span className="font-medium text-foreground">{label}</span>
              <span className="block text-muted-foreground">{detail}</span>
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export const pageHints: Record<string, string> = {
  '/dashboard': 'Overview of earnings, spending & visa limits',
  '/jobs': 'Employers & pay rates for shift earnings',
  '/shifts': 'Calendar, live clock-in & fortnight tracking',
  '/savings': 'Goals, progress & hours-to-complete',
  '/expenses': 'Categories, budgets & payment methods',
};
