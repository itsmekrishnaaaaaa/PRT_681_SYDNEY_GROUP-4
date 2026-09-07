import { Badge } from '@/shared/ui/badge';
import { Tabs, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import type { AnalyticsPeriodKey } from '@/shared/types';
import { cn } from '@/shared/lib/utils';

const PERIODS: { key: AnalyticsPeriodKey; label: string; recommended?: boolean }[] = [
  { key: '7d', label: '7 days', recommended: true },
  { key: '30d', label: '1 month' },
  { key: '12m', label: '12 months' },
];

type Props = {
  value: AnalyticsPeriodKey;
  onChange: (period: AnalyticsPeriodKey) => void;
  className?: string;
};

export function AnalyticsPeriodFilter({ value, onChange, className }: Props) {
  return (
    <div className={cn('space-y-2', className)}>
      <div className="flex flex-wrap items-center gap-2">
        <Tabs value={value} onValueChange={(v) => onChange(v as AnalyticsPeriodKey)}>
          <TabsList>
            {PERIODS.map((p) => (
              <TabsTrigger key={p.key} value={p.key} className="gap-1.5">
                {p.label}
                {p.recommended && (
                  <Badge variant="secondary" className="h-5 px-1.5 text-[10px] font-normal">
                    Recommended
                  </Badge>
                )}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </div>
      <p className="text-muted-foreground text-xs">
        Use <strong>7 days</strong> for daily shift tracking · <strong>1 month</strong> for spending trends ·{' '}
        <strong>12 months</strong> for long-term overview
      </p>
    </div>
  );
}
