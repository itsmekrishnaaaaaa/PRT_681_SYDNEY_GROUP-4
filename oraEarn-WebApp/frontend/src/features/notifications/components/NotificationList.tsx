import { cn } from '@/shared/lib/utils';
import { AlertCircle, AlertTriangle, Info } from 'lucide-react';

export type NotificationSeverity = 'error' | 'warning' | 'info';

export interface AppNotification {
  type: string;
  message: string;
  severity: NotificationSeverity;
}

const iconMap = {
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info,
};

const styleMap = {
  error: 'border-destructive/50 bg-destructive/5 text-destructive',
  warning: 'border-amber-500/50 bg-amber-50 text-amber-900',
  info: 'border-border bg-muted/50 text-foreground',
};

export function NotificationList({ items }: { items: AppNotification[] }) {
  if (items.length === 0) return null;

  return (
    <div className="space-y-2">
      {items.map((n) => {
        const Icon = iconMap[n.severity];
        return (
          <div key={`${n.type}-${n.message}`} className={cn('flex gap-3 rounded-lg border p-3 text-sm', styleMap[n.severity])}>
            <Icon className="h-4 w-4 shrink-0 mt-0.5" />
            <p>{n.message}</p>
          </div>
        );
      })}
    </div>
  );
}
