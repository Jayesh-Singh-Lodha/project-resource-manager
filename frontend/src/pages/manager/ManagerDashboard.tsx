import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getTeam, getManagedProjects } from '../../api/manager.api';
import PageHeader, { StatCard } from '../../components/ui/PageHeader';

import { Users, FolderKanban, BarChart3, Bot, ArrowRight, ClipboardList, UserPlus } from 'lucide-react';

export default function ManagerDashboard() {
  const { data: team } = useQuery({ queryKey: ['team'], queryFn: getTeam });
  const { data: projects } = useQuery({ queryKey: ['managed-projects'], queryFn: getManagedProjects });

  const benchCount = team?.filter((t) => t.status?.toLowerCase() === 'bench').length || 0;
  const atRiskCount = (projects as any[])?.filter((p: any) => p.healthStatus?.toLowerCase().includes('risk')).length || 0;

  const menuCards = [
    { label: 'Resource Dashboard', path: '/manager/resources', icon: <BarChart3 size={24} />, desc: 'View bench and active employees' },
    { label: 'Allocate Resource', path: '/manager/allocate', icon: <UserPlus size={24} />, desc: 'AI-assisted or direct allocation' },
    { label: 'My Projects', path: '/manager/projects', icon: <FolderKanban size={24} />, desc: 'Project health and milestones' },
    { label: 'Timesheets', path: '/manager/timesheets', icon: <ClipboardList size={24} />, desc: 'Team timesheet status' },
    { label: 'AI Assistant', path: '/manager/ai', icon: <Bot size={24} />, desc: 'Skill match and risk analysis' },
  ];

  return (
    <div>
      <PageHeader title="Manager Dashboard" subtitle="Team overview and quick actions" />

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <StatCard label="Team Members" value={team?.length || 0} icon={<Users size={20} />} />
        <StatCard label="On Bench" value={benchCount} icon={<BarChart3 size={20} />} trend="Available for allocation" />
        <StatCard label="Projects at Risk" value={atRiskCount} icon={<FolderKanban size={20} />} className={atRiskCount > 0 ? 'border-danger/30' : ''} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {menuCards.map((card) => (
          <Link key={card.path} to={card.path} className="glass-card p-5 group hover:border-accent/30 transition-all duration-300 hover:shadow-glow-sm">
            <div className="flex items-start justify-between mb-3">
              <div className="p-2.5 rounded-xl bg-accent/10 text-accent group-hover:bg-accent/20 transition-colors">{card.icon}</div>
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
