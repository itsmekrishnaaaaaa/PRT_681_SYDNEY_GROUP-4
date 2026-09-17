import { cn } from '@/shared/lib/utils';

export function ChatTypingIndicator({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center gap-1 px-1 py-2', className)} aria-label="Assistant is typing">
      {[0, 1, 2].map((i) => (
        <span
          key={i}
          className="h-2 w-2 rounded-full bg-muted-foreground/50 animate-bounce"
          style={{ animationDelay: `${i * 150}ms` }}
        />
      ))}
    </div>
  );
}
