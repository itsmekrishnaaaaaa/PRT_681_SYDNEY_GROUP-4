import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { clearStoredAuth } from '@/shared/api/client';
import type { AuthResponse, RegisterResponse, UserProfile } from '@/shared/types';
import * as authApi from './api';
import type { RegisterPayload } from './api';

interface AuthContextValue {
  user: UserProfile | null;
  loading: boolean;
  isAdmin: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<RegisterResponse>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const persistAuth = (data: AuthResponse) => {
  localStorage.setItem('auth', JSON.stringify(data));
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const init = async () => {
      const raw = localStorage.getItem('auth');
      if (!raw) {
        setLoading(false);
        return;
      }

      try {
        const profile = await authApi.getMe();
        setUser(profile);
      } catch {
        clearStoredAuth();
      } finally {
        setLoading(false);
      }
    };

    // Safety net: if /auth/me (plus a possible token refresh) has not answered in 20s, treat the session as
    // gone so the app never sits on the loading screen.
    const timeout = window.setTimeout(() => {
      clearStoredAuth();
      setLoading(false);
    }, 20000);

    void init().finally(() => window.clearTimeout(timeout));

    return () => window.clearTimeout(timeout);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const data = await authApi.login(email, password);
    persistAuth(data);
    setUser(data.user);
  }, []);

  const register = useCallback(async (payload: RegisterPayload) => {
    return authApi.register(payload);
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // Ignore logout API errors and clear local session anyway.
    }
    clearStoredAuth();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      loading,
      isAdmin: user?.roles.includes('Admin') ?? false,
      login,
      register,
      logout,
    }),
    [user, loading, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
