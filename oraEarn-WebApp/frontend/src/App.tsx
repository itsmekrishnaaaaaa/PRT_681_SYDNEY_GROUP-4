import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import {
  AdminRoute,
  AuthProvider,
  GuestRoute,
  LoginPage,
  ProtectedRoute,
  RegisterPage,
} from './auth';
import { AdminPage } from '@/features/admin/AdminPage';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { ExpensesPage } from '@/features/expenses/ExpensesPage';
import { JobsPage } from '@/features/jobs/JobsPage';
import { SavingsGoalsPage } from '@/features/savings/SavingsGoalsPage';
import { ShiftsPage } from '@/features/shifts/ShiftsPage';
import { Layout } from '@/shared/layout/Layout';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<GuestRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/jobs" element={<JobsPage />} />
              <Route path="/shifts" element={<ShiftsPage />} />
              <Route path="/savings" element={<SavingsGoalsPage />} />
              <Route path="/expenses" element={<ExpensesPage />} />
            </Route>
          </Route>

          <Route element={<AdminRoute />}>
            <Route element={<Layout />}>
              <Route path="/admin" element={<AdminPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
