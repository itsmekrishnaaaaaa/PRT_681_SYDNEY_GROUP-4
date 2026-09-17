export const API_URL =
  import.meta.env.VITE_API_URL ||
  (import.meta.env.DEV ? 'http://localhost:5203/api' : '');

export const HUB_URL =
  import.meta.env.VITE_HUB_URL ||
  (import.meta.env.DEV ? 'http://localhost:5203/hubs/shifts' : '');

if (!API_URL || !HUB_URL) {
  throw new Error('VITE_API_URL and VITE_HUB_URL must be set at build time for production.');
}
