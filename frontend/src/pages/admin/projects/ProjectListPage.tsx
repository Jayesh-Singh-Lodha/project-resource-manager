import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllProjects } from '../../../api/projects.api';
import { Link } from 'react-router-dom';
import PageHeader from '../../../components/ui/PageHeader';
import StatusBadge from '../../../components/ui/StatusBadge';
import HealthBadge from '../../../components/ui/HealthBadge';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { Plus, Search } from 'lucide-react';
import { formatDate } from '../../../lib/utils';

export default function ProjectListPage() {
  const { data: projects, isLoading } = useQuery({ queryKey: ['projects'], queryFn: getAllProjects });
  const [search, setSearch] = useState('');

  const filtered = projects?.filter((p) =>
    p.name.toLowerCase().includes(search.toLowerCase())
  ) || [];

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="All Projects" subtitle={`${projects?.length || 0} projects total`}>
        <Link to="/admin/projects/create" className="btn-primary text-sm"><Plus size={16} /> Create Project</Link>
      </PageHeader>

      <div className="relative mb-6">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
        <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search projects..." className="input pl-10 max-w-md" />
      </div>

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              <th className="table-header">ID</th>
              <th className="table-header">Name</th>
              <th className="table-header">Manager</th>
              <th className="table-header">End Date</th>
              <th className="table-header">Status</th>
              <th className="table-header">Health</th>
              <th className="table-header">SP Done/Total</th>
              <th className="table-header">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((p) => (
              <tr key={p.id} className="table-row">
                <td className="table-cell font-mono text-text-muted">{p.id}</td>
                <td className="table-cell font-medium text-text-primary">{p.name}</td>
                <td className="table-cell">{p.managerName || '—'}</td>
                <td className="table-cell">{formatDate(p.endDate)}</td>
                <td className="table-cell"><StatusBadge status={p.status} /></td>
                <td className="table-cell"><HealthBadge status={p.healthStatus} /></td>
                <td className="table-cell">
                  <span className="text-accent font-medium">{p.storyPointsCompleted}</span>
                  <span className="text-text-muted"> / {p.totalStoryPoints}</span>
                </td>
                <td className="table-cell">
                  <div className="flex gap-1">
                    <Link to={`/admin/projects/${p.id}/edit`} className="btn-ghost text-xs px-2 py-1">Edit</Link>
                    <Link to={`/admin/projects/${p.id}/milestones`} className="btn-ghost text-xs px-2 py-1">Milestones</Link>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
