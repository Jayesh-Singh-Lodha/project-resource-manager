import { useQuery } from '@tanstack/react-query';
import { getMyAllocations } from '../../api/allocations.api';
import PageHeader from '../../components/ui/PageHeader';

import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { formatDate } from '../../lib/utils';

export default function MyAllocationsPage() {
  const { data: allocations, isLoading } = useQuery({ queryKey: ['my-allocations'], queryFn: getMyAllocations });

  const totalUtil = allocations?.reduce((sum, a) => sum + a.utilisationPercent, 0) || 0;
  const now = new Date();
  const active = allocations?.filter((a) => new Date(a.toDate) >= now) || [];
  const past = allocations?.filter((a) => new Date(a.toDate) < now) || [];

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="My Allocations" subtitle={`Total Utilisation: ${totalUtil}%`} />

      {/* Utilisation Bar */}
      <div className="glass-card p-5 mb-6">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm text-text-muted">Current Utilisation</span>
          <span className="text-lg font-bold text-text-primary">{totalUtil}%</span>
        </div>
        <div className="w-full h-3 bg-border rounded-full overflow-hidden">
          <div
            className="h-full rounded-full bg-gradient-to-r from-accent to-violet transition-all duration-500"
            style={{ width: `${Math.min(totalUtil, 100)}%` }}
          />
        </div>
      </div>

      {/* Active */}
      {active.length > 0 && (
        <div className="glass-card overflow-hidden mb-6">
          <div className="px-4 py-3 border-b border-border">
            <h3 className="text-sm font-semibold text-success">ACTIVE ({active.length})</h3>
          </div>
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="table-header">Project</th>
                <th className="table-header">%</th>
                <th className="table-header">From</th>
                <th className="table-header">To</th>
              </tr>
            </thead>
            <tbody>
              {active.map((a) => (
                <tr key={a.id} className="table-row">
                  <td className="table-cell font-medium text-text-primary">{a.projectName}</td>
                  <td className="table-cell"><span className="badge-accent">{a.utilisationPercent}%</span></td>
                  <td className="table-cell">{formatDate(a.fromDate)}</td>
                  <td className="table-cell">{formatDate(a.toDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Past */}
      {past.length > 0 && (
        <div className="glass-card overflow-hidden">
          <div className="px-4 py-3 border-b border-border">
            <h3 className="text-sm font-semibold text-text-muted">PAST ({past.length})</h3>
          </div>
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="table-header">Project</th>
                <th className="table-header">%</th>
                <th className="table-header">From</th>
                <th className="table-header">To</th>
              </tr>
            </thead>
            <tbody>
              {past.map((a) => (
                <tr key={a.id} className="table-row opacity-60">
                  <td className="table-cell">{a.projectName}</td>
                  <td className="table-cell"><span className="badge-neutral">{a.utilisationPercent}%</span></td>
                  <td className="table-cell">{formatDate(a.fromDate)}</td>
                  <td className="table-cell">{formatDate(a.toDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
