import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createProject } from '../../../api/projects.api';
import { getAllUsers } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import { AlertCircle } from 'lucide-react';

export default function CreateProjectPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { data: users } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const managers = users?.filter((u) => u.role === 'Manager' && u.isActive) || [];

  const [form, setForm] = useState({
    name: '', description: '', startDate: '', endDate: '', status: 'Planned', managerId: '', totalStoryPoints: '',
  });
  const [error, setError] = useState('');

  const mutation = useMutation({
    mutationFn: () => createProject({
      name: form.name,
      description: form.description || null,
      startDate: form.startDate,
      endDate: form.endDate,
      status: form.status,
      managerId: form.managerId ? Number(form.managerId) : null,
      totalStoryPoints: Number(form.totalStoryPoints),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      navigate('/admin/projects');
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to create project.'),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    mutation.mutate();
  };

  return (
    <div>
      <PageHeader title="Create Project" />
      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      <form onSubmit={handleSubmit} className="glass-card p-6 max-w-2xl space-y-4">
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Project Name *</label>
          <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="input" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Description</label>
          <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="input" rows={3} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1">Start Date *</label>
            <input type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} className="input" required />
          </div>
          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1">End Date *</label>
            <input type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} className="input" required />
          </div>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1">Status</label>
            <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })} className="input">
              <option value="Planned">Planned</option>
              <option value="Active">Active</option>
              <option value="OnHold">On Hold</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1">Total Story Points *</label>
            <input type="number" value={form.totalStoryPoints} onChange={(e) => setForm({ ...form, totalStoryPoints: e.target.value })} className="input" required min={0} />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-text-secondary mb-1">Assign Manager</label>
          <select value={form.managerId} onChange={(e) => setForm({ ...form, managerId: e.target.value })} className="input">
            <option value="">None</option>
            {managers.map((m) => <option key={m.id} value={m.id}>{m.fullName} (ID: {m.id})</option>)}
          </select>
        </div>
        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={() => navigate('/admin/projects')} className="btn-secondary">Cancel</button>
          <button type="submit" className="btn-primary" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Project'}
          </button>
        </div>
      </form>
    </div>
  );
}
