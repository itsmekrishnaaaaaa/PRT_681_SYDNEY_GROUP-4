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
    <div className={cn('flex flex-col items-center justify-center rounded-xl border border-dashed bg-muted/30 px-6 py-12 text-center', className)}>
      <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-violet-100 text-violet-700">
        <Icon className="h-7 w-7" />
      </div>
      <h3 className="font-semibold text-base">{title}</h3>
      <p className="mt-1 max-w-sm text-muted-foreground text-sm leading-relaxed">{description}</p>
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
