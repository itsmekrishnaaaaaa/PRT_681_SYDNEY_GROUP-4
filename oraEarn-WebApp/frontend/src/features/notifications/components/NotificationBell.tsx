import { useCallback, useEffect, useRef, useState } from 'react';
import { getErrorMessage } from '@/shared/api/client';
import {
  getNotifications,
  markAllNotificationsRead,
  markNotificationRead,
} from '@/features/notifications/api';
import { Button } from '@/shared/ui/button';
import { ScrollArea } from '@/shared/ui/scroll-area';
import { cn } from '@/shared/lib/utils';
import type { AppNotification } from '@/shared/types';
import {
  AlertCircle,
  AlertTriangle,
  Bell,
  Check,
  CheckCheck,
  Info,
  Loader2,
  X,
} from 'lucide-react';

const iconMap = {
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info,
};

const styleMap = {
  error: 'border-destructive/30 bg-destructive/5',
  warning: 'border-foreground/30 bg-muted',
  info: 'border-border bg-muted/40',
};

type Props = {
  unreadCount?: number;
  onUnreadChange?: (count: number) => void;
  placement?: 'bottom-end' | 'bottom-start';
};

export function NotificationBell({
  unreadCount: externalUnread,
  onUnreadChange,
  placement = 'bottom-end',
}: Props) {
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState<AppNotification[]>([]);
  const [filter, setFilter] = useState<'all' | 'unread'>('all');
  const panelRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const unreadCount = externalUnread ?? items.filter((n) => !n.isRead).length;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const inbox = await getNotifications();
      setItems(inbox.items);
      onUnreadChange?.(inbox.unreadCount);
    } catch (err) {
      console.error(getErrorMessage(err, 'Could not load notifications.'));
    } finally {
      setLoading(false);
    }
  }, [onUnreadChange]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    if (!open) return;
    const onPointerDown = (e: MouseEvent) => {
      const target = e.target as Node;
      if (panelRef.current?.contains(target) || containerRef.current?.contains(target)) return;
      setOpen(false);
    };
    document.addEventListener('mousedown', onPointerDown);
    return () => document.removeEventListener('mousedown', onPointerDown);
  }, [open]);

  const handleOpen = () => {
    setOpen((v) => !v);
    if (!open) void load();
  };

  const handleMarkRead = async (id: string) => {
    await markNotificationRead(id);
    setItems((prev) => {
      const next = prev.map((n) => (n.id === id ? { ...n, isRead: true } : n));
      onUnreadChange?.(next.filter((n) => !n.isRead).length);
      return next;
    });
  };

  const handleMarkAllRead = async () => {
    const unreadIds = items.filter((n) => !n.isRead).map((n) => n.id);
    if (unreadIds.length === 0) return;
    await markAllNotificationsRead(unreadIds);
    setItems((prev) => prev.map((n) => ({ ...n, isRead: true })));
    onUnreadChange?.(0);
  };

  const visible = items.filter((n) => filter === 'all' || !n.isRead);

  return (
    <div className="relative" ref={containerRef}>
      <Button
        type="button"
        variant="outline"
        size="icon"
        className="relative h-10 w-10 rounded-full shadow-sm"
        aria-label="Notifications"
        onClick={handleOpen}
      >
        <Bell className="h-4 w-4" />
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-destructive-foreground">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </Button>

      {open && (
        <div
          ref={panelRef}
          className={cn(
            'z-50 overflow-hidden rounded-xl border bg-background shadow-xl',
            'fixed inset-x-3 top-[calc(3.75rem+env(safe-area-inset-top))] lg:absolute lg:inset-x-auto lg:top-full lg:mt-2',
            'w-auto max-h-[min(70dvh,28rem)] lg:w-[min(calc(100vw-1.5rem),24rem)]',
            placement === 'bottom-start' ? 'lg:left-0' : 'lg:right-0',
          )}
        >
          <div className="flex items-center justify-between border-b px-4 py-3">
            <div>
              <p className="font-semibold text-sm">Notifications</p>
              <p className="text-muted-foreground text-xs">{unreadCount} unread</p>
            </div>
            <Button type="button" variant="ghost" size="icon" className="h-8 w-8" onClick={() => setOpen(false)}>
              <X className="h-4 w-4" />
            </Button>
          </div>

          <div className="flex gap-1 border-b px-3 py-2">
            {(['all', 'unread'] as const).map((tab) => (
              <button
                key={tab}
                type="button"
                className={cn(
                  'rounded-md px-3 py-1 text-xs font-medium capitalize transition-colors',
                  filter === tab ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-muted',
                )}
                onClick={() => setFilter(tab)}
              >
                {tab}
              </button>
            ))}
            {unreadCount > 0 && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="ml-auto h-7 gap-1 text-xs"
                onClick={() => void handleMarkAllRead()}
              >
                <CheckCheck className="h-3.5 w-3.5" />
                Mark all read
              </Button>
            )}
          </div>

          <ScrollArea className="max-h-[min(60vh,22rem)]">
            {loading ? (
              <div className="flex items-center justify-center gap-2 py-12 text-muted-foreground text-sm">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading…
              </div>
            ) : visible.length === 0 ? (
              <div className="py-12 text-center text-muted-foreground text-sm">
                {filter === 'unread' ? 'All caught up — no unread notifications.' : 'No notifications yet.'}
              </div>
            ) : (
              <ul className="divide-y">
                {visible.map((n) => {
                  const Icon = iconMap[n.severity];
                  return (
                    <li
                      key={n.id}
                      className={cn(
                        'relative px-4 py-3 transition-colors',
                        !n.isRead && 'bg-muted/70',
                        styleMap[n.severity],
                      )}
                    >
                      {!n.isRead && (
                        <span className="absolute left-1.5 top-4 h-2 w-2 rounded-full bg-foreground" />
                      )}
                      <div className="flex gap-3 pl-2">
                        <Icon className={cn('mt-0.5 h-4 w-4 shrink-0', n.severity === 'error' && 'text-foreground', n.severity === 'warning' && 'text-muted-foreground')} />
                        <div className="min-w-0 flex-1 space-y-1">
                          <div className="flex items-start justify-between gap-2">
                            <p className={cn('font-medium text-sm leading-tight', !n.isRead && 'text-foreground')}>
                              {n.title}
                            </p>
                            {!n.isRead && (
                              <Button
                                type="button"
                                variant="ghost"
                                size="icon"
                                className="h-7 w-7 shrink-0"
                                title="Mark as read"
                                onClick={() => void handleMarkRead(n.id)}
                              >
                                <Check className="h-3.5 w-3.5" />
                              </Button>
                            )}
                          </div>
                          <p className="text-muted-foreground text-xs leading-relaxed">{n.message}</p>
                          <p className="text-[10px] text-muted-foreground capitalize">{n.category}</p>
                        </div>
                      </div>
                    </li>
                  );
                })}
              </ul>
            )}
          </ScrollArea>
        </div>
      )}
    </div>
  );
}
