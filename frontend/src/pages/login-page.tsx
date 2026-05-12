import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, LoginSchema } from '@/features/auth/schemas/login-schema';
import { login } from '@/features/auth/api/auth-api';
import { useAuthStore } from '@/store/auth-store';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';

export function LoginPage() {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginSchema>({ resolver: zodResolver(loginSchema) });
  const setTokens = useAuthStore((s) => s.setTokens);
  const navigate = useNavigate();
  return <div className="min-h-screen grid place-items-center"><form className="w-full max-w-md p-8 bg-white rounded-2xl shadow" onSubmit={handleSubmit(async (v) => { try { const t = await login(v); setTokens(t.accessToken, t.refreshToken); navigate('/dashboard'); } catch { toast.error('Login failed'); } })}><h1 className="text-2xl font-semibold mb-6">LedgerFlow Dashboard</h1><input {...register('email')} placeholder="Email" className="w-full border rounded-lg p-3 mb-2" /><p className="text-xs text-red-500">{errors.email?.message}</p><input {...register('password')} type="password" placeholder="Password" className="w-full border rounded-lg p-3 mb-2" /><p className="text-xs text-red-500">{errors.password?.message}</p><button disabled={isSubmitting} className="w-full mt-4 bg-slate-900 text-white rounded-lg p-3">Sign in</button></form></div>;
}
