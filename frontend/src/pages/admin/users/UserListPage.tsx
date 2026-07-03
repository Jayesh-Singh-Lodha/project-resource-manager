import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllUsers } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import StatusBadge from '../../../components/ui/StatusBadge';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import { Plus, Search, Settings } from 'lucide-react';

export default function UserListPage() {
  const { data: users, isLoading } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const [search, setSearch] = useState('');

  const filtered = users?.filter((u) =>
    u.username.toLowerCase().includes(search.toLowerCase()) ||
    u.fullName.toLowerCase().includes(search.toLowerCase())
  ) || [];

  const activeCount = users?.filter((u) => u.isActive).length || 0;
  const inactiveCount = (users?.length || 0) - activeCount;

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="All Users" subtitle={`Total: ${users?.length || 0} | Active: ${activeCount} | Inactive: ${inactiveCount}`}>
        <Link to="/admin/users/create" className="btn-primary text-sm"><Plus size={16} /> Create User</Link>
      </PageHeader>

      <div className="relative mb-6">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
        <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search users..." className="input pl-10 max-w-md" />
      </div>

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              <th className="table-header">ID</th>
              <th className="table-header">Username</th>
              <th className="table-header">Full Name</th>
              <th className="table-header">Email</th>
              <th className="table-header">Role</th>
              <th className="table-header">Status</th>
              <th className="table-header">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((u) => (
              <tr key={u.id} className="table-row">
                <td className="table-cell font-mono text-text-muted">{u.id}</td>
                <td className="table-cell font-medium text-text-primary">{u.username}</td>
                <td className="table-cell">
                  <div className="flex items-center gap-2">
                    <span>{u.fullName}</span>
                    {u.isTimesheetFrozen && (
                      <span className="text-[9px] px-1.5 py-0.5 rounded-full bg-danger/10 text-danger border border-danger/20 font-semibold">FROZEN</span>
                    )}
                  </div>
                </td>
                <td className="table-cell text-text-muted">{u.email}</td>
                <td className="table-cell"><StatusBadge status={u.role} /></td>
                <td className="table-cell">
                  <StatusBadge status={u.isActive ? 'Active' : 'Inactive'} />
                </td>
                <td className="table-cell">
                  <Link to={`/admin/users/${u.id}/edit`} className="btn-ghost text-xs px-2 py-1 inline-flex items-center gap-1">
                    <Settings size={12} /> Manage
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
