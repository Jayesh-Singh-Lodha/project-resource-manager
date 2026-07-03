import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getTeam, getEmployeeDetail } from '../../api/manager.api';
import PageHeader from '../../components/ui/PageHeader';
import StatusBadge from '../../components/ui/StatusBadge';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { formatDate } from '../../lib/utils';
import { X } from 'lucide-react';
import type { EmployeeDetailResponse } from '../../types';

export default function ResourceDashboardPage() {
  const { data: team, isLoading } = useQuery({ queryKey: ['team'], queryFn: getTeam });
  const [detail, setDetail] = useState<EmployeeDetailResponse | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);

  const bench = team?.filter((t) => t.status?.toLowerCase() === 'bench') || [];
  const active = team?.filter((t) => t.status?.toLowerCase() !== 'bench') || [];

  const handleDrillDown = async (id: number) => {
    setLoadingDetail(true);
    try {
      const data = await getEmployeeDetail(id);
      setDetail(data);
    } catch { /* ignore */ }
    setLoadingDetail(false);
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Resource Dashboard" subtitle={`Bench: ${bench.length} | Active: ${active.length}`} />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          {/* Bench */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-success mb-3">ON BENCH ({bench.length} available)</h3>
            {bench.length === 0 ? (
              <p className="text-sm text-text-muted">No employees on bench.</p>
            ) : (
              <table className="w-full">
                <thead>
                  <tr className="border-b border-border">
                    <th className="table-header">ID</th>
                    <th className="table-header">Name</th>
                    <th className="table-header">Department</th>
                    <th className="table-header">Skills</th>
                  </tr>
                </thead>
                <tbody>
                  {bench.map((e) => (
                    <tr key={e.id} className="table-row cursor-pointer" onClick={() => handleDrillDown(e.id)}>
                      <td className="table-cell font-mono text-text-muted">{e.id}</td>
                      <td className="table-cell font-medium text-text-primary">{e.fullName}</td>
                      <td className="table-cell">{e.department || '—'}</td>
                      <td className="table-cell">
                        <div className="flex flex-wrap gap-1">
                          {e.skills?.slice(0, 3).map((s) => <span key={s} className="badge-accent text-[10px]">{s}</span>)}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Active */}
          <div className="glass-card p-5">
            <h3 className="text-sm font-semibold text-accent mb-3">ACTIVE EMPLOYEES</h3>
            <table className="w-full">
              <thead>
                <tr className="border-b border-border">
                  <th className="table-header">ID</th>
                  <th className="table-header">Name</th>
                  <th className="table-header">Alloc %</th>
                  <th className="table-header">Availability</th>
                </tr>
              </thead>
              <tbody>
                {active.map((e) => (
                  <tr key={e.id} className="table-row cursor-pointer" onClick={() => handleDrillDown(e.id)}>
                    <td className="table-cell font-mono text-text-muted">{e.id}</td>
                    <td className="table-cell font-medium text-text-primary">{e.fullName}</td>
                    <td className="table-cell">
                      <div className="flex items-center gap-2">
                        <div className="w-16 h-1.5 bg-border rounded-full overflow-hidden">
                          <div className="h-full rounded-full bg-gradient-to-r from-accent to-violet" style={{ width: `${Math.min(e.currentUtilisationPercent, 100)}%` }} />
                        </div>
                        <span className="text-xs">{e.currentUtilisationPercent}%</span>
                      </div>
                    </td>
                    <td className="table-cell">
                      {e.currentUtilisationPercent >= 100 ? (
                        <span className="text-danger text-xs">FULL</span>
                      ) : (
                        <span className="text-success text-xs">{100 - e.currentUtilisationPercent}% free</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Detail Panel */}
        <div>
          {detail && (
            <div className="glass-card p-5 sticky top-6 animate-slide-in-right">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-text-primary">{detail.employee.fullName}</h3>
                <button onClick={() => { setDetail(null); }} className="btn-ghost p-1"><X size={16} /></button>
              </div>
              <div className="space-y-3 text-sm">
                <div><span className="text-text-muted">Department:</span> <span className="text-text-primary ml-2">{detail.employee.department || '—'}</span></div>
                <div><span className="text-text-muted">Status:</span> <StatusBadge status={detail.employee.status} className="ml-2" /></div>
                <div><span className="text-text-muted">Skills:</span>
                  <div className="flex flex-wrap gap-1 mt-1">
                    {detail.employee.skills?.map((s) => <span key={s} className="badge-accent text-[10px]">{s}</span>)}
                  </div>
                </div>

                {detail.allocations.length > 0 && (
                  <div>
                    <p className="text-text-muted mb-2">Active Allocations:</p>
                    {detail.allocations.map((a) => (
                      <div key={a.id} className="p-2 bg-background rounded-lg border border-border mb-1 text-xs">
                        <p className="font-medium text-text-primary">{a.projectName}</p>
                        <p className="text-text-muted">{a.utilisationPercent}% · {formatDate(a.fromDate)} → {formatDate(a.toDate)}</p>
                      </div>
                    ))}
                  </div>
                )}

                {detail.recentActivityTags.length > 0 && (
                  <div>
                    <p className="text-text-muted mb-1">Recent Activity Tags:</p>
                    <div className="flex flex-wrap gap-1">
                      {detail.recentActivityTags.map((tag) => <span key={tag} className="badge-neutral text-[10px]">{tag}</span>)}
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}
          {loadingDetail && <LoadingSpinner message="Loading details..." />}
        </div>
      </div>
    </div>
  );
}
