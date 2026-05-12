import { http } from '@/services/http';
import { LoginSchema } from '../schemas/login-schema';

export async function login(data: LoginSchema) {
  const res = await http.post('/api/auth/login', data);
  return res.data as { accessToken: string; refreshToken: string };
}
