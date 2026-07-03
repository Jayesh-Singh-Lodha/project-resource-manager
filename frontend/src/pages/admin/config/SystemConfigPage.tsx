import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllConfigs, updateConfig } from '../../../api/config.api';
import PageHeader from '../../../components/ui/PageHeader';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { Save, AlertCircle, Eye, EyeOff } from 'lucide-react';

export default function SystemConfigPage() {
  const queryClient = useQueryClient();
  const { data: configs, isLoading } = useQuery({ queryKey: ['configs'], queryFn: getAllConfigs });
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showApiKey, setShowApiKey] = useState(false);

  const mutation = useMutation({
    mutationFn: ({ key, value }: { key: string; value: string }) => updateConfig(key, { value }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['configs'] });
      setEditingKey(null);
      setSuccess('Configuration updated.');
      setTimeout(() => setSuccess(''), 3000);
    },
    onError: (err: any) => setError(err?.response?.data?.message || 'Failed to update.'),
  });

  const getDisplayValue = (key: string, value: string) => {
    if (key.toLowerCase().includes('apikey') || key.toLowerCase().includes('api_key')) {
      return showApiKey ? value : '•'.repeat(Math.min(value.length, 30));
    }
    return value;
  };

  const getLabel = (key: string) => {
    const labels: Record<string, string> = {
      LlmProvider: 'LLM Provider',
      LlmApiKey: 'LLM API Key',
      SchedulerIntervalHours: 'Scheduler Interval (hours)',
      MaxWeeklyHours: 'Max Weekly Hours',
    };
    return labels[key] || key.replace(/([A-Z])/g, ' $1').trim();
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="System Configuration" subtitle="Manage system-wide settings" />

      {error && (
        <div className="flex items-center gap-2 p-3 mb-4 rounded-lg bg-danger/10 border border-danger/20">
          <AlertCircle size={16} className="text-danger" />
          <p className="text-sm text-danger">{error}</p>
        </div>
      )}
      {success && (
        <div className="p-3 mb-4 rounded-lg bg-success/10 border border-success/20 text-sm text-success animate-fade-in">{success}</div>
      )}

      <div className="glass-card divide-y divide-border">
        {configs?.map((config) => (
          <div key={config.key} className="p-5 flex items-center justify-between gap-4">
            <div className="flex-1">
              <p className="text-sm font-medium text-text-primary">{getLabel(config.key)}</p>
              {editingKey === config.key ? (
                <div className="flex items-center gap-2 mt-2">
                  <input
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    className="input max-w-sm"
                    autoFocus
                  />
                  <button
                    onClick={() => mutation.mutate({ key: config.key, value: editValue })}
                    className="btn-primary text-xs"
                    disabled={mutation.isPending}
                  >
                    <Save size={14} /> Save
                  </button>
                  <button onClick={() => setEditingKey(null)} className="btn-ghost text-xs">Cancel</button>
                </div>
              ) : (
                <div className="flex items-center gap-2 mt-1">
                  <p className="text-sm text-text-muted font-mono">{getDisplayValue(config.key, config.value)}</p>
                  {(config.key.toLowerCase().includes('apikey') || config.key.toLowerCase().includes('api_key')) && (
                    <button onClick={() => setShowApiKey(!showApiKey)} className="btn-ghost p-1">
                      {showApiKey ? <EyeOff size={14} /> : <Eye size={14} />}
                    </button>
                  )}
                </div>
              )}
            </div>
            {editingKey !== config.key && (
              <button
                onClick={() => { setEditingKey(config.key); setEditValue(config.value); }}
                className="btn-secondary text-xs"
              >
                Edit
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
