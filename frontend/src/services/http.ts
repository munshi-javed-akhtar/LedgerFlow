import axios from 'axios';
import { useAuthStore } from '@/store/auth-store';

export const http = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8080' });
http.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
