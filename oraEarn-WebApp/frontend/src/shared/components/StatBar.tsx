import { cn } from '@/shared/lib/utils';

type StatBarProps = {
  label: string;
  value: number;
  max: number;
  display?: string;
  className?: string;
};

export function StatBar({ label, value, max, display, className }: StatBarProps) {
  const pct = max <= 0 ? 0 : Math.min(100, (value / max) * 100);
  return (
    <div className={cn('space-y-1.5', className)}>
      <div className="flex items-center justify-between text-sm">
        <span className="font-medium">{label}</span>
        <span className="text-muted-foreground">{display ?? value}</span>
      </div>
      <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
        <div className="h-full rounded-full bg-foreground transition-all" style={{ width: `${pct}%` }} />
      </div>
    </div>
  );
}
