import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getTeamTimesheets, updateTimesheetStatus, restoreTimesheetAccess } from '../../api/timesheets.api';
import { getTeam } from '../../api/manager.api';
import PageHeader from '../../components/ui/PageHeader';
import StatusBadge from '../../components/ui/StatusBadge';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import EmptyState from '../../components/ui/EmptyState';
import ConfirmDialog from '../../components/ui/ConfirmDialog';
import { Search, CheckCircle2, XCircle, Unlock, AlertTriangle } from 'lucide-react';
import { format, startOfWeek } from 'date-fns';
import type { TimesheetResponse } from '../../types';

export default function TimesheetsPage() {
  const lastMonday = startOfWeek(new Date(), { weekStartsOn: 1 });
  const [weekDate, setWeekDate] = useState(format(lastMonday, 'yyyy-MM-dd'));
  const [timesheets, setTimesheets] = useState<TimesheetResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [actionLoading, setActionLoading] = useState<number | null>(null);
  const [actionSuccess, setActionSuccess] = useState('');
  const [actionError, setActionError] = useState('');

  // Frozen employees
  const { data: team, refetch: refetchTeam } = useQuery({ queryKey: ['team'], queryFn: getTeam });
  const frozenEmployees = team?.filter((t) => t.isTimesheetFrozen) || [];
  const [restoreConfirm, setRestoreConfirm] = useState<{ id: number; name: string } | null>(null);
  const [restoreLoading, setRestoreLoading] = useState(false);

  const handleLoad = async () => {
    setLoading(true);
    setActionError('');
    try {
      const data = await getTeamTimesheets(weekDate);
      setTimesheets(data);
      setLoaded(true);
    } catch { setTimesheets([]); setLoaded(true); }
    setLoading(false);
  };

  const handleStatusUpdate = async (timesheetId: number, status: string) => {
    setActionLoading(timesheetId);
    setActionError('');
    try {
      await updateTimesheetStatus(timesheetId, status);
      setActionSuccess(`Timesheet ${status.toLowerCase()}.`);
      setTimeout(() => setActionSuccess(''), 3000);
      // Refresh
      const data = await getTeamTimesheets(weekDate);
      setTimesheets(data);
    } catch (err: any) {
      setActionError(err?.response?.data?.message || `Failed to ${status.toLowerCase()}.`);
    }
    setActionLoading(null);
  };

  const handleRestore = async () => {
    if (!restoreConfirm) return;
    setRestoreLoading(true);
    try {
      await restoreTimesheetAccess(restoreConfirm.id);
      setActionSuccess(`Timesheet access restored for ${restoreConfirm.name}.`);
      setTimeout(() => setActionSuccess(''), 3000);
      setRestoreConfirm(null);
      refetchTeam();
    } catch (err: any) {
      setActionError(err?.response?.data?.message || 'Failed to restore access.');
      setRestoreConfirm(null);
    }
    setRestoreLoading(false);
  };

  return (
    <div>
      <PageHeader title="Team Timesheets" subtitle="View, approve, and reject submitted timesheets" />

      {actionSuccess && (
        <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success animate-fade-in">{actionSuccess}</div>
      )}
      {actionError && (
        <div className="p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20 text-sm text-danger animate-fade-in">{actionError}</div>
      )}

      <div className="flex gap-3 mb-6">
        <input type="date" value={weekDate} onChange={(e) => setWeekDate(e.target.value)} className="input w-auto" />
        <button onClick={handleLoad} className="btn-primary text-sm"><Search size={16} /> Load</button>
      </div>

      {loading && <LoadingSpinner />}

      {loaded && timesheets.length === 0 && <EmptyState title="No timesheets found" message="Try a different week." />}

      {timesheets.length > 0 && (
        <div className="glass-card overflow-hidden mb-8">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="table-header">Employee</th>
                <th className="table-header">Project</th>
                <th className="table-header">Hours</th>
                <th className="table-header">Status</th>
                <th className="table-header">Actions</th>
              </tr>
            </thead>
            <tbody>
              {timesheets.flatMap((ts) =>
                ts.entries.length > 0
                  ? ts.entries.map((entry, idx) => (
                      <tr key={`${ts.id}-${entry.id}`} className="table-row">
                        <td className="table-cell font-medium text-text-primary">{idx === 0 ? ts.userName : ''}</td>
                        <td className="table-cell">{entry.projectName}</td>
                        <td className="table-cell">{entry.hoursWorked}</td>
                        <td className="table-cell">{idx === 0 && <StatusBadge status={ts.status} />}</td>
                        <td className="table-cell">
                          {idx === 0 && ts.status === 'Submitted' && (
                            <div className="flex gap-1">
                              <button
                                onClick={() => handleStatusUpdate(ts.id, 'Approved')}
                                disabled={actionLoading === ts.id}
                                className="btn-ghost text-xs px-2 py-1 text-success hover:bg-success/10 inline-flex items-center gap-1"
                              >
                                <CheckCircle2 size={12} /> Approve
                              </button>
                              <button
                                onClick={() => handleStatusUpdate(ts.id, 'Rejected')}
                                disabled={actionLoading === ts.id}
                                className="btn-ghost text-xs px-2 py-1 text-danger hover:bg-danger/10 inline-flex items-center gap-1"
                              >
                                <XCircle size={12} /> Reject
                              </button>
                            </div>
                          )}
                        </td>
                      </tr>
                    ))
                  : [
                      <tr key={ts.id} className="table-row">
                        <td className="table-cell font-medium text-text-primary">{ts.userName}</td>
                        <td className="table-cell text-text-muted">—</td>
                        <td className="table-cell">0</td>
                        <td className="table-cell"><StatusBadge status={ts.status} /></td>
                        <td className="table-cell">
                          {ts.status === 'Submitted' && (
                            <div className="flex gap-1">
                              <button
                                onClick={() => handleStatusUpdate(ts.id, 'Approved')}
                                disabled={actionLoading === ts.id}
                                className="btn-ghost text-xs px-2 py-1 text-success hover:bg-success/10 inline-flex items-center gap-1"
                              >
                                <CheckCircle2 size={12} /> Approve
                              </button>
                              <button
                                onClick={() => handleStatusUpdate(ts.id, 'Rejected')}
                                disabled={actionLoading === ts.id}
                                className="btn-ghost text-xs px-2 py-1 text-danger hover:bg-danger/10 inline-flex items-center gap-1"
                              >
                                <XCircle size={12} /> Reject
                              </button>
                            </div>
                          )}
                        </td>
                      </tr>,
                    ]
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Frozen Employees Panel */}
      {frozenEmployees.length > 0 && (
        <div className="glass-card p-5">
          <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wide mb-3 flex items-center gap-2">
            <AlertTriangle size={16} className="text-warning" /> Frozen Employees ({frozenEmployees.length})
          </h3>
          <p className="text-xs text-text-muted mb-4">These employees are frozen due to missed timesheets. Restore access to allow them to submit again.</p>
          <div className="space-y-2">
            {frozenEmployees.map((emp) => (
              <div key={emp.id} className="flex items-center justify-between p-3 bg-background rounded-lg border border-danger/20">
                <div>
                  <p className="text-sm font-medium text-text-primary">{emp.fullName}</p>
                  <p className="text-xs text-text-muted">{emp.department || 'No department'} · ID: {emp.id}</p>
                </div>
                <button
                  onClick={() => setRestoreConfirm({ id: emp.id, name: emp.fullName })}
                  className="btn-secondary text-xs inline-flex items-center gap-1"
                >
                  <Unlock size={12} /> Restore Access
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      <ConfirmDialog
        open={!!restoreConfirm}
        title="Restore Timesheet Access"
        message={`Restore timesheet access for ${restoreConfirm?.name}? They will be able to submit timesheets again.`}
        confirmLabel="Yes, Restore"
        onConfirm={handleRestore}
        onCancel={() => setRestoreConfirm(null)}
        loading={restoreLoading}
      />
    </div>
  );
}
