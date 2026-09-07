import * as React from 'react';
import { cn } from '@/shared/lib/utils';

function Avatar({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      className={cn('relative flex h-8 w-8 shrink-0 overflow-hidden rounded-full', className)}
      {...props}
    />
  );
}

function AvatarFallback({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      className={cn(
        'flex h-full w-full items-center justify-center rounded-full bg-muted text-xs font-medium text-muted-foreground',
        className,
      )}
      {...props}
    />
  );
}

export { Avatar, AvatarFallback };
