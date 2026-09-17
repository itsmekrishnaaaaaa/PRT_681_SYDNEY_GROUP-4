import { advisorChat } from '@/features/savings/api';
import { getErrorMessage } from '@/shared/api/client';
import { ChatWidget } from '@/features/aura/components';
import { useAuth } from '@/auth';

/** Floating Aura chat — bottom-right pop-out. */
export function AuraWidget() {
  const { user } = useAuth();
  const initials =
    `${user?.firstName?.[0] ?? ''}${user?.lastName?.[0] ?? ''}`.toUpperCase() || 'U';
  const first = user?.firstName ?? 'there';

  return (
    <ChatWidget
      title="Aura"
      subtitle="Your OraEarn assistant"
      badge="AI"
      assistantName="Aura"
      userInitials={initials}
      position="bottom-right"
      welcomeMessage={`Hey ${first}! I'm Aura — ask about your visa, hours, savings, or balance.`}
      onSend={async (message) => {
        try {
          const res = await advisorChat(message);
          return { reply: res.reply };
        } catch (err) {
          throw new Error(getErrorMessage(err));
        }
      }}
    />
  );
}
