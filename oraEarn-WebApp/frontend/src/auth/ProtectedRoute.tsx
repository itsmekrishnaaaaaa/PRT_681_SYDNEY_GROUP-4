import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './AuthContext';

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

  if (user.roles.includes('Admin')) {
    return <Navigate to="/admin" replace />;
  }

  return <Outlet />;
}

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
