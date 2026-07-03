import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getAllUsers } from '../../api/users.api';
import { getAllProjects } from '../../api/projects.api';
import { getAllAllocations } from '../../api/allocations.api';
import PageHeader, { StatCard } from '../../components/ui/PageHeader';
import { Users, FolderKanban, GitBranch, UserCog, Settings, ArrowRight } from 'lucide-react';

export default function AdminDashboard() {
  const { data: users } = useQuery({ queryKey: ['users'], queryFn: getAllUsers });
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: getAllProjects });
  const { data: allocations } = useQuery({ queryKey: ['allocations'], queryFn: getAllAllocations });

  const activeUsers = users?.filter((u) => u.isActive).length || 0;
  const totalProjects = projects?.length || 0;
  const activeAllocations = allocations?.length || 0;

  const menuCards = [
    { label: 'Manage Employees', path: '/admin/employees', icon: <Users size={24} />, desc: 'View, update, and manage employee profiles and skills' },
    { label: 'Manage Projects', path: '/admin/projects', icon: <FolderKanban size={24} />, desc: 'Create projects, manage milestones' },
    { label: 'All Allocations', path: '/admin/allocations', icon: <GitBranch size={24} />, desc: 'Company-wide allocation matrix' },
    { label: 'Manage Users', path: '/admin/users', icon: <UserCog size={24} />, desc: 'Create accounts, reset passwords, manage access' },
    { label: 'System Config', path: '/admin/config', icon: <Settings size={24} />, desc: 'LLM provider, scheduler, max hours' },
  ];

  return (
    <div>
      <PageHeader title="Admin Dashboard" subtitle="System overview and management" />

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <StatCard label="Active Users" value={activeUsers} icon={<Users size={20} />} />
        <StatCard label="Total Projects" value={totalProjects} icon={<FolderKanban size={20} />} />
        <StatCard label="Active Allocations" value={activeAllocations} icon={<GitBranch size={20} />} />
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {menuCards.map((card) => (
          <Link
            key={card.path}
            to={card.path}
            className="glass-card p-5 group hover:border-accent/30 transition-all duration-300 hover:shadow-glow-sm"
          >
            <div className="flex items-start justify-between mb-3">
              <div className="p-2.5 rounded-xl bg-accent/10 text-accent group-hover:bg-accent/20 transition-colors">
                {card.icon}
              </div>
              <ArrowRight size={16} className="text-text-muted group-hover:text-accent transition-colors" />
            </div>
            <h3 className="text-base font-semibold text-text-primary mb-1">{card.label}</h3>
            <p className="text-xs text-text-muted">{card.desc}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
