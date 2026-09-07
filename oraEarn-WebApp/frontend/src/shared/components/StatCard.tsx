import type { LucideIcon } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';

const toneStyles = {
  violet: 'bg-violet-100 text-violet-700',
  amber: 'bg-amber-100 text-amber-700',
  emerald: 'bg-emerald-100 text-emerald-700',
  blue: 'bg-blue-100 text-blue-700',
  rose: 'bg-rose-100 text-rose-700',
  slate: 'bg-slate-100 text-slate-700',
} as const;

type StatCardProps = {
  icon: LucideIcon;
  label: string;
  value: React.ReactNode;
  hint?: string;
  tone?: keyof typeof toneStyles;
  className?: string;
  trend?: React.ReactNode;
};

export function StatCard({ icon: Icon, label, value, hint, tone = 'violet', className, trend }: StatCardProps) {
  return (
    <Card className={cn('overflow-hidden transition-shadow hover:shadow-md', className)}>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-2">
          <div className={cn('flex h-9 w-9 items-center justify-center rounded-lg', toneStyles[tone])}>
            <Icon className="h-4 w-4" />
          </div>
          {trend}
        </div>
        <CardDescription className="pt-2">{label}</CardDescription>
        <CardTitle className="text-2xl tabular-nums">{value}</CardTitle>
      </CardHeader>
      {hint && (
        <CardContent className="pt-0">
          <p className="text-muted-foreground text-xs">{hint}</p>
        </CardContent>
      )}
    </Card>
  );
}
