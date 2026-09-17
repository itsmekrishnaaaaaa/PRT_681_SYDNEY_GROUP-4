import { useState } from 'react';
import { MessageCircle, Sparkles, X } from 'lucide-react';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';
import { ChatConversation } from './ChatConversation';
import type { ChatWidgetProps } from './types';
import { useChat } from './useChat';

export function ChatWidget({
  onSend,
  title = 'Assistant',
  subtitle,
  badge,
  assistantName = 'Assistant',
  welcomeMessage = 'Hello! How can I help?',
  userInitials = 'U',
  position = 'bottom-right',
  className,
}: ChatWidgetProps) {
  const [open, setOpen] = useState(false);
  const chat = useChat({ onSend, welcomeMessage });

  const corner = position === 'bottom-left' ? 'left-4 sm:left-6' : 'right-4 sm:right-6';

  return (
    <div className={cn('pointer-events-none fixed inset-0 z-50', className)} aria-live="polite">
      {/* Pop-out panel */}
      <div
        className={cn(
          'pointer-events-auto fixed bottom-20 flex w-[min(calc(100vw-2rem),380px)] flex-col overflow-hidden rounded-2xl border bg-card shadow-2xl transition-all duration-300 sm:bottom-24 sm:w-[400px]',
          corner,
          open ? 'scale-100 opacity-100' : 'pointer-events-none scale-95 opacity-0',
        )}
        style={{ height: 'min(560px, calc(100vh - 7rem))' }}
        role="dialog"
        aria-label={`${title} chat`}
        aria-hidden={!open}
      >
        {/* Header */}
        <div className="flex shrink-0 items-start justify-between gap-2 bg-primary px-4 py-3 text-primary-foreground">
          <div className="flex min-w-0 items-center gap-2.5">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary-foreground/20">
              <Sparkles className="h-4 w-4" />
            </div>
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-1.5">
                <p className="font-semibold text-sm leading-tight">{title}</p>
                {badge && (
                  <Badge variant="secondary" className="h-5 border-primary-foreground/20 bg-primary-foreground/15 px-1.5 text-[10px] text-primary-foreground">
                    {badge}
                  </Badge>
                )}
              </div>
              {subtitle && <p className="truncate text-[11px] text-primary-foreground/75">{subtitle}</p>}
            </div>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-8 w-8 shrink-0 text-primary-foreground hover:bg-primary-foreground/20 hover:text-primary-foreground"
            onClick={() => setOpen(false)}
            aria-label="Close chat"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        <ChatConversation
          {...chat}
          userInitials={userInitials}
          assistantName={assistantName}
        />
      </div>

      {/* Floating launcher */}
      <Button
        type="button"
        size="icon"
        className={cn(
          'pointer-events-auto fixed bottom-4 h-14 w-14 rounded-full shadow-lg transition-transform hover:scale-105 sm:bottom-6',
          'bg-primary text-primary-foreground hover:bg-primary/90',
          corner,
        )}
        onClick={() => setOpen((v) => !v)}
        aria-label={open ? 'Close chat' : `Open ${title} chat`}
        aria-expanded={open}
      >
        {open ? <X className="h-6 w-6" /> : <MessageCircle className="h-6 w-6" />}
      </Button>
    </div>
  );
}
