import { cn } from '@/shared/lib/utils';
import { createContext, useContext, useState, type ReactNode } from 'react';

type TabsContextValue = {
  value: string;
  setValue: (v: string) => void;
};

const TabsContext = createContext<TabsContextValue | null>(null);

export function Tabs({
  defaultValue = '',
  value: controlledValue,
  onValueChange,
  className,
  children,
}: {
  defaultValue?: string;
  value?: string;
  onValueChange?: (v: string) => void;
  className?: string;
  children: ReactNode;
}) {
  const [internal, setInternal] = useState(defaultValue || controlledValue || '');
  const value = controlledValue ?? internal;
  const setValue = (v: string) => {
    setInternal(v);
    onValueChange?.(v);
  };

  return (
    <TabsContext.Provider value={{ value, setValue }}>
      <div className={className}>{children}</div>
    </TabsContext.Provider>
  );
}

export function TabsList({ className, children }: { className?: string; children: ReactNode }) {
  return (
    <div className={cn('flex w-full min-w-0 items-center overflow-x-auto rounded-lg border bg-muted p-1 text-muted-foreground sm:inline-flex sm:w-auto sm:h-9', className)}>
      {children}
    </div>
  );
}

export function TabsTrigger({ value, className, children }: { value: string; className?: string; children: ReactNode }) {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error('TabsTrigger must be used within Tabs');
  const active = ctx.value === value;
  return (
    <button
      type="button"
      role="tab"
      aria-selected={active}
      className={cn(
        'inline-flex flex-1 items-center justify-center whitespace-nowrap rounded-md px-3 py-2 text-sm font-medium transition-all sm:flex-none sm:py-1',
        active ? 'bg-background text-foreground shadow-sm' : 'hover:text-foreground',
        className,
      )}
      onClick={() => ctx.setValue(value)}
    >
      {children}
    </button>
  );
}

export function TabsContent({ value, className, children }: { value: string; className?: string; children: ReactNode }) {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error('TabsContent must be used within Tabs');
  if (ctx.value !== value) return null;
  return <div role="tabpanel" className={cn('mt-4', className)}>{children}</div>;
}
