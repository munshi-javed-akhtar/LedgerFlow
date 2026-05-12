import { createBrowserRouter, Navigate } from 'react-router-dom';
import { DashboardLayout } from '@/layouts/dashboard-layout';
import { ProtectedRoute } from './protected-route';
import { DashboardPage } from '@/pages/dashboard-page';
import { LoginPage } from '@/pages/login-page';
import { WalletsPage } from '@/pages/wallets-page';
import { TransactionsPage } from '@/pages/transactions-page';

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { element: <ProtectedRoute />, children: [{ element: <DashboardLayout />, children: [
    { path: '/dashboard', element: <DashboardPage /> },
    { path: '/wallets', element: <WalletsPage /> },
    { path: '/transactions', element: <TransactionsPage /> }
  ] }] },
  { path: '*', element: <Navigate to="/dashboard" replace /> }
]);
