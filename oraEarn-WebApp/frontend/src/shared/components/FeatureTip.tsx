import type { LucideIcon } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Lightbulb, Sparkles } from 'lucide-react';

type FeatureTipProps = {
  title?: string;
  children: React.ReactNode;
  icon?: LucideIcon;
  variant?: 'default' | 'accent';
  className?: string;
};

export function FeatureTip({ title, children, icon: Icon = Lightbulb, variant = 'default', className }: FeatureTipProps) {
  return (
    <div
      className={cn(
        'flex gap-3 rounded-xl border p-4 text-sm',
        variant === 'accent'
          ? 'border-violet-200 bg-gradient-to-r from-violet-50 to-indigo-50'
          : 'border-border bg-muted/40',
        className,
      )}
    >
      <div className={cn('mt-0.5 shrink-0', variant === 'accent' ? 'text-violet-600' : 'text-muted-foreground')}>
        {variant === 'accent' ? <Sparkles className="h-4 w-4" /> : <Icon className="h-4 w-4" />}
      </div>
      <div className="min-w-0 space-y-0.5">
        {title && <p className="font-medium">{title}</p>}
        <p className="text-muted-foreground leading-relaxed">{children}</p>
      </div>
    </div>
  );
}
