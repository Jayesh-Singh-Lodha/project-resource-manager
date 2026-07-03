import { Inbox } from 'lucide-react';

interface EmptyStateProps {
  title: string;
  message?: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
}

export default function EmptyState({ title, message, icon, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 animate-fade-in">
      <div className="p-4 rounded-full bg-surface-hover mb-4">
        {icon || <Inbox size={32} className="text-text-muted" />}
      </div>
      <h3 className="text-lg font-medium text-text-primary mb-1">{title}</h3>
      {message && <p className="text-sm text-text-muted mb-4">{message}</p>}
      {action}
    </div>
  );
}
