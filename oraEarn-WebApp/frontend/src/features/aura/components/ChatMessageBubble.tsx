import { Bot } from 'lucide-react';
import { Avatar, AvatarFallback } from '@/shared/ui/avatar';
import { cn } from '@/shared/lib/utils';
import type { ChatMessage } from './types';

type Props = {
  message: ChatMessage;
  userInitials: string;
  assistantName: string;
};

export function ChatMessageBubble({ message, userInitials, assistantName }: Props) {
  const isUser = message.role === 'user';

  return (
    <div className={cn('flex gap-3', isUser ? 'flex-row-reverse' : 'flex-row')}>
      <Avatar className="h-8 w-8 mt-0.5">
        <AvatarFallback
          className={cn(
            isUser ? 'bg-primary text-primary-foreground' : 'bg-violet-600 text-white',
          )}
        >
          {isUser ? userInitials : <Bot className="h-4 w-4" aria-hidden />}
        </AvatarFallback>
      </Avatar>

      <div className={cn('flex max-w-[min(85%,32rem)] flex-col gap-2', isUser ? 'items-end' : 'items-start')}>
        <span className="text-[10px] font-medium uppercase tracking-wide text-muted-foreground">
          {isUser ? 'You' : assistantName}
        </span>
        <div
          className={cn(
            'rounded-2xl px-4 py-2.5 text-sm leading-relaxed shadow-sm',
            isUser
              ? 'rounded-tr-md bg-primary text-primary-foreground'
              : 'rounded-tl-md bg-muted text-foreground',
          )}
        >
          {message.text}
        </div>
      </div>
    </div>
  );
}
