import { cn } from '@/shared/lib/utils';
import { money } from '@/shared/utils/dates';

export type TimelinePoint = {
  dayLabel: string;
  date: string;
  shiftHours: number;
  shiftCount: number;
  expenseAmount: number;
  expenseCount: number;
};

type TimelineChartProps = {
  title: string;
  description?: string;
  data: TimelinePoint[];
  mode: 'shifts' | 'expenses';
  className?: string;
};

export function TimelineChart({ title, description, data, mode, className }: TimelineChartProps) {
  const values = data.map((d) => (mode === 'shifts' ? d.shiftHours : d.expenseAmount));
  const max = Math.max(...values, mode === 'shifts' ? 8 : 1);
  const scrollable = data.length > 7;

  const formatValue = (d: TimelinePoint) => {
    if (mode === 'shifts') {
      return d.shiftHours > 0 ? `${d.shiftHours}h` : '—';
    }
    return d.expenseAmount > 0 ? money(d.expenseAmount) : '—';
  };

  const subLabel = (d: TimelinePoint) =>
    mode === 'shifts'
      ? d.shiftCount === 1 ? '1 shift' : d.shiftCount > 1 ? `${d.shiftCount} shifts` : 'No shifts'
      : d.expenseCount === 1 ? '1 expense' : d.expenseCount > 1 ? `${d.expenseCount} expenses` : 'No expenses';

  return (
    <div className={cn('space-y-4', className)}>
      <div>
        <p className="font-medium text-sm">{title}</p>
        {description && <p className="text-muted-foreground text-xs mt-0.5">{description}</p>}
      </div>
      <div className={cn('overflow-x-auto pb-1', scrollable && '-mx-1 px-1')}>
        <div
          className={cn(
            'flex items-end gap-1.5 h-40 min-w-full',
            !scrollable && 'justify-between',
          )}
          style={scrollable ? { minWidth: `${data.length * 2.25}rem` } : undefined}
        >
          {data.map((d) => {
            const value = mode === 'shifts' ? d.shiftHours : d.expenseAmount;
            const pct = max <= 0 ? 0 : Math.max(4, (value / max) * 100);
            return (
              <div
                key={d.date}
                className={cn('flex flex-col items-center gap-1.5 h-full justify-end', scrollable ? 'min-w-[2rem] flex-1' : 'flex-1')}
              >
                <span className="text-[10px] text-muted-foreground font-medium whitespace-nowrap">{formatValue(d)}</span>
                <div
                  className={cn(
                    'w-full max-w-10 rounded-t-md transition-all',
                    mode === 'shifts' ? 'bg-violet-600' : 'bg-amber-500',
                  )}
                  style={{ height: `${pct}%` }}
                  title={`${d.dayLabel} · ${d.date}`}
                />
                <div className="text-center">
                  <p className="text-[10px] font-semibold">{d.dayLabel}</p>
                  <p className="text-[9px] text-muted-foreground leading-tight hidden sm:block">{subLabel(d)}</p>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
