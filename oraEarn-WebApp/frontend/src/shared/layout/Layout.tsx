import { useEffect, useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Briefcase, CalendarDays, LayoutDashboard, LogOut, Menu, PiggyBank, Receipt, Shield, Sparkles, X } from 'lucide-react';
import { AuraWidget } from '@/features/aura/AuraWidget';
import { NotificationBell } from '@/features/notifications/components/NotificationBell';
import { pageHints, QuickStartGuide } from '@/shared/layout/QuickStartGuide';
import { useAuth } from '@/auth';
import { Button } from '@/shared/ui/button';
import { Separator } from '@/shared/ui/separator';
import { cn } from '@/shared/lib/utils';

const navItems = [
  { to: '/', label: 'Analytics', short: 'Home', icon: LayoutDashboard, end: true },
  { to: '/jobs', label: 'Jobs', short: 'Jobs', icon: Briefcase },
  { to: '/shifts', label: 'Shifts', short: 'Shifts', icon: CalendarDays },
  { to: '/savings', label: 'Savings Goals', short: 'Savings', icon: PiggyBank },
  { to: '/expenses', label: 'Expenses', short: 'Spend', icon: Receipt },
];

export function Layout() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    if (!menuOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMenuOpen(false);
    };
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', onKey);
    return () => {
      document.body.style.overflow = '';
      window.removeEventListener('keydown', onKey);
    };
  }, [menuOpen]);

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const currentHint = pageHints[location.pathname];

  const navLinkClass = (isActive: boolean) =>
    cn(
      'flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
      isActive
        ? 'bg-primary text-primary-foreground shadow-sm'
        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
    );

  const sidebar = (
    <>
      <div className="flex items-center gap-3 rounded-xl bg-primary p-3 text-primary-foreground">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-primary-foreground/15 font-bold">
          OE
        </div>
        <div className="min-w-0">
          <p className="font-semibold tracking-tight">OraEarn</p>
          <p className="flex items-center gap-1 truncate text-xs text-primary-foreground/80">
            <Sparkles className="h-3 w-3 shrink-0" />
            {isAdmin ? 'Admin portal' : 'Track shifts. Earn smarter.'}
          </p>
        </div>
      </div>

      <nav className="flex flex-col gap-0.5">
        {!isAdmin &&
          navItems.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end}>
              {({ isActive }) => (
                <span className={navLinkClass(isActive)}>
                  <Icon className="h-4 w-4 shrink-0" />
                  {label}
                </span>
              )}
            </NavLink>
          ))}
        {isAdmin && (
          <NavLink to="/admin" end>
            {({ isActive }) => (
              <span className={navLinkClass(isActive)}>
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
    </>
  );

  return (
    <div className="min-h-dvh bg-muted/30 lg:grid lg:grid-cols-[16rem_minmax(0,1fr)] xl:grid-cols-[18rem_minmax(0,1fr)]">
      <header className="sticky top-0 z-40 flex items-center gap-2 border-b bg-card/95 px-3 py-2 backdrop-blur lg:hidden pt-[max(0.5rem,env(safe-area-inset-top))]">
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="h-10 w-10 shrink-0"
          aria-label={menuOpen ? 'Close menu' : 'Open menu'}
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((v) => !v)}
        >
          {menuOpen ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
        </Button>
        <div className="min-w-0 flex-1">
          <p className="truncate font-semibold leading-tight">OraEarn</p>
          <p className="truncate text-[11px] text-muted-foreground">
            {isAdmin ? 'Admin portal' : (currentHint ?? 'Track shifts. Earn smarter.')}
          </p>
        </div>
        {!isAdmin && <NotificationBell placement="bottom-end" />}
      </header>

      {menuOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-black/50 lg:hidden"
          aria-label="Close menu"
          onClick={() => setMenuOpen(false)}
        />
      )}

      <aside
        className={cn(
          'flex flex-col gap-6 overflow-y-auto bg-card p-4',
          'fixed inset-y-0 left-0 z-50 w-[min(18rem,calc(100vw-3rem))] border-r pt-[max(1rem,env(safe-area-inset-top))] transition-transform duration-200',
          'lg:static lg:z-auto lg:w-auto lg:translate-x-0 lg:border-b-0 lg:pt-4',
          menuOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
        )}
      >
        {sidebar}
      </aside>

      <main
        className={cn(
          'min-w-0 overflow-x-hidden px-3 py-4 sm:px-4 md:p-6 lg:p-8',
          !isAdmin && 'pb-[calc(5.25rem+env(safe-area-inset-bottom))] lg:pb-8',
        )}
      >
        {currentHint && !isAdmin && (
          <p className="mb-4 hidden text-muted-foreground text-xs lg:block">{currentHint}</p>
        )}
        <Outlet />
        {!isAdmin && <AuraWidget />}
      </main>

      {!isAdmin && (
        <nav
          className="fixed inset-x-0 bottom-0 z-40 grid grid-cols-5 border-t bg-card/95 pb-[env(safe-area-inset-bottom)] backdrop-blur lg:hidden"
          aria-label="Primary"
        >
          {navItems.map(({ to, short, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end} className="min-w-0">
              {({ isActive }) => (
                <span
                  className={cn(
                    'flex flex-col items-center justify-center gap-0.5 px-1 py-2 text-[10px] font-medium',
                    isActive ? 'text-foreground' : 'text-muted-foreground',
                  )}
                >
                  <Icon className={cn('h-5 w-5', isActive && 'stroke-[2.25]')} />
                  <span className="truncate">{short}</span>
                </span>
              )}
            </NavLink>
          ))}
        </nav>
      )}

      {!isAdmin && (
        <div className="fixed top-4 right-4 z-50 hidden lg:block">
          <NotificationBell placement="bottom-end" />
        </div>
      )}
    </div>
  );
}
