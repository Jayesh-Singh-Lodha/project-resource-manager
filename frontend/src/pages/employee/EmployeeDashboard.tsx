import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getLastWeekTimesheetStatus } from '../../api/timesheets.api';
import { getMyAllocations } from '../../api/allocations.api';
import PageHeader from '../../components/ui/PageHeader';
import { FileText, Calendar, GitBranch, AlertTriangle, ArrowRight } from 'lucide-react';

export default function EmployeeDashboard() {
  const { data: lastWeek } = useQuery({ queryKey: ['last-week-status'], queryFn: getLastWeekTimesheetStatus });
  const { data: allocations } = useQuery({ queryKey: ['my-allocations'], queryFn: getMyAllocations });

  const totalUtil = allocations?.reduce((sum, a) => sum + a.utilisationPercent, 0) || 0;
  const showReminder = lastWeek === null || lastWeek?.status?.toLowerCase() === 'missed';

  const menuCards = [
    { label: 'Submit Timesheet', path: '/employee/timesheets/submit', icon: <FileText size={24} />, desc: 'Log hours and activity tags for the week' },
    { label: 'My Timesheets', path: '/employee/timesheets', icon: <Calendar size={24} />, desc: 'View submitted and missed timesheets' },
    { label: 'My Allocations', path: '/employee/allocations', icon: <GitBranch size={24} />, desc: 'Current and past project allocations' },
  ];

  return (
    <div>
      <PageHeader title="Employee Dashboard" />

      {/* Missing Timesheet Reminder */}
      {showReminder && (
        <div className="flex items-center gap-3 p-4 mb-6 rounded-xl bg-warning/10 border border-warning/30 animate-fade-in">
          <AlertTriangle size={20} className="text-warning shrink-0" />
          <div>
            <p className="text-sm font-medium text-warning">Timesheet Reminder</p>
            <p className="text-xs text-text-muted">Your timesheet for last week has not been submitted.</p>
          </div>
          <Link to="/employee/timesheets/submit" className="btn-primary text-xs ml-auto">Submit Now</Link>
        </div>
      )}

      {/* Utilisation Summary */}
      <div className="glass-card p-5 mb-6">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm text-text-muted">Total Utilisation</span>
          <span className="text-sm font-medium text-text-primary">{totalUtil}%</span>
        </div>
        <div className="w-full h-2 bg-border rounded-full overflow-hidden">
          <div
            className="h-full rounded-full bg-gradient-to-r from-accent to-violet transition-all duration-500"
            style={{ width: `${Math.min(totalUtil, 100)}%` }}
          />
        </div>
        <p className="text-xs text-text-muted mt-1">{allocations?.length || 0} active allocation(s)</p>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
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
