import type { LucideIcon } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

type EmptyStateProps = {
  icon: LucideIcon;
  title: string;
  description: string;
  action?: React.ReactNode;
  className?: string;
};

export function EmptyState({ icon: Icon, title, description, action, className }: EmptyStateProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center rounded-xl border border-dashed bg-muted/30 px-4 py-8 text-center sm:px-6 sm:py-12', className)}>
      <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-secondary text-foreground">
        <Icon className="h-7 w-7" />
      </div>
      <h3 className="font-semibold text-base">{title}</h3>
      <p className="mt-1 max-w-sm text-muted-foreground text-sm leading-relaxed">{description}</p>
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
