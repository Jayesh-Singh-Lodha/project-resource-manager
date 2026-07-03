import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { changePassword } from '../api/auth.api';
import { Lock, AlertCircle, Shield } from 'lucide-react';

export default function ChangePasswordPage() {
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { role, clearForcePasswordChange } = useAuthStore();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }

    setLoading(true);
    try {
      await changePassword({ newPassword, confirmPassword });
      clearForcePasswordChange();

      const dashboardMap: Record<string, string> = {
        Admin: '/admin',
        Manager: '/manager',
        Employee: '/employee',
      };
      navigate(dashboardMap[role || ''] || '/login');
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data?.Message || 'Failed to change password.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background relative overflow-hidden">
      <div className="absolute top-1/4 -left-32 w-96 h-96 bg-accent/5 rounded-full blur-3xl" />
      <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-violet/5 rounded-full blur-3xl" />

      <div className="relative w-full max-w-md mx-4 animate-slide-up">
        <div className="glass-card p-8">
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-warning to-danger mb-4">
              <Shield size={28} className="text-white" />
            </div>
            <h1 className="text-2xl font-bold text-text-primary">Change Password</h1>
            <p className="text-sm text-text-muted mt-1">You must set a new password to continue.</p>
          </div>

          {error && (
            <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 animate-fade-in">
              <AlertCircle size={16} className="text-danger shrink-0" />
              <p className="text-sm text-danger">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1.5">New Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
                <input
                  id="new-password"
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="input pl-10"
                  placeholder="Min 8 characters, 1 uppercase, 1 number"
                  required
                  autoFocus
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1.5">Confirm Password</label>
              <div className="relative">
                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
                <input
                  id="confirm-password"
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  className="input pl-10"
                  placeholder="Re-enter new password"
                  required
                />
              </div>
            </div>

            <button
              id="change-password-submit"
              type="submit"
              className="btn-primary w-full py-2.5"
              disabled={loading}
            >
              {loading ? 'Updating...' : 'Save & Continue'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
