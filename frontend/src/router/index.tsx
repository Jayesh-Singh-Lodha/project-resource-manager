import { createBrowserRouter, Navigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import AppShell from '../components/layout/AppShell';

// Pages
import LoginPage from '../pages/LoginPage';
import ChangePasswordPage from '../pages/ChangePasswordPage';

// Admin
import AdminDashboard from '../pages/admin/AdminDashboard';
import EmployeeListPage from '../pages/admin/employees/EmployeeListPage';
import ManageSkillsPage from '../pages/admin/employees/ManageSkillsPage';
import ProjectListPage from '../pages/admin/projects/ProjectListPage';
import CreateProjectPage from '../pages/admin/projects/CreateProjectPage';
import EditProjectPage from '../pages/admin/projects/EditProjectPage';
import MilestonesPage from '../pages/admin/projects/MilestonesPage';
import AllAllocationsPage from '../pages/admin/allocations/AllAllocationsPage';
import UserListPage from '../pages/admin/users/UserListPage';
import CreateUserPage from '../pages/admin/users/CreateUserPage';
import EditUserPage from '../pages/admin/users/EditUserPage';
import SystemConfigPage from '../pages/admin/config/SystemConfigPage';

// Manager
import ManagerDashboard from '../pages/manager/ManagerDashboard';
import ResourceDashboardPage from '../pages/manager/ResourceDashboardPage';
import AllocateResourcePage from '../pages/manager/AllocateResourcePage';
import MyProjectsPage from '../pages/manager/MyProjectsPage';
import TimesheetsPage from '../pages/manager/TimesheetsPage';
import AIAssistantPage from '../pages/manager/AIAssistantPage';

// Employee
import EmployeeDashboard from '../pages/employee/EmployeeDashboard';
import SubmitTimesheetPage from '../pages/employee/SubmitTimesheetPage';
import MyTimesheetsPage from '../pages/employee/MyTimesheetsPage';
import MyAllocationsPage from '../pages/employee/MyAllocationsPage';

/**
 * Role-based route guard component.
 * Redirects to login if not authenticated or to change-password if forced.
 */
function ProtectedRoute({ children, allowedRoles }: { children: React.ReactNode; allowedRoles: string[] }) {
  const { isAuthenticated, role, forcePasswordChange } = useAuthStore();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (forcePasswordChange) return <Navigate to="/change-password" replace />;
  if (role && !allowedRoles.includes(role)) {
    const dashboardMap: Record<string, string> = { Admin: '/admin', Manager: '/manager', Employee: '/employee' };
    return <Navigate to={dashboardMap[role] || '/login'} replace />;
  }

  return <>{children}</>;
}

/**
 * Root redirect — sends authenticated users to their dashboard.
 */
function RootRedirect() {
  const { isAuthenticated, role, forcePasswordChange } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (forcePasswordChange) return <Navigate to="/change-password" replace />;
  const dashboardMap: Record<string, string> = { Admin: '/admin', Manager: '/manager', Employee: '/employee' };
  return <Navigate to={dashboardMap[role || ''] || '/login'} replace />;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootRedirect />,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/change-password',
    element: <ChangePasswordPage />,
  },

  // Admin Routes
  {
    element: <ProtectedRoute allowedRoles={['Admin']}><AppShell /></ProtectedRoute>,
    children: [
      { path: '/admin', element: <AdminDashboard /> },
      { path: '/admin/employees', element: <EmployeeListPage /> },
      { path: '/admin/employees/skills', element: <ManageSkillsPage /> },
      { path: '/admin/projects', element: <ProjectListPage /> },
      { path: '/admin/projects/create', element: <CreateProjectPage /> },
      { path: '/admin/projects/:id/edit', element: <EditProjectPage /> },
      { path: '/admin/projects/:id/milestones', element: <MilestonesPage /> },
      { path: '/admin/allocations', element: <AllAllocationsPage /> },
      { path: '/admin/users', element: <UserListPage /> },
      { path: '/admin/users/create', element: <CreateUserPage /> },
      { path: '/admin/users/:id/edit', element: <EditUserPage /> },
      { path: '/admin/config', element: <SystemConfigPage /> },
    ],
  },

  // Manager Routes
  {
    element: <ProtectedRoute allowedRoles={['Manager']}><AppShell /></ProtectedRoute>,
    children: [
      { path: '/manager', element: <ManagerDashboard /> },
      { path: '/manager/resources', element: <ResourceDashboardPage /> },
      { path: '/manager/allocate', element: <AllocateResourcePage /> },
      { path: '/manager/projects', element: <MyProjectsPage /> },
      { path: '/manager/timesheets', element: <TimesheetsPage /> },
      { path: '/manager/ai', element: <AIAssistantPage /> },
    ],
  },

  // Employee Routes
  {
    element: <ProtectedRoute allowedRoles={['Employee']}><AppShell /></ProtectedRoute>,
    children: [
      { path: '/employee', element: <EmployeeDashboard /> },
      { path: '/employee/timesheets/submit', element: <SubmitTimesheetPage /> },
      { path: '/employee/timesheets', element: <MyTimesheetsPage /> },
      { path: '/employee/allocations', element: <MyAllocationsPage /> },
    ],
  },
]);
