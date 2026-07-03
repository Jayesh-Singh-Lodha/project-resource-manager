import { cn } from '../../lib/utils';

interface PageHeaderProps {
  title: string;
  subtitle?: string;
  children?: React.ReactNode;
}

export default function PageHeader({ title, subtitle, children }: PageHeaderProps) {
  return (
    <div className="flex items-start justify-between mb-6">
      <div>
        <h1 className="text-2xl font-bold text-text-primary">{title}</h1>
        {subtitle && (
          <p className="mt-1 text-sm text-text-muted">{subtitle}</p>
        )}
      </div>
      {children && <div className="flex items-center gap-2">{children}</div>}
    </div>
  );
}

interface StatCardProps {
  label: string;
  value: string | number;
  icon: React.ReactNode;
  trend?: string;
  className?: string;
}

export function StatCard({ label, value, icon, trend, className }: StatCardProps) {
  return (
    <div className={cn('glass-card p-5 animate-slide-up', className)}>
      <div className="flex items-center justify-between mb-3">
        <span className="text-text-muted text-sm">{label}</span>
        <div className="p-2 rounded-lg bg-accent/10 text-accent">{icon}</div>
      </div>
      <div className="text-2xl font-bold text-text-primary">{value}</div>
      {trend && <p className="text-xs text-text-muted mt-1">{trend}</p>}
    </div>
  );
}
