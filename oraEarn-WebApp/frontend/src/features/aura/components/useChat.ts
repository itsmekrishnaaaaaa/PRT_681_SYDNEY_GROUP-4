import { useCallback, useEffect, useRef, useState } from 'react';
import type { ChatMessage, ChatSendHandler } from './types';

let messageCounter = 0;
const nextId = () => `msg-${++messageCounter}-${Date.now()}`;

type UseChatOptions = {
  onSend: ChatSendHandler;
  welcomeMessage: string;
};

export function useChat({ onSend, welcomeMessage }: UseChatOptions) {
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: nextId(),
      role: 'assistant',
      text: welcomeMessage,
    },
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  const send = useCallback(
    async (text: string) => {
      const trimmed = text.trim();
      if (!trimmed || loading) return;

      setError('');
      setInput('');
      setMessages((m) => [...m, { id: nextId(), role: 'user', text: trimmed }]);
      setLoading(true);

      try {
        const res = await onSend(trimmed);
        setMessages((m) => [
          ...m,
          {
            id: nextId(),
            role: 'assistant',
            text: res.reply,
          },
        ]);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Something went wrong');
      } finally {
        setLoading(false);
      }
    },
    [loading, onSend],
  );

  return {
    messages,
    input,
    setInput,
    loading,
    error,
    send,
    bottomRef,
  };
}
