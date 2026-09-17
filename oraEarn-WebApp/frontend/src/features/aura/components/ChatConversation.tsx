import { Send, Sparkles } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { ScrollArea } from '@/shared/ui/scroll-area';
import { Textarea } from '@/shared/ui/textarea';
import { ChatMessageBubble } from './ChatMessageBubble';
import { ChatTypingIndicator } from './ChatTypingIndicator';
import type { ChatMessage } from './types';

type Props = {
  messages: ChatMessage[];
  input: string;
  setInput: (v: string) => void;
  loading: boolean;
  error: string;
  send: (text: string) => Promise<void>;
  bottomRef: React.RefObject<HTMLDivElement | null>;
  userInitials: string;
  assistantName: string;
};

export function ChatConversation({
  messages,
  input,
  setInput,
  loading,
  error,
  send,
  bottomRef,
  userInitials,
  assistantName,
}: Props) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    void send(input);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      void send(input);
    }
  };

  return (
    <div className="flex min-h-0 flex-1 flex-col bg-background">
      <ScrollArea className="flex-1 px-4 py-4">
        <div className="flex flex-col gap-4">
          {messages.map((msg) => (
            <ChatMessageBubble
              key={msg.id}
              message={msg}
              userInitials={userInitials}
              assistantName={assistantName}
            />
          ))}
          {loading && (
            <div className="flex gap-3">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted">
                <Sparkles className="h-4 w-4 animate-pulse text-foreground" />
              </div>
              <ChatTypingIndicator />
            </div>
          )}
          <div ref={bottomRef} />
        </div>
      </ScrollArea>

      {error && (
        <p className="border-t border-destructive/20 bg-destructive/5 px-4 py-2 text-sm text-destructive">{error}</p>
      )}

      <form className="flex items-end gap-2 border-t bg-muted/30 p-3" onSubmit={handleSubmit}>
        <Textarea
          rows={1}
          placeholder="Message…"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={loading}
          className="max-h-24 min-h-[40px] bg-background text-sm"
        />
        <Button type="submit" size="icon" disabled={loading || !input.trim()} className="h-10 w-10 shrink-0">
          <Send className="h-4 w-4" />
        </Button>
      </form>
    </div>
  );
}
