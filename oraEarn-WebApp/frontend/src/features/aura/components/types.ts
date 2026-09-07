export type ChatRole = 'user' | 'assistant';

export type ChatMessage = {
  id: string;
  role: ChatRole;
  text: string;
  suggestions?: string[];
};

export type ChatSendResult = {
  reply: string;
  suggestions?: string[];
};

export type ChatSendHandler = (message: string) => Promise<ChatSendResult>;

export type ChatWidgetProps = {
  onSend: ChatSendHandler;
  title?: string;
  subtitle?: string;
  badge?: string;
  assistantName?: string;
  welcomeMessage?: string;
  starters?: string[];
  userInitials?: string;
  position?: 'bottom-right' | 'bottom-left';
  className?: string;
};
