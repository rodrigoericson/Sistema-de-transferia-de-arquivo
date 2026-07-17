import { create } from 'zustand';
import api from '../lib/api';
import type { ApiResponse, LoginResponse } from '../types';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}

function getStoredToken(): string | null {
  const token = sessionStorage.getItem('sta_token');
  if (token && isTokenExpired(token)) {
    sessionStorage.removeItem('sta_token');
    sessionStorage.removeItem('sta_username');
    sessionStorage.removeItem('sta_role');
    return null;
  }
  return token;
}

interface AuthState {
  token: string | null;
  username: string | null;
  role: string | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<{ success: boolean; message?: string }>;
  logout: () => void;
}

export const useAuth = create<AuthState>((set) => ({
  token: getStoredToken(),
  username: getStoredToken() ? sessionStorage.getItem('sta_username') : null,
  role: getStoredToken() ? sessionStorage.getItem('sta_role') : null,
  isAuthenticated: !!getStoredToken(),

  login: async (username: string, password: string) => {
    try {
      const { data } = await api.post<ApiResponse<LoginResponse>>('/auth/login', { username, password });
      if (data.success && data.data) {
        const { token, username: user, role } = data.data;
        sessionStorage.setItem('sta_token', token);
        sessionStorage.setItem('sta_username', user);
        sessionStorage.setItem('sta_role', role);
        set({ token, username: user, role, isAuthenticated: true });
        return { success: true };
      }
      return { success: false, message: data.message || 'Credenciais inválidas.' };
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        return { success: false, message: axiosErr.response?.data?.message || 'Credenciais inválidas.' };
      }
      return { success: false, message: 'Erro de conexão.' };
    }
  },

  logout: () => {
    sessionStorage.removeItem('sta_token');
    sessionStorage.removeItem('sta_username');
    sessionStorage.removeItem('sta_role');
    set({ token: null, username: null, role: null, isAuthenticated: false });
  },
}));
