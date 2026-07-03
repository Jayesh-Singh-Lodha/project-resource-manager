import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllAllocations } from '../../../api/allocations.api';
import PageHeader from '../../../components/ui/PageHeader';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { Search } from 'lucide-react';
import { formatDate } from '../../../lib/utils';

export default function AllAllocationsPage() {
  const { data: allocations, isLoading } = useQuery({ queryKey: ['allocations'], queryFn: getAllAllocations });
  const [search, setSearch] = useState('');

  const filtered = allocations?.filter((a) =>
    a.userName.toLowerCase().includes(search.toLowerCase()) ||
    a.projectName.toLowerCase().includes(search.toLowerCase())
  ) || [];

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="All Allocations" subtitle={`Total Active Allocations: ${allocations?.length || 0}`} />

      <div className="relative mb-6">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
        <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Filter by employee or project..." className="input pl-10 max-w-md" />
      </div>

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              <th className="table-header">Employee</th>
              <th className="table-header">Project</th>
              <th className="table-header">%</th>
              <th className="table-header">From</th>
              <th className="table-header">To</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((a) => (
              <tr key={a.id} className="table-row">
                <td className="table-cell font-medium text-text-primary">{a.userName}</td>
                <td className="table-cell">{a.projectName}</td>
                <td className="table-cell">
                  <span className="badge-accent">{a.utilisationPercent}%</span>
                </td>
                <td className="table-cell">{formatDate(a.fromDate)}</td>
                <td className="table-cell">{formatDate(a.toDate)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
