import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { getProjectById, updateProject } from '../../../api/projects.api';
import { getAllUsers } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { AlertCircle, ArrowLeft, Save } from 'lucide-react';

export default function EditProjectPage() {
  const { id } = useParams<{ id: string }>();
  const projectId = Number(id);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: project, isLoading: loadingProject } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => getProjectById(projectId),
  });
  const { data: users } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const managers = users?.filter((u) => u.role === 'Manager' && u.isActive) || [];

  const [form, setForm] = useState({
    name: '', description: '', startDate: '', endDate: '', status: 'Planned', managerId: '', totalStoryPoints: '',
  });
  const [error, setError] = useState('');

  useEffect(() => {
    if (project) {
      setForm({
        name: project.name,
        description: project.description || '',
        startDate: project.startDate.split('T')[0],
        endDate: project.endDate.split('T')[0],
        status: project.status,
        managerId: project.managerId ? String(project.managerId) : '',
        totalStoryPoints: String(project.totalStoryPoints),
      });
    }
  }, [project]);

  const mutation = useMutation({
    mutationFn: () => updateProject(projectId, {
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
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      navigate('/admin/projects');
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to update project.'),
  });

  if (loadingProject) return <LoadingSpinner />;
  if (!project) return <div className="text-center py-12 text-text-muted">Project not found.</div>;

  return (
    <div>
      <PageHeader title={`Edit Project: ${project.name}`} subtitle={`ID: ${project.id} · SP Completed: ${project.storyPointsCompleted}`}>
        <Link to="/admin/projects" className="btn-ghost text-sm"><ArrowLeft size={16} /> Back</Link>
      </PageHeader>

      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}

      <form onSubmit={(e) => { e.preventDefault(); setError(''); mutation.mutate(); }} className="glass-card p-6 max-w-2xl space-y-4">
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
              <option value="Completed">Completed</option>
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
            <Save size={16} /> {mutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
}
