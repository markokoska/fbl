import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import api from '../api/client';
import type { AuthResponse } from '../api/types';

interface AuthUser {
  userId: string;
  displayName: string;
  email: string;
  roles: string[];
}

interface AuthContextType {
  user: AuthUser | null;
  isAdmin: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('fbl_user');
    if (stored) {
      setUser(JSON.parse(stored));
    }
    setLoading(false);
  }, []);

  const handleAuth = (data: AuthResponse) => {
    localStorage.setItem('fbl_token', data.token);
    const u: AuthUser = {
      userId: data.userId,
      displayName: data.displayName,
      email: data.email,
      roles: data.roles,
    };
    localStorage.setItem('fbl_user', JSON.stringify(u));
    setUser(u);
  };

  const login = async (email: string, password: string) => {
    const res = await api.post<AuthResponse>('/auth/login', { email, password });
    handleAuth(res.data);
  };

  const register = async (email: string, displayName: string, password: string) => {
    const res = await api.post<AuthResponse>('/auth/register', { email, displayName, password });
    handleAuth(res.data);
  };

  const logout = () => {
    localStorage.removeItem('fbl_token');
    localStorage.removeItem('fbl_user');
    setUser(null);
  };

  const isAdmin = user?.roles.includes('Admin') ?? false;

  return (
    <AuthContext.Provider value={{ user, isAdmin, login, register, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
