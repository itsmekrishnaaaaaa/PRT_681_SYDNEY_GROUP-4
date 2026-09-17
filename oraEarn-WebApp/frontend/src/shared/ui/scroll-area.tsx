import * as React from 'react';
import { cn } from '@/shared/lib/utils';

type ScrollAreaProps = React.ComponentProps<'div'> & {
  viewportRef?: React.Ref<HTMLDivElement>;
};

function ScrollArea({ className, children, viewportRef, ...props }: ScrollAreaProps) {
  return (
    <div
      ref={viewportRef}
      className={cn('overflow-y-auto overflow-x-hidden scrollbar-thin', className)}
      {...props}
    >
      {children}
    </div>
  );
}

export { ScrollArea };
