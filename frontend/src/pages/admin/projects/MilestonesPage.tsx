import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getProjectById, getMilestonesByProjectId, addMilestone, updateMilestoneStatus } from '../../../api/projects.api';
import PageHeader from '../../../components/ui/PageHeader';

import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import EmptyState from '../../../components/ui/EmptyState';
import { Plus, ArrowLeft, AlertCircle, ChevronDown, ChevronUp } from 'lucide-react';
import { formatDate } from '../../../lib/utils';

export default function MilestonesPage() {
  const { id } = useParams<{ id: string }>();
  const projectId = Number(id);
  const queryClient = useQueryClient();

  const { data: project, isLoading: loadingProject } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => getProjectById(projectId),
  });
  const { data: milestones, isLoading: loadingMilestones } = useQuery({
    queryKey: ['milestones', projectId],
    queryFn: () => getMilestonesByProjectId(projectId),
  });

  const [showAddForm, setShowAddForm] = useState(false);
  const [form, setForm] = useState({ title: '', dueDate: '', storyPoints: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const addMut = useMutation({
    mutationFn: () => addMilestone(projectId, {
      title: form.title,
      dueDate: form.dueDate,
      storyPoints: Number(form.storyPoints),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['milestones', projectId] });
      setForm({ title: '', dueDate: '', storyPoints: '' });
      setShowAddForm(false);
      setSuccess('Milestone added.');
      setError('');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to add milestone.'),
  });

  const statusMut = useMutation({
    mutationFn: ({ milestoneId, status }: { milestoneId: number; status: string }) =>
      updateMilestoneStatus(milestoneId, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['milestones', projectId] });
      setSuccess('Status updated.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to update status.'),
  });

  const isLoading = loadingProject || loadingMilestones;
  if (isLoading) return <LoadingSpinner />;
  if (!project) return <div className="text-center py-12 text-text-muted">Project not found.</div>;

  return (
    <div>
      <PageHeader title={`Milestones — ${project.name}`} subtitle={`${milestones?.length || 0} milestones`}>
        <Link to="/admin/projects" className="btn-ghost text-sm"><ArrowLeft size={16} /> Back to Projects</Link>
      </PageHeader>

      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 animate-fade-in">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      {success && (
        <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success animate-fade-in">{success}</div>
      )}

      {/* Add Milestone Collapsible */}
      <div className="glass-card mb-6">
        <button
          onClick={() => setShowAddForm(!showAddForm)}
          className="w-full flex items-center justify-between p-4 text-left"
        >
          <span className="text-sm font-semibold text-text-primary flex items-center gap-2"><Plus size={16} /> Add Milestone</span>
          {showAddForm ? <ChevronUp size={16} className="text-text-muted" /> : <ChevronDown size={16} className="text-text-muted" />}
        </button>
        {showAddForm && (
          <form onSubmit={(e) => { e.preventDefault(); setError(''); addMut.mutate(); }} className="px-4 pb-4 space-y-3 border-t border-border pt-4 animate-slide-up">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-1">Title *</label>
              <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} className="input" required />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Due Date *</label>
                <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} className="input" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Story Points *</label>
                <input type="number" value={form.storyPoints} onChange={(e) => setForm({ ...form, storyPoints: e.target.value })} className="input" required min={0} />
              </div>
            </div>
            <div className="flex justify-end gap-2">
              <button type="button" onClick={() => setShowAddForm(false)} className="btn-ghost text-sm">Cancel</button>
              <button type="submit" className="btn-primary text-sm" disabled={addMut.isPending}>
                {addMut.isPending ? 'Adding...' : 'Add Milestone'}
              </button>
            </div>
          </form>
        )}
      </div>

      {/* Milestones Table */}
      {!milestones || milestones.length === 0 ? (
        <EmptyState title="No milestones yet" message="Add your first milestone using the form above." />
      ) : (
        <div className="glass-card overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="table-header">#</th>
                <th className="table-header">Title</th>
                <th className="table-header">Due Date</th>
                <th className="table-header">Story Points</th>
                <th className="table-header">Status</th>
              </tr>
            </thead>
            <tbody>
              {milestones.map((m, i) => (
                <tr key={m.id} className="table-row">
                  <td className="table-cell text-text-muted">{i + 1}</td>
                  <td className="table-cell font-medium text-text-primary">{m.title}</td>
                  <td className="table-cell">{formatDate(m.dueDate)}</td>
                  <td className="table-cell">{m.storyPoints}</td>
                  <td className="table-cell">
                    <select
                      value={m.status}
                      onChange={(e) => statusMut.mutate({ milestoneId: m.id, status: e.target.value })}
                      className="input py-1 px-2 text-xs w-auto min-w-[110px]"
                      disabled={statusMut.isPending}
                    >
                      <option value="NotStarted">Not Started</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Done">Done</option>
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
