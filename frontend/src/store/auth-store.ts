import { create } from 'zustand';

type AuthState = { accessToken: string | null; refreshToken: string | null; setTokens: (a: string, r: string) => void; logout: () => void };
export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null, refreshToken: null,
  setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
  logout: () => set({ accessToken: null, refreshToken: null })
}));
