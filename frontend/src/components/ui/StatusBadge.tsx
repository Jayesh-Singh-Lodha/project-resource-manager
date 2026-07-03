import { cn } from '../../lib/utils';

interface StatusBadgeProps {
  status: string;
  className?: string;
}

function getStatusStyle(status: string): string {
  const s = status?.toLowerCase();
  if (['active', 'submitted', 'allocated', 'done', 'completed'].includes(s)) return 'badge-success';
  if (['attention', 'in_progress', 'inprogress', 'pending', 'on_hold', 'onhold', 'planned'].includes(s)) return 'badge-warning';
  if (['at_risk', 'atrisk', 'missed', 'inactive', 'overdue'].includes(s)) return 'badge-danger';
  if (['bench', 'not_started', 'notstarted'].includes(s)) return 'badge-neutral';
  return 'badge-neutral';
}

export default function StatusBadge({ status, className }: StatusBadgeProps) {
  return (
    <span className={cn(getStatusStyle(status), className)}>
      {status?.replace(/_/g, ' ').toUpperCase()}
    </span>
  );
}
