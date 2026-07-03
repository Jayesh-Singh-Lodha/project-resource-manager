import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllUsers } from '../../../api/users.api';
import PageHeader from '../../../components/ui/PageHeader';
import StatusBadge from '../../../components/ui/StatusBadge';
import LoadingSpinner from '../../../components/ui/LoadingSpinner';
import EmptyState from '../../../components/ui/EmptyState';
import { Search } from 'lucide-react';

export default function EmployeeListPage() {
  const { data: users, isLoading } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [deptFilter, setDeptFilter] = useState('all');

  const employees = users?.filter((u) => u.role === 'Employee') || [];

  const departments = [...new Set(employees.map((e) => e.department).filter(Boolean))];

  const filtered = employees.filter((e) => {
    const matchSearch = e.fullName.toLowerCase().includes(search.toLowerCase()) ||
      e.username.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === 'all' || e.status.toLowerCase() === statusFilter;
    const matchDept = deptFilter === 'all' || e.department === deptFilter;
    return matchSearch && matchStatus && matchDept;
  });

  const benchCount = employees.filter((e) => e.status.toLowerCase() === 'bench').length;
  const allocatedCount = employees.filter((e) => e.status.toLowerCase() === 'allocated').length;

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="All Employees" subtitle={`Total: ${employees.length} | Allocated: ${allocatedCount} | Bench: ${benchCount}`} />

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-6">
        <div className="relative flex-1 min-w-[200px]">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by name or username..."
            className="input pl-10"
          />
        </div>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="input w-auto">
          <option value="all">All Status</option>
          <option value="allocated">Allocated</option>
          <option value="bench">Bench</option>
        </select>
        <select value={deptFilter} onChange={(e) => setDeptFilter(e.target.value)} className="input w-auto">
          <option value="all">All Departments</option>
          {departments.map((d) => <option key={d} value={d!}>{d}</option>)}
        </select>
      </div>

      {/* Table */}
      {filtered.length === 0 ? (
        <EmptyState title="No employees found" message="Try adjusting your filters." />
      ) : (
        <div className="glass-card overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="table-header">ID</th>
                <th className="table-header">Name</th>
                <th className="table-header">Department</th>
                <th className="table-header">Status</th>
                <th className="table-header">Utilisation</th>
                <th className="table-header">Skills</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((emp) => (
                <tr key={emp.id} className="table-row">
                  <td className="table-cell font-mono text-text-muted">{emp.id}</td>
                  <td className="table-cell font-medium text-text-primary">{emp.fullName}</td>
                  <td className="table-cell">{emp.department || '—'}</td>
                  <td className="table-cell"><StatusBadge status={emp.status} /></td>
                  <td className="table-cell">
                    <div className="flex items-center gap-2">
                      <div className="w-16 h-1.5 bg-border rounded-full overflow-hidden">
                        <div
                          className="h-full rounded-full bg-gradient-to-r from-accent to-violet"
                          style={{ width: `${Math.min(emp.currentUtilisationPercent, 100)}%` }}
                        />
                      </div>
                      <span className="text-xs text-text-muted">{emp.currentUtilisationPercent}%</span>
                    </div>
                  </td>
                  <td className="table-cell">
                    <div className="flex flex-wrap gap-1">
                      {emp.skills?.slice(0, 3).map((s) => (
                        <span key={s} className="badge-accent text-[10px]">{s}</span>
                      ))}
                      {(emp.skills?.length || 0) > 3 && (
                        <span className="badge-neutral text-[10px]">+{emp.skills!.length - 3}</span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
