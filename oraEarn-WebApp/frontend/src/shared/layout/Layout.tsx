import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Briefcase, CalendarDays, LayoutDashboard, LogOut, PiggyBank, Receipt, Shield, Sparkles } from 'lucide-react';
import { AuraWidget } from '@/features/aura/AuraWidget';
import { NotificationBell } from '@/features/notifications/components/NotificationBell';
import { pageHints, QuickStartGuide } from '@/shared/layout/QuickStartGuide';
import { useAuth } from '@/auth';
import { Button } from '@/shared/ui/button';
import { Separator } from '@/shared/ui/separator';
import { cn } from '@/shared/lib/utils';

const navItems = [
  { to: '/', label: 'Analytics', icon: LayoutDashboard, end: true },
  { to: '/jobs', label: 'Jobs', icon: Briefcase },
  { to: '/shifts', label: 'Shifts', icon: CalendarDays },
  { to: '/savings', label: 'Savings Goals', icon: PiggyBank },
  { to: '/expenses', label: 'Expenses', icon: Receipt },
];

export function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const currentHint = pageHints[location.pathname];

  return (
    <div className="min-h-screen bg-muted/30 lg:grid lg:grid-cols-[280px_1fr]">
      <aside className="flex flex-col gap-6 border-b bg-card p-4 lg:border-b-0 lg:border-r">
        <div className="flex items-center gap-3 rounded-xl bg-primary p-3 text-primary-foreground">
          <div className="flex h-11 w-11 items-center justify-center rounded-lg bg-primary-foreground/15 font-bold">
            OE
          </div>
          <div>
            <p className="font-semibold tracking-tight">OraEarn</p>
            <p className="flex items-center gap-1 text-xs text-primary-foreground/80">
              <Sparkles className="h-3 w-3" />
              {isAdmin ? 'Admin portal' : 'Track shifts. Earn smarter.'}
            </p>
          </div>
        </div>

        <nav className="flex flex-col gap-0.5">
          {!isAdmin &&
            navItems.map(({ to, label, icon: Icon, end }) => (
              <NavLink key={to} to={to} end={end}>
                {({ isActive }) => (
                  <span
                    className={cn(
                      'flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-primary text-primary-foreground shadow-sm'
                        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                    )}
                  >
                    <Icon className="h-4 w-4 shrink-0" />
                    {label}
                  </span>
                )}
              </NavLink>
            ))}
          {isAdmin && (
            <NavLink to="/admin" end>
              {({ isActive }) => (
                <span
                  className={cn(
                    'flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium',
                    isActive ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-accent',
                  )}
                >
                  <Shield className="h-4 w-4" /> Admin Analytics
                </span>
              )}
            </NavLink>
          )}
        </nav>

        {!isAdmin && <QuickStartGuide />}

        <div className="mt-auto space-y-3">
          <Separator />
          <div className="rounded-lg bg-muted/50 p-3 text-sm">
            <p className="font-medium">
              {user?.firstName} {user?.lastName}
            </p>
            <p className="truncate text-muted-foreground text-xs">{user?.email}</p>
          </div>
          <Button variant="outline" className="w-full" onClick={() => void handleLogout()}>
            <LogOut className="h-4 w-4" /> Logout
          </Button>
        </div>
      </aside>

      <main className="min-w-0 p-4 md:p-6 lg:p-8">
        <div className="mb-4 flex items-center justify-end gap-2 lg:hidden">
          <Button variant="outline" size="sm" onClick={() => void handleLogout()}>
            <LogOut className="h-4 w-4" /> Logout
          </Button>
        </div>
        {currentHint && !isAdmin && (
          <p className="mb-4 hidden text-muted-foreground text-xs lg:block">{currentHint}</p>
        )}
        <Outlet />
        {!isAdmin && <AuraWidget />}
      </main>

      {!isAdmin && (
        <div className="fixed top-4 right-4 z-50">
          <NotificationBell placement="bottom-end" />
        </div>
      )}
    </div>
  );
}
