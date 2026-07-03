import { Loader2 } from 'lucide-react';

interface LoadingSpinnerProps {
  message?: string;
}

export default function LoadingSpinner({ message = 'Loading...' }: LoadingSpinnerProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 animate-fade-in">
      <Loader2 size={32} className="text-accent animate-spin mb-3" />
      <p className="text-sm text-text-muted">{message}</p>
    </div>
  );
}
