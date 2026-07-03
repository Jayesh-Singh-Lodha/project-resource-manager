import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getManagedProjects } from '../../api/manager.api';
import { searchResources } from '../../api/manager.api';
import { createAllocation, endAllocation, getProjectAllocations } from '../../api/allocations.api';
import { getTeam } from '../../api/manager.api';
import PageHeader from '../../components/ui/PageHeader';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import ConfirmDialog from '../../components/ui/ConfirmDialog';
import { Bot, UserPlus, XCircle, AlertCircle, Send, Loader2 } from 'lucide-react';
import { formatDate } from '../../lib/utils';
import type { ProjectResponse, AllocationResponse } from '../../types';

export default function AllocateResourcePage() {
  const [mode, setMode] = useState<'ai' | 'direct' | 'end' | null>(null);

  return (
    <div>
      <PageHeader title="Allocate Resource" subtitle="AI-assisted or direct allocation" />

      {/* Mode Selection */}
      {!mode && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button onClick={() => setMode('ai')} className="glass-card p-6 text-left hover:border-accent/30 transition-all group">
            <Bot size={28} className="text-accent mb-3" />
            <h3 className="font-semibold text-text-primary">Find resource using AI</h3>
            <p className="text-xs text-text-muted mt-1">Describe what you need in plain English</p>
          </button>
          <button onClick={() => setMode('direct')} className="glass-card p-6 text-left hover:border-accent/30 transition-all group">
            <UserPlus size={28} className="text-accent mb-3" />
            <h3 className="font-semibold text-text-primary">Direct allocation</h3>
            <p className="text-xs text-text-muted mt-1">I already know who I want</p>
          </button>
          <button onClick={() => setMode('end')} className="glass-card p-6 text-left hover:border-danger/30 transition-all group">
            <XCircle size={28} className="text-danger mb-3" />
            <h3 className="font-semibold text-text-primary">End an allocation</h3>
            <p className="text-xs text-text-muted mt-1">Remove someone from a project</p>
          </button>
        </div>
      )}

      {mode && (
        <button onClick={() => setMode(null)} className="btn-ghost text-xs mb-4">← Back to options</button>
      )}

      {mode === 'ai' && <AiSearchFlow />}
      {mode === 'direct' && <DirectAllocationFlow />}
      {mode === 'end' && <EndAllocationFlow />}
    </div>
  );
}

function AiSearchFlow() {
  const [criteria, setCriteria] = useState('');
  const [result, setResult] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    setLoading(true);
    try {
      const res = await searchResources(criteria);
      setResult(res.response);
    } catch (err: any) {
      setResult('Error: ' + (err?.response?.data?.message || 'Failed to search.'));
    }
    setLoading(false);
  };

  return (
    <div className="glass-card p-6 max-w-3xl">
      <h3 className="text-lg font-semibold text-text-primary mb-4">AI Resource Search</h3>
      <div className="space-y-4">
        <div>
          <label className="block text-sm text-text-secondary mb-1">Describe your requirement</label>
          <textarea
            value={criteria}
            onChange={(e) => setCriteria(e.target.value)}
            className="input"
            rows={3}
            placeholder="e.g. I need a backend developer with Java and microservices experience, available for at least 3 months from June"
          />
        </div>
        <button onClick={handleSearch} className="btn-primary" disabled={loading || !criteria.trim()}>
          {loading ? <><Loader2 size={16} className="animate-spin" /> Searching...</> : <><Send size={16} /> Search</>}
        </button>

        {result && (
          <div className="p-4 bg-background rounded-lg border border-border animate-slide-up">
            <p className="text-xs text-text-muted mb-2">AI Response:</p>
            <div className="text-sm text-text-primary whitespace-pre-wrap">{result}</div>
            <p className="text-[10px] text-text-muted mt-3 italic">Note: AI-generated suggestions. Verify before confirming allocation.</p>
          </div>
        )}
      </div>
    </div>
  );
}

function DirectAllocationFlow() {
  const queryClient = useQueryClient();
  const { data: projects } = useQuery({ queryKey: ['managed-projects'], queryFn: getManagedProjects });
  const { data: team } = useQuery({ queryKey: ['team'], queryFn: getTeam });
  const [form, setForm] = useState({ projectId: '', userId: '', utilisationPercent: '', fromDate: '', toDate: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const mutation = useMutation({
    mutationFn: () => createAllocation({
      projectId: Number(form.projectId),
      userId: Number(form.userId),
      utilisationPercent: Number(form.utilisationPercent),
      fromDate: form.fromDate,
      toDate: form.toDate,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['team'] });
      setSuccess('Allocation saved successfully!');
      setForm({ projectId: '', userId: '', utilisationPercent: '', fromDate: '', toDate: '' });
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to allocate.'),
  });

  return (
    <div className="glass-card p-6 max-w-xl">
      <h3 className="text-lg font-semibold text-text-primary mb-4">Direct Allocation</h3>
      {error && <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20"><AlertCircle size={16} className="text-danger" /><p className="text-sm text-danger">{error}</p></div>}
      {success && <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success">{success}</div>}

      <form onSubmit={(e) => { e.preventDefault(); setError(''); setSuccess(''); mutation.mutate(); }} className="space-y-4">
        <div>
          <label className="block text-sm text-text-secondary mb-1">Project</label>
          <select value={form.projectId} onChange={(e) => setForm({ ...form, projectId: e.target.value })} className="input" required>
            <option value="">Select project...</option>
            {(projects as ProjectResponse[])?.map((p) => <option key={p.id} value={p.id}>{p.name} (ID: {p.id})</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm text-text-secondary mb-1">Employee</label>
          <select value={form.userId} onChange={(e) => setForm({ ...form, userId: e.target.value })} className="input" required>
            <option value="">Select employee...</option>
            {team?.map((e) => <option key={e.id} value={e.id}>{e.fullName} ({e.currentUtilisationPercent}% used)</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm text-text-secondary mb-1">Utilisation %</label>
          <input type="number" min={1} max={100} value={form.utilisationPercent} onChange={(e) => setForm({ ...form, utilisationPercent: e.target.value })} className="input" required />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm text-text-secondary mb-1">From Date</label>
            <input type="date" value={form.fromDate} onChange={(e) => setForm({ ...form, fromDate: e.target.value })} className="input" required />
          </div>
          <div>
            <label className="block text-sm text-text-secondary mb-1">To Date</label>
            <input type="date" value={form.toDate} onChange={(e) => setForm({ ...form, toDate: e.target.value })} className="input" required />
          </div>
        </div>
        <button type="submit" className="btn-primary w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Allocating...' : 'Confirm Allocation'}
        </button>
      </form>
    </div>
  );
}

function EndAllocationFlow() {
  const queryClient = useQueryClient();
  const { data: projects } = useQuery({ queryKey: ['managed-projects'], queryFn: getManagedProjects });
  const [selectedProject, setSelectedProject] = useState('');
  const [allocations, setAllocations] = useState<AllocationResponse[]>([]);
  const [loadingAlloc, setLoadingAlloc] = useState(false);
  const [confirmEnd, setConfirmEnd] = useState<AllocationResponse | null>(null);

  const handleProjectChange = async (projectId: string) => {
    setSelectedProject(projectId);
    if (!projectId) { setAllocations([]); return; }
    setLoadingAlloc(true);
    try {
      const data = await getProjectAllocations(Number(projectId));
      setAllocations(data);
    } catch { setAllocations([]); }
    setLoadingAlloc(false);
  };

  const endMut = useMutation({
    mutationFn: (id: number) => endAllocation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['team'] });
      setConfirmEnd(null);
      handleProjectChange(selectedProject);
    },
  });

  return (
    <div className="glass-card p-6 max-w-2xl">
      <h3 className="text-lg font-semibold text-text-primary mb-4">End Allocation</h3>
      <div className="mb-4">
        <label className="block text-sm text-text-secondary mb-1">Select Project</label>
        <select value={selectedProject} onChange={(e) => handleProjectChange(e.target.value)} className="input">
          <option value="">Choose a project...</option>
          {(projects as ProjectResponse[])?.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
      </div>

      {loadingAlloc && <LoadingSpinner message="Loading allocations..." />}

      {allocations.length > 0 && (
        <table className="w-full">
          <thead><tr className="border-b border-border">
            <th className="table-header">Employee</th><th className="table-header">%</th><th className="table-header">From</th><th className="table-header">To</th><th className="table-header"></th>
          </tr></thead>
          <tbody>
            {allocations.map((a) => (
              <tr key={a.id} className="table-row">
                <td className="table-cell font-medium text-text-primary">{a.userName}</td>
                <td className="table-cell">{a.utilisationPercent}%</td>
                <td className="table-cell">{formatDate(a.fromDate)}</td>
                <td className="table-cell">{formatDate(a.toDate)}</td>
                <td className="table-cell"><button onClick={() => setConfirmEnd(a)} className="btn-danger text-xs">End</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <ConfirmDialog
        open={!!confirmEnd}
        title="End Allocation"
        message={`End ${confirmEnd?.userName}'s allocation on this project? Their end date will be set to today.`}
        confirmLabel="Yes, End Now"
        onConfirm={() => confirmEnd && endMut.mutate(confirmEnd.id)}
        onCancel={() => setConfirmEnd(null)}
        loading={endMut.isPending}
      />
    </div>
  );
}
