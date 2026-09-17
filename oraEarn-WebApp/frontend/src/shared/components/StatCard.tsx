import type { LucideIcon } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';

const toneStyles = {
  violet: 'bg-secondary text-foreground',
  amber: 'bg-secondary text-foreground',
  emerald: 'bg-primary text-primary-foreground',
  blue: 'bg-secondary text-foreground',
  rose: 'bg-muted text-foreground',
  slate: 'bg-secondary text-foreground',
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
    <Card className={cn('overflow-hidden', className)}>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-2">
          <div className={cn('flex h-9 w-9 items-center justify-center rounded-lg', toneStyles[tone])}>
            <Icon className="h-4 w-4" />
          </div>
          {trend}
        </div>
        <CardDescription className="pt-2">{label}</CardDescription>
        <CardTitle className="break-words text-xl tabular-nums sm:text-2xl">{value}</CardTitle>
      </CardHeader>
      {hint && (
        <CardContent className="pt-0">
          <p className="text-muted-foreground text-xs">{hint}</p>
        </CardContent>
      )}
    </Card>
  );
}
