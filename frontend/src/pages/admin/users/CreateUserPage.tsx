import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createUser } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import { AlertCircle, Check, Copy } from 'lucide-react';

export default function CreateUserPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: '', email: '', fullName: '', role: 'Employee', department: '' });
  const [error, setError] = useState('');
  const [tempPassword, setTempPassword] = useState('');

  const mutation = useMutation({
    mutationFn: () => createUser({
      ...form,
      department: form.department || null,
    }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setTempPassword(data.temporaryPassword);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to create user.'),
  });

  if (tempPassword) {
    return (
      <div>
        <PageHeader title="User Created" />
        <div className="glass-card p-6 max-w-lg animate-slide-up">
          <div className="flex items-center gap-2 mb-4">
            <div className="p-2 rounded-full bg-success/10"><Check size={20} className="text-success" /></div>
            <h3 className="text-lg font-semibold text-text-primary">Account Created Successfully</h3>
          </div>
          <p className="text-sm text-text-muted mb-4">The user must change this password on first login.</p>
          <div className="p-4 bg-background rounded-lg border border-border">
            <p className="text-xs text-text-muted mb-1">Temporary Password (shown once):</p>
            <div className="flex items-center gap-2">
              <code className="text-lg font-mono text-accent">{tempPassword}</code>
              <button onClick={() => navigator.clipboard.writeText(tempPassword)} className="btn-ghost p-1.5"><Copy size={14} /></button>
            </div>
          </div>
          <button onClick={() => navigate('/admin/users')} className="btn-primary w-full mt-4">Back to Users</button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader title="Create User Account" />
      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      <form onSubmit={(e) => { e.preventDefault(); mutation.mutate(); }} className="glass-card p-6 max-w-lg space-y-4">
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Full Name *</label>
          <input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} className="input" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Email *</label>
          <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} className="input" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Username *</label>
          <input value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} className="input" required />
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
        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={() => navigate('/admin/users')} className="btn-secondary">Cancel</button>
          <button type="submit" className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Account'}
          </button>
        </div>
      </form>
    </div>
  );
}
