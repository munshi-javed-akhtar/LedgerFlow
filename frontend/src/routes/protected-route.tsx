import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/store/auth-store';

export function ProtectedRoute() {
  return useAuthStore((x) => x.accessToken) ? <Outlet /> : <Navigate to="/login" replace />;
}
