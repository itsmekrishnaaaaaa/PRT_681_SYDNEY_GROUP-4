import type { LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import { cn } from '@/shared/lib/utils';

type PageHeaderProps = {
  title: string;
  description?: string;
  icon?: LucideIcon;
  action?: ReactNode;
  className?: string;
  badge?: ReactNode;
};

export function PageHeader({ title, description, icon: Icon, action, className, badge }: PageHeaderProps) {
  return (
    <div className={cn('mb-6 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between', className)}>
      <div className="flex gap-4">
        {Icon && (
          <div className="hidden sm:flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-violet-600 to-indigo-600 text-white shadow-md">
            <Icon className="h-6 w-6" />
          </div>
        )}
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
            {badge}
          </div>
          {description && (
            <p className="mt-1.5 text-muted-foreground text-sm max-w-2xl leading-relaxed">{description}</p>
          )}
        </div>
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}
