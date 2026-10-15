import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { API_URL } from './config';

export const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

// _retry marks a request that has already been replayed after a refresh, so a second 401 is final.
type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean };

// The one in-flight refresh, shared by every request that gets a 401 at the same time. The API rotates the
// refresh token on each use, so two parallel refresh calls would send the same token and the second would
// fail and log the user out (NFR-4.4). Reset to null once the refresh settles.
let refreshPromise: Promise<string | null> | null = null;

// Tokens live in localStorage under "auth" so a reload keeps the session. Any script on the page can read
// them, which is the usual trade-off against cookies; the choice is not recorded beyond that.
const getStoredAuth = () => {
  const raw = localStorage.getItem('auth');
  return raw ? JSON.parse(raw) as { accessToken: string; refreshToken: string } : null;
};

export const getAccessToken = () => getStoredAuth()?.accessToken ?? null;

const setStoredAuth = (accessToken: string, refreshToken: string) => {
  const raw = localStorage.getItem('auth');
  const existing = raw ? JSON.parse(raw) : {};
  localStorage.setItem('auth', JSON.stringify({ ...existing, accessToken, refreshToken }));
};

export const clearStoredAuth = () => localStorage.removeItem('auth');

// Reads storage on every request so a token saved by a refresh is picked up straight away.
api.interceptors.request.use((config) => {
  const auth = getStoredAuth();
  if (auth?.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetryConfig | undefined;
    const url = original?.url ?? '';

    // Never retry auth bootstrap endpoints — avoids refresh deadlocks.
    if (url.includes('/auth/refresh') || url.includes('/auth/login')) {
      return Promise.reject(error);
    }

    // Only a 401 on a request that has not been retried yet triggers a refresh.
    if (!original || error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }

    const auth = getStoredAuth();
    if (!auth?.refreshToken) {
      clearStoredAuth();
      return Promise.reject(error);
    }

    original._retry = true;

    if (!refreshPromise) {
      refreshPromise = api
        .post('/auth/refresh', { refreshToken: auth.refreshToken })
        .then((res) => {
          const { accessToken, refreshToken } = res.data;
          setStoredAuth(accessToken, refreshToken);
          return accessToken as string;
        })
        // A failed refresh means the session is over. This clears storage but does not update AuthContext,
        // so the user is only sent to /login on the next page reload.
        .catch(() => {
          clearStoredAuth();
          return null;
        })
        .finally(() => {
          refreshPromise = null;
        });
    }

    const newToken = await refreshPromise;
    if (!newToken) {
      return Promise.reject(error);
    }

    // Replay the original request once with the new access token.
    original.headers.Authorization = `Bearer ${newToken}`;
    return api(original);
  },
);

// Controllers return { message }, while ASP.NET validation failures return ProblemDetails (title/detail),
// so both shapes are checked before falling back to a generic message.
export const getErrorMessage = (error: unknown, fallback = 'Something went wrong') => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data === 'string' && data.trim()) return data;
    if (data && typeof data === 'object') {
      const obj = data as Record<string, unknown>;
      if (typeof obj.message === 'string' && obj.message) return obj.message;
      if (typeof obj.title === 'string' && obj.title) return obj.title;
      if (typeof obj.detail === 'string' && obj.detail) return obj.detail;
    }
    if (error.response?.status === 401) return 'Session expired — please sign in again.';
    if (error.response?.status === 404) return 'Record not found — refresh and try again.';
  }
  if (error instanceof Error && error.message) return error.message;
  return fallback;
};
