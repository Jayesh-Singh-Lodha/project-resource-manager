import { getHealthEmoji, getHealthColor, cn } from '../../lib/utils';

interface HealthBadgeProps {
  status: string;
  className?: string;
}

export default function HealthBadge({ status, className }: HealthBadgeProps) {
  const emoji = getHealthEmoji(status);
  const colorClass = getHealthColor(status);
  const label = status?.replace(/_/g, ' ').toUpperCase();

  return (
    <span className={cn('inline-flex items-center gap-1.5 text-sm font-medium', colorClass, className)}>
      {emoji} {label}
    </span>
  );
}
