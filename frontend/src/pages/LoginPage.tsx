import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { login as loginApi } from '../api/auth.api';
import { Lock, User, AlertCircle, Zap } from 'lucide-react';
import type { UserRole } from '../types';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const authLogin = useAuthStore((s) => s.login);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await loginApi({ username, password });
      authLogin(response.token, response.role as UserRole, response.fullName, response.forcePasswordChange);

      if (response.forcePasswordChange) {
        navigate('/change-password');
      } else {
        const dashboardMap: Record<string, string> = {
          Admin: '/admin',
          Manager: '/manager',
          Employee: '/employee',
        };
        navigate(dashboardMap[response.role] || '/login');
      }
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data?.Message || 'Invalid credentials. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background relative overflow-hidden">
      {/* Background gradient orbs */}
      <div className="absolute top-1/4 -left-32 w-96 h-96 bg-accent/5 rounded-full blur-3xl" />
      <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-violet/5 rounded-full blur-3xl" />

      <div className="relative w-full max-w-md mx-4 animate-slide-up">
        {/* Logo Card */}
        <div className="glass-card p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-accent to-violet mb-4 shadow-glow">
              <Zap size={28} className="text-white" />
            </div>
            <h1 className="text-2xl font-bold text-text-primary">PRM Tool</h1>
            <p className="text-sm text-text-muted mt-1">Project &amp; Resource Management</p>
          </div>

          {/* Error */}
          {error && (
            <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 animate-fade-in">
              <AlertCircle size={16} className="text-danger shrink-0" />
              <p className="text-sm text-danger">{error}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1.5">Username</label>
              <div className="relative">
                <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
                <input
                  id="login-username"
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="input pl-10"
                  placeholder="Enter username"
                  required
                  autoFocus
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1.5">Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
                <input
                  id="login-password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="input pl-10"
                  placeholder="Enter password"
                  required
                />
              </div>
            </div>

            <button
              id="login-submit"
              type="submit"
              className="btn-primary w-full py-2.5"
              disabled={loading}
            >
              {loading ? 'Signing in...' : 'Sign In'}
            </button>
          </form>
        </div>

        <p className="text-center text-xs text-text-muted mt-4">
          Learn &amp; Code — Final Project
        </p>
      </div>
    </div>
  );
}
