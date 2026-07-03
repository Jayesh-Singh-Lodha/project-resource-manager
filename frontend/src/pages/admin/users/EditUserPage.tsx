import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getAllUsers, updateUser, deactivateUser, reactivateUser, resetPassword, assignManager } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import ConfirmDialog from '../../../components/ui/ConfirmDialog';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import StatusBadge from '../../../components/ui/StatusBadge';
import { AlertCircle, Check, Copy, ArrowLeft, Save, KeyRound, UserCog, Power } from 'lucide-react';

export default function EditUserPage() {
  const { id } = useParams<{ id: string }>();
  const userId = Number(id);

  const queryClient = useQueryClient();

  const { data: users, isLoading } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const user = users?.find((u) => u.id === userId);

  // Edit form state
  const [form, setForm] = useState({ fullName: '', department: '', role: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Reset password state
  const [tempPassword, setTempPassword] = useState('');
  const [showResetConfirm, setShowResetConfirm] = useState(false);

  // Assign manager state
  const [selectedManagerId, setSelectedManagerId] = useState<string>('');

  // Deactivate/Reactivate state
  const [showDeactivateConfirm, setShowDeactivateConfirm] = useState(false);

  useEffect(() => {
    if (user) {
      setForm({ fullName: user.fullName, department: user.department || '', role: user.role });
      setSelectedManagerId(user.managerId ? String(user.managerId) : '');
    }
  }, [user]);

  const managers = users?.filter((u) => u.role === 'Manager' && u.isActive) || [];

  // Update user mutation
  const updateMut = useMutation({
    mutationFn: () => updateUser(userId, {
      fullName: form.fullName,
      department: form.department || null,
      role: form.role,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setSuccess('User updated successfully.');
      setError('');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to update user.'),
  });

  // Reset password mutation
  const resetPwMut = useMutation({
    mutationFn: () => {
      const newTempPw = `Reset${Date.now().toString(36).slice(-4)}!`;
      return resetPassword(userId, { newTemporaryPassword: newTempPw }).then(() => newTempPw);
    },
    onSuccess: (newPw: string) => {
      setTempPassword(newPw);
      setShowResetConfirm(false);
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (err: any) => { setError(err?.response?.data?.message || 'Failed to reset password.'); setShowResetConfirm(false); },
  });

  // Assign manager mutation
  const assignMgrMut = useMutation({
    mutationFn: () => assignManager(userId, selectedManagerId ? Number(selectedManagerId) : null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setSuccess('Manager assigned successfully.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to assign manager.'),
  });

  // Deactivate/Reactivate mutation
  const toggleActiveMut = useMutation({
    mutationFn: () => user?.isActive ? deactivateUser(userId) : reactivateUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setShowDeactivateConfirm(false);
      setSuccess(user?.isActive ? 'User deactivated.' : 'User reactivated.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => { setError(err?.response?.data?.message || 'Failed.'); setShowDeactivateConfirm(false); },
  });

  if (isLoading) return <LoadingSpinner />;
  if (!user) return <div className="text-center py-12 text-text-muted">User not found.</div>;

  return (
    <div>
      <PageHeader title={`Manage User: ${user.fullName}`} subtitle={`ID: ${user.id} · @${user.username}`}>
        <Link to="/admin/users" className="btn-ghost text-sm"><ArrowLeft size={16} /> Back to Users</Link>
      </PageHeader>

      {/* Status Banners */}
      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 animate-fade-in">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      {success && (
        <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success animate-fade-in">{success}</div>
      )}

      {/* Reset Password Result */}
      {tempPassword && (
        <div className="glass-card p-5 mb-6 animate-slide-up">
          <div className="flex items-center gap-2 mb-3">
            <div className="p-2 rounded-full bg-success/10"><Check size={20} className="text-success" /></div>
            <h3 className="text-base font-semibold text-text-primary">Password Reset Successfully</h3>
          </div>
          <p className="text-xs text-text-muted mb-3">The user must change this password on next login.</p>
          <div className="p-3 bg-background rounded-lg border border-border">
            <p className="text-xs text-text-muted mb-1">New Temporary Password (shown once):</p>
            <div className="flex items-center gap-2">
              <code className="text-lg font-mono text-accent">{tempPassword}</code>
              <button onClick={() => navigator.clipboard.writeText(tempPassword)} className="btn-ghost p-1.5"><Copy size={14} /></button>
            </div>
          </div>
          <button onClick={() => setTempPassword('')} className="btn-ghost text-xs mt-2">Dismiss</button>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* --- Edit Details Panel --- */}
        <div className="lg:col-span-2">
          <form onSubmit={(e) => { e.preventDefault(); setError(''); updateMut.mutate(); }} className="glass-card p-6 space-y-4">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide">Edit Details</h3>
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">Full Name *</label>
              <input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} className="input" required />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Role *</label>
                <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })} className="input">
                  <option value="Admin">Admin</option>
                  <option value="Manager">Manager</option>
                  <option value="Employee">Employee</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Department</label>
                <input value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} className="input" placeholder="e.g. Backend" />
              </div>
            </div>
            <div className="flex justify-end">
              <button type="submit" className="btn-primary" disabled={updateMut.isPending}>
                <Save size={16} /> {updateMut.isPending ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </form>

          {/* Assign Manager */}
          {user.role === 'Employee' && (
            <div className="glass-card p-6 mt-6 space-y-4">
              <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide flex items-center gap-2"><UserCog size={16} /> Assign Manager</h3>
              <div className="flex items-end gap-3">
                <div className="flex-1">
                  <label className="block text-sm font-medium text-text-secondary mb-1">Manager</label>
                  <select value={selectedManagerId} onChange={(e) => setSelectedManagerId(e.target.value)} className="input">
                    <option value="">None (unassign)</option>
                    {managers.map((m) => <option key={m.id} value={m.id}>{m.fullName} (ID: {m.id})</option>)}
                  </select>
                </div>
                <button onClick={() => assignMgrMut.mutate()} className="btn-primary text-sm" disabled={assignMgrMut.isPending}>
                  {assignMgrMut.isPending ? 'Assigning...' : 'Assign'}
                </button>
              </div>
              {user.managerId && (
                <p className="text-xs text-text-muted">Current manager ID: <span className="font-mono text-text-primary">{user.managerId}</span></p>
              )}
            </div>
          )}
        </div>

        {/* --- Sidebar Actions --- */}
        <div className="space-y-4">
          {/* Status Card */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3">Account Status</h3>
            <div className="space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-text-muted">Status</span>
                <StatusBadge status={user.isActive ? 'Active' : 'Inactive'} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-text-muted">Role</span>
                <StatusBadge status={user.role} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-text-muted">Utilisation</span>
                <span className="text-text-primary font-medium">{user.currentUtilisationPercent}%</span>
              </div>
              {user.isTimesheetFrozen && (
                <div className="flex items-center justify-between">
                  <span className="text-text-muted">Timesheet</span>
                  <span className="text-[10px] px-2 py-0.5 rounded-full bg-danger/10 text-danger border border-danger/20 font-medium">FROZEN</span>
                </div>
              )}
            </div>
          </div>

          {/* Reset Password */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3 flex items-center gap-2"><KeyRound size={16} /> Reset Password</h3>
            <p className="text-xs text-text-muted mb-3">Generates a new temporary password. The user will be forced to change it on next login.</p>
            <button onClick={() => setShowResetConfirm(true)} className="btn-secondary w-full text-sm">Reset Password</button>
          </div>

          {/* Deactivate / Reactivate */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3 flex items-center gap-2"><Power size={16} /> {user.isActive ? 'Deactivate' : 'Reactivate'} Account</h3>
            {user.isActive ? (
              <p className="text-xs text-text-muted mb-3">Deactivating will end all active allocations and prevent login.</p>
            ) : (
              <p className="text-xs text-text-muted mb-3">Reactivating will allow the user to log in again.</p>
            )}
            <button
              onClick={() => setShowDeactivateConfirm(true)}
              className={user.isActive ? 'btn-danger w-full text-sm' : 'btn-primary w-full text-sm'}
            >
              {user.isActive ? 'Deactivate Account' : 'Reactivate Account'}
            </button>
          </div>
        </div>
      </div>

      {/* Confirm Dialogs */}
      <ConfirmDialog
        open={showResetConfirm}
        title="Reset Password"
        message={`Reset password for ${user.fullName}? A new temporary password will be generated.`}
        confirmLabel="Yes, Reset"
        onConfirm={() => resetPwMut.mutate()}
        onCancel={() => setShowResetConfirm(false)}
        loading={resetPwMut.isPending}
      />
      <ConfirmDialog
        open={showDeactivateConfirm}
        title={user.isActive ? 'Deactivate User' : 'Reactivate User'}
        message={user.isActive
          ? `Deactivate ${user.fullName}? All active allocations will be ended immediately.`
          : `Reactivate ${user.fullName}? They will be able to log in again.`}
        confirmLabel={user.isActive ? 'Yes, Deactivate' : 'Yes, Reactivate'}
        onConfirm={() => toggleActiveMut.mutate()}
        onCancel={() => setShowDeactivateConfirm(false)}
        loading={toggleActiveMut.isPending}
      />
    </div>
  );
}
