import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { API_URL } from './config';

export const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean };

let refreshPromise: Promise<string | null> | null = null;

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

    original.headers.Authorization = `Bearer ${newToken}`;
    return api(original);
  },
);

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
