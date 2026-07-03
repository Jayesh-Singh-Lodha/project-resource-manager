import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getManagedProjects, getProjectRiskSummary } from '../../api/manager.api';
import { getMilestonesByProjectId } from '../../api/projects.api';
import { getProjectAllocations } from '../../api/allocations.api';
import PageHeader from '../../components/ui/PageHeader';
import HealthBadge from '../../components/ui/HealthBadge';
import StatusBadge from '../../components/ui/StatusBadge';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { formatDate } from '../../lib/utils';
import { Bot, Loader2 } from 'lucide-react';
import type { ProjectResponse, MilestoneResponse, AllocationResponse } from '../../types';

export default function MyProjectsPage() {
  const { data: projects, isLoading } = useQuery({ queryKey: ['managed-projects'], queryFn: getManagedProjects });
  const [selectedProject, setSelectedProject] = useState<ProjectResponse | null>(null);
  const [milestones, setMilestones] = useState<MilestoneResponse[]>([]);
  const [allocations, setAllocations] = useState<AllocationResponse[]>([]);
  const [riskSummary, setRiskSummary] = useState('');
  const [loadingRisk, setLoadingRisk] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);

  const handleSelect = async (p: ProjectResponse) => {
    setSelectedProject(p);
    setRiskSummary('');
    setLoadingDetail(true);
    try {
      const [ms, allocs] = await Promise.all([
        getMilestonesByProjectId(p.id),
        getProjectAllocations(p.id),
      ]);
      setMilestones(ms);
      setAllocations(allocs);
    } catch { /* ignore */ }
    setLoadingDetail(false);
  };

  const handleRiskSummary = async () => {
    if (!selectedProject) return;
    setLoadingRisk(true);
    try {
      const res = await getProjectRiskSummary(selectedProject.id);
      setRiskSummary(res.summary);
    } catch (err: any) {
      setRiskSummary('Error: ' + (err?.response?.data?.message || 'Failed to generate.'));
    }
    setLoadingRisk(false);
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="My Projects" />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Project List */}
        <div className="space-y-2">
          {(projects as ProjectResponse[])?.map((p) => (
            <button
              key={p.id}
              onClick={() => handleSelect(p)}
              className={`w-full glass-card p-4 text-left transition-all ${selectedProject?.id === p.id ? 'border-accent/50 shadow-glow-sm' : 'hover:border-border-hover'}`}
            >
              <div className="flex items-center justify-between mb-1">
                <h3 className="font-medium text-text-primary text-sm">{p.name}</h3>
                <HealthBadge status={p.healthStatus} />
              </div>
              <p className="text-xs text-text-muted">Ends {formatDate(p.endDate)}</p>
            </button>
          ))}
        </div>

        {/* Detail Panel */}
        <div className="lg:col-span-2">
          {loadingDetail && <LoadingSpinner />}
          {selectedProject && !loadingDetail && (
            <div className="glass-card p-6 animate-fade-in">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h2 className="text-xl font-bold text-text-primary">{selectedProject.name}</h2>
                  <HealthBadge status={selectedProject.healthStatus} />
                </div>
                <button onClick={handleRiskSummary} className="btn-primary text-sm" disabled={loadingRisk}>
                  {loadingRisk ? <Loader2 size={16} className="animate-spin" /> : <Bot size={16} />}
                  AI Risk Summary
                </button>
              </div>

              {riskSummary && (
                <div className="p-4 mb-6 bg-background rounded-lg border border-accent/20 animate-slide-up">
                  <p className="text-xs text-accent mb-2 font-medium">AI Risk Summary</p>
                  <p className="text-sm text-text-primary whitespace-pre-wrap italic">"{riskSummary}"</p>
                  <p className="text-[10px] text-text-muted mt-2">Generated from milestone and timesheet data.</p>
                </div>
              )}

              {/* Milestones */}
              <h3 className="text-sm font-semibold text-text-secondary mb-2">Milestones</h3>
              <table className="w-full mb-6">
                <thead><tr className="border-b border-border">
                  <th className="table-header">#</th><th className="table-header">Title</th><th className="table-header">Due Date</th><th className="table-header">SP</th><th className="table-header">Status</th>
                </tr></thead>
                <tbody>
                  {milestones.map((m, i) => (
                    <tr key={m.id} className="table-row">
                      <td className="table-cell text-text-muted">{i + 1}</td>
                      <td className="table-cell font-medium text-text-primary">{m.title}</td>
                      <td className="table-cell">{formatDate(m.dueDate)}</td>
                      <td className="table-cell">{m.storyPoints}</td>
                      <td className="table-cell"><StatusBadge status={m.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {/* Allocations */}
              <h3 className="text-sm font-semibold text-text-secondary mb-2">Allocated Resources</h3>
              <table className="w-full">
                <thead><tr className="border-b border-border">
                  <th className="table-header">Name</th><th className="table-header">%</th><th className="table-header">From</th><th className="table-header">To</th>
                </tr></thead>
                <tbody>
                  {allocations.map((a) => (
                    <tr key={a.id} className="table-row">
                      <td className="table-cell font-medium text-text-primary">{a.userName}</td>
                      <td className="table-cell"><span className="badge-accent">{a.utilisationPercent}%</span></td>
                      <td className="table-cell">{formatDate(a.fromDate)}</td>
                      <td className="table-cell">{formatDate(a.toDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
