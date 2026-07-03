import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import {
  LayoutDashboard,
  Users,
  FolderKanban,
  GitBranch,
  UserCog,
  Settings,
  LogOut,
  Menu,
  X,
  BarChart3,
  UserPlus,
  ClipboardList,
  Bot,
  FileText,
  Calendar,
  ChevronRight,
} from 'lucide-react';
import { cn } from '../../lib/utils';

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
}

const adminNavItems: NavItem[] = [
  { label: 'Dashboard', path: '/admin', icon: <LayoutDashboard size={20} /> },
  { label: 'Manage Employees', path: '/admin/employees', icon: <Users size={20} /> },
  { label: 'Manage Projects', path: '/admin/projects', icon: <FolderKanban size={20} /> },
  { label: 'All Allocations', path: '/admin/allocations', icon: <GitBranch size={20} /> },
  { label: 'Manage Users', path: '/admin/users', icon: <UserCog size={20} /> },
  { label: 'System Config', path: '/admin/config', icon: <Settings size={20} /> },
];

const managerNavItems: NavItem[] = [
  { label: 'Dashboard', path: '/manager', icon: <LayoutDashboard size={20} /> },
  { label: 'Resource Dashboard', path: '/manager/resources', icon: <BarChart3 size={20} /> },
  { label: 'Allocate Resource', path: '/manager/allocate', icon: <UserPlus size={20} /> },
  { label: 'My Projects', path: '/manager/projects', icon: <FolderKanban size={20} /> },
  { label: 'Timesheets', path: '/manager/timesheets', icon: <ClipboardList size={20} /> },
  { label: 'AI Assistant', path: '/manager/ai', icon: <Bot size={20} /> },
];

const employeeNavItems: NavItem[] = [
  { label: 'Dashboard', path: '/employee', icon: <LayoutDashboard size={20} /> },
  { label: 'Submit Timesheet', path: '/employee/timesheets/submit', icon: <FileText size={20} /> },
  { label: 'My Timesheets', path: '/employee/timesheets', icon: <Calendar size={20} /> },
  { label: 'My Allocations', path: '/employee/allocations', icon: <GitBranch size={20} /> },
];

function getNavItems(role: string | null): NavItem[] {
  switch (role) {
    case 'Admin': return adminNavItems;
    case 'Manager': return managerNavItems;
    case 'Employee': return employeeNavItems;
    default: return [];
  }
}

function getRoleLabel(role: string | null): string {
  switch (role) {
    case 'Admin': return 'ADMIN PANEL';
    case 'Manager': return 'MANAGER PANEL';
    case 'Employee': return 'EMPLOYEE PANEL';
    default: return 'PRM TOOL';
  }
}

export default function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const { role, fullName, logout } = useAuthStore();
  const navigate = useNavigate();
  const navItems = getNavItems(role);
  const now = new Date();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Sidebar */}
      <aside
        className={cn(
          'flex flex-col h-full bg-surface border-r border-border transition-all duration-300 ease-in-out z-30',
          sidebarOpen ? 'w-64' : 'w-16'
        )}
      >
        {/* Logo & Toggle */}
        <div className="flex items-center justify-between px-4 h-16 border-b border-border">
          {sidebarOpen && (
            <div className="animate-fade-in">
              <h1 className="text-sm font-bold gradient-text">PRM TOOL</h1>
              <p className="text-[10px] text-text-muted">{getRoleLabel(role)}</p>
            </div>
          )}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-1.5 rounded-lg hover:bg-surface-hover text-text-muted hover:text-text-primary transition-colors"
          >
            {sidebarOpen ? <X size={18} /> : <Menu size={18} />}
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 py-4 space-y-1 overflow-y-auto px-2">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/admin' || item.path === '/manager' || item.path === '/employee'}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
                  isActive
                    ? 'bg-accent/10 text-accent shadow-glow-sm'
                    : 'text-text-muted hover:text-text-primary hover:bg-surface-hover'
                )
              }
            >
              {item.icon}
              {sidebarOpen && (
                <span className="animate-fade-in truncate">{item.label}</span>
              )}
            </NavLink>
          ))}
        </nav>

        {/* Logout */}
        <div className="border-t border-border p-2">
          <button
            onClick={handleLogout}
            className="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm font-medium text-danger hover:bg-danger/10 transition-all duration-200"
          >
            <LogOut size={20} />
            {sidebarOpen && <span className="animate-fade-in">Logout</span>}
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top Bar */}
        <header className="flex items-center justify-between h-16 px-6 border-b border-border bg-surface/50 backdrop-blur-sm">
          <div className="flex items-center gap-2 text-text-muted text-sm">
            <ChevronRight size={14} />
            <span>Welcome back</span>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-xs text-text-muted">
              {now.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
              {' · '}
              {now.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })}
            </span>
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-accent to-violet flex items-center justify-center text-white text-xs font-bold">
                {fullName?.charAt(0)?.toUpperCase() || '?'}
              </div>
              {sidebarOpen && (
                <span className="text-sm font-medium text-text-primary">{fullName}</span>
              )}
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-y-auto p-6">
          <div className="max-w-7xl mx-auto animate-fade-in">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
