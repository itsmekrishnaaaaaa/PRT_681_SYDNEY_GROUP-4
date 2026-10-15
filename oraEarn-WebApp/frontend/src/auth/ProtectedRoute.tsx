import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './AuthContext';

// These guards only decide what the browser shows. The API enforces access itself with [Authorize] and
// role checks; the guards are not a security boundary.

// Signed-in student area. While AuthContext is still checking the stored session, show a placeholder
// instead of redirecting, otherwise a page reload would briefly bounce a valid user to /login.
export function ProtectedRoute() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen grid place-items-center text-muted-foreground">
        Loading OraEarn…
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Admin accounts only use the admin area, so they are sent there rather than to the student dashboard.
  if (user.roles.includes('Admin')) {
    return <Navigate to="/admin" replace />;
  }

  return <Outlet />;
}

// Admin-only area; signed-in non-admins go back to the student dashboard.
export function AdminRoute() {
  const { user, loading, isAdmin } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen grid place-items-center text-muted-foreground">
        Loading OraEarn…
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (!isAdmin) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}

// Login and register pages; anyone already signed in is sent to their home page instead.
export function GuestRoute() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen grid place-items-center text-muted-foreground">
        Loading OraEarn…
      </div>
    );
  }

  if (user) {
    return <Navigate to={user.roles.includes('Admin') ? '/admin' : '/'} replace />;
  }

  return <Outlet />;
}
