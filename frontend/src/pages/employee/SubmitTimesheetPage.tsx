import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getMyAllocations } from '../../api/allocations.api';
import { submitTimesheet } from '../../api/timesheets.api';
import PageHeader from '../../components/ui/PageHeader';
import LoadingSpinner from '../../components/ui/LoadingSpinner';
import { AlertCircle, Check } from 'lucide-react';
import { format, startOfWeek, subWeeks } from 'date-fns';

const ACTIVITY_TAGS = [
  'Backend API Development', 'Microservices / Architecture', 'Database Design & Queries',
  'WebSocket / Real-time Features', 'Frontend Development', 'Code Review / Mentoring',
  'Bug Fixing', 'DevOps / Deployment', 'Testing & QA', 'Documentation',
];

export default function SubmitTimesheetPage() {
  const queryClient = useQueryClient();
  const { data: allocations, isLoading } = useQuery({ queryKey: ['my-allocations'], queryFn: getMyAllocations });

  const lastMonday = format(startOfWeek(subWeeks(new Date(), 0), { weekStartsOn: 1 }), 'yyyy-MM-dd');
  const [weekDate, setWeekDate] = useState(lastMonday);
  const [entries, setEntries] = useState<Record<number, { hours: string; tags: string[] }>>({});
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const activeAllocations = allocations?.filter((a) => {
    const from = new Date(a.fromDate);
    const to = new Date(a.toDate);
    const week = new Date(weekDate);
    return week >= from && week <= to;
  }) || [];

  const updateEntry = (projectId: number, field: 'hours' | 'tags', value: string | string[]) => {
    setEntries((prev) => ({
      ...prev,
      [projectId]: {
        hours: field === 'hours' ? (value as string) : (prev[projectId]?.hours || ''),
        tags: field === 'tags' ? (value as string[]) : (prev[projectId]?.tags || []),
      },
    }));
  };

  const toggleTag = (projectId: number, tag: string) => {
    const current = entries[projectId]?.tags || [];
    const updated = current.includes(tag) ? current.filter((t) => t !== tag) : [...current, tag];
    updateEntry(projectId, 'tags', updated);
  };

  const totalHours = Object.values(entries).reduce((sum, e) => sum + (Number(e.hours) || 0), 0);

  const mutation = useMutation({
    mutationFn: () => submitTimesheet({
      userId: 0, // server overrides from JWT
      weekStartDate: weekDate,
      entries: activeAllocations.map((a) => ({
        projectId: a.projectId,
        hoursWorked: Number(entries[a.projectId]?.hours || 0),
        activityTags: entries[a.projectId]?.tags?.join(', ') || null,
      })).filter((e) => e.hoursWorked > 0),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['last-week-status'] });
      setSuccess(true);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to submit.'),
  });

  if (isLoading) return <LoadingSpinner />;

  if (success) {
    return (
      <div>
        <PageHeader title="Timesheet Submitted" />
        <div className="glass-card p-6 max-w-lg text-center animate-slide-up">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-success/10 mb-4">
            <Check size={32} className="text-success" />
          </div>
          <h3 className="text-xl font-bold text-text-primary mb-2">Timesheet Submitted Successfully!</h3>
          <p className="text-sm text-text-muted mb-4">Total: {totalHours} hours</p>
          <button onClick={() => { setSuccess(false); setEntries({}); }} className="btn-secondary">Submit Another</button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader title="Submit Timesheet" />

      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}

      <div className="glass-card p-5 mb-6 max-w-3xl">
        <label className="block text-sm text-text-secondary mb-1">Week Start Date</label>
        <input type="date" value={weekDate} onChange={(e) => setWeekDate(e.target.value)} className="input w-auto" />
      </div>

      {activeAllocations.length === 0 ? (
        <div className="glass-card p-6 text-center">
          <p className="text-text-muted">No active allocations for this week.</p>
        </div>
      ) : (
        <div className="space-y-6 max-w-3xl">
          {activeAllocations.map((alloc, index) => (
            <div key={alloc.projectId} className="glass-card p-5 animate-slide-up" style={{ animationDelay: `${index * 100}ms` }}>
              <div className="flex items-center justify-between mb-3">
                <h3 className="font-semibold text-text-primary">
                  PROJECT {index + 1} OF {activeAllocations.length} — {alloc.projectName}
                </h3>
                <span className="badge-accent">{alloc.utilisationPercent}%</span>
              </div>

              <div className="mb-4">
                <label className="block text-sm text-text-muted mb-1">Hours worked this week</label>
                <input
                  type="number"
                  min={0}
                  value={entries[alloc.projectId]?.hours || ''}
                  onChange={(e) => updateEntry(alloc.projectId, 'hours', e.target.value)}
                  className="input w-32"
                  placeholder="0"
                />
              </div>

              <div>
                <label className="block text-sm text-text-muted mb-2">Activity Tags</label>
                <div className="flex flex-wrap gap-2">
                  {ACTIVITY_TAGS.map((tag) => (
                    <button
                      key={tag}
                      type="button"
                      onClick={() => toggleTag(alloc.projectId, tag)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                        entries[alloc.projectId]?.tags?.includes(tag)
                          ? 'bg-accent text-white'
                          : 'bg-surface-hover text-text-muted hover:text-text-primary'
                      }`}
                    >
                      {tag}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          ))}

          {/* Summary */}
          <div className="glass-card p-5">
            <h3 className="font-semibold text-text-primary mb-3">SUMMARY</h3>
            {activeAllocations.map((alloc) => (
              <div key={alloc.projectId} className="flex items-center justify-between py-1 text-sm">
                <span className="text-text-secondary">{alloc.projectName}</span>
                <div className="flex items-center gap-4">
                  <span className="text-text-primary font-medium">{entries[alloc.projectId]?.hours || 0} hrs</span>
                  <span className="text-text-muted text-xs">[{entries[alloc.projectId]?.tags?.join(', ') || '—'}]</span>
                </div>
              </div>
            ))}
            <div className="border-t border-border mt-2 pt-2 flex items-center justify-between">
              <span className="font-semibold text-text-primary">Total</span>
              <span className="font-bold text-accent">{totalHours} hrs</span>
            </div>
          </div>

          <button
            onClick={() => { setError(''); mutation.mutate(); }}
            className="btn-primary w-full py-3"
            disabled={mutation.isPending || totalHours === 0}
          >
            {mutation.isPending ? 'Submitting...' : 'Submit Timesheet'}
          </button>
        </div>
      )}
    </div>
  );
}
