import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getMyTimesheets } from '../../api/timesheets.api';
import PageHeader from '../../components/ui/PageHeader';
import StatusBadge from '../../components/ui/StatusBadge';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { formatDate } from '../../lib/utils';
import { ChevronDown, ChevronRight } from 'lucide-react';


export default function MyTimesheetsPage() {
  const { data: timesheets, isLoading } = useQuery({ queryKey: ['my-timesheets'], queryFn: getMyTimesheets });
  const [expandedId, setExpandedId] = useState<number | null>(null);

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="My Timesheets" subtitle="Your weekly timesheet history" />

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              <th className="table-header w-8"></th>
              <th className="table-header">Week Start</th>
              <th className="table-header">Total Hours</th>
              <th className="table-header">Status</th>
            </tr>
          </thead>
          <tbody>
            {timesheets?.map((ts) => {
              const totalHours = ts.entries.reduce((sum, e) => sum + e.hoursWorked, 0);
              const isExpanded = expandedId === ts.id;

              return (
                <>
                  <tr
                    key={ts.id}
                    className="table-row cursor-pointer"
                    onClick={() => setExpandedId(isExpanded ? null : ts.id)}
                  >
                    <td className="table-cell">
                      {isExpanded ? <ChevronDown size={14} className="text-text-muted" /> : <ChevronRight size={14} className="text-text-muted" />}
                    </td>
                    <td className="table-cell font-medium text-text-primary">{formatDate(ts.weekStartDate)}</td>
                    <td className="table-cell">{totalHours} hrs</td>
                    <td className="table-cell"><StatusBadge status={ts.status} /></td>
                  </tr>
                  {isExpanded && (
                    <tr key={`${ts.id}-detail`}>
                      <td colSpan={4} className="px-4 pb-4">
                        <div className="bg-background rounded-lg border border-border p-4 animate-slide-up">
                          <table className="w-full">
                            <thead>
                              <tr className="border-b border-border/50">
                                <th className="table-header">Project</th>
                                <th className="table-header">Hours</th>
                                <th className="table-header">Activity Tags</th>
                              </tr>
                            </thead>
                            <tbody>
                              {ts.entries.map((entry) => (
                                <tr key={entry.id} className="border-b border-border/30 last:border-0">
                                  <td className="table-cell font-medium text-text-primary">{entry.projectName}</td>
                                  <td className="table-cell">{entry.hoursWorked}</td>
                                  <td className="table-cell">
                                    <div className="flex flex-wrap gap-1">
                                      {entry.activityTags?.split(',').map((tag) => (
                                        <span key={tag.trim()} className="badge-neutral text-[10px]">{tag.trim()}</span>
                                      ))}
                                    </div>
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </td>
                    </tr>
                  )}
                </>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
