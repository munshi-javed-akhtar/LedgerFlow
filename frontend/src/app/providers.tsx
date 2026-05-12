import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode } from 'react';
import { Toaster } from 'sonner';

const client = new QueryClient();
export function AppProviders({ children }: { children: ReactNode }) {
  return <QueryClientProvider client={client}>{children}<Toaster richColors /></QueryClientProvider>;
}
