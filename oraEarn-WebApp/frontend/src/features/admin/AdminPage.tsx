import { useEffect, useState } from 'react';
import { getAdminAnalytics, updateUserRoles } from '@/features/admin/api';
import { getErrorMessage } from '@/shared/api/client';
import { RegistrationApprovals } from '@/features/admin/components/RegistrationApprovals';
import { StatBar } from '@/shared/components/StatBar';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import type { AdminAnalytics } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { BarChart3, ClipboardList, Shield, Users } from 'lucide-react';

const roleOptions = ['Admin', 'User'];

export function AdminPage() {
  const [analytics, setAnalytics] = useState<AdminAnalytics | null>(null);
  const [error, setError] = useState('');

  const load = async () => setAnalytics(await getAdminAnalytics());
  useEffect(() => { void load(); }, []);

  const handleRoleToggle = async (userId: string, currentRoles: string[], role: string) => {
    setError('');
    const roles = currentRoles.includes(role) ? currentRoles.filter((r) => r !== role) : [...currentRoles, role];
    if (roles.length === 0) { setError('Each user must have at least one role.'); return; }
    try {
      await updateUserRoles(userId, roles);
      await load();
    } catch (err) { setError(getErrorMessage(err)); }
  };

  if (!analytics) return <p className="text-muted-foreground">Loading admin analytics...</p>;

  const maxUserSpend = Math.max(...analytics.users.map((u) => u.totalExpenses), 1);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2"><Shield className="h-6 w-6" /> Admin dashboard</h1>
        <p className="text-muted-foreground">Platform analytics and user management — admin only.</p>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <Tabs defaultValue="analytics">
        <TabsList>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
          <TabsTrigger value="registrations">Registrations</TabsTrigger>
        </TabsList>

        <TabsContent value="registrations">
          <RegistrationApprovals />
        </TabsContent>

        <TabsContent value="analytics">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Card><CardHeader className="pb-2"><CardDescription className="flex items-center gap-1"><Users className="h-3.5 w-3.5" /> Users</CardDescription><CardTitle>{analytics.totalUsers}</CardTitle></CardHeader><CardContent className="text-xs text-muted-foreground">{analytics.totalAdmins} admins</CardContent></Card>
        <Card><CardHeader className="pb-2"><CardDescription>Total shifts</CardDescription><CardTitle>{analytics.totalShifts}</CardTitle></CardHeader><CardContent className="text-xs text-muted-foreground">{analytics.totalHours.toFixed(1)} hours · {money(analytics.totalEarnings)} earned</CardContent></Card>
        <Card><CardHeader className="pb-2"><CardDescription>Total expenses</CardDescription><CardTitle>{money(analytics.totalExpenseAmount)}</CardTitle></CardHeader><CardContent className="text-xs text-muted-foreground">{analytics.totalExpenses} transactions</CardContent></Card>
        <Card><CardHeader className="pb-2"><CardDescription>Savings goals</CardDescription><CardTitle>{analytics.totalSavingsGoals}</CardTitle></CardHeader><CardContent className="text-xs text-muted-foreground">{analytics.totalJobs} jobs across platform</CardContent></Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><BarChart3 className="h-4 w-4" /> User spending comparison</CardTitle>
            <CardDescription>Total expenses per user</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {analytics.users.map((user) => (
              <StatBar
                key={user.id}
                label={`${user.firstName} ${user.lastName}`}
                value={user.totalExpenses}
                max={maxUserSpend}
                display={money(user.totalExpenses)}
              />
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>User earnings</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            {analytics.users.map((user) => (
              <div key={user.id} className="flex flex-col gap-1 border-b border-border pb-2 text-sm sm:flex-row sm:items-center sm:justify-between">
                <span>{user.firstName} {user.lastName}</span>
                <span className="font-medium">{money(user.totalEarnings)} · {user.shiftCount} shifts</span>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><ClipboardList className="h-4 w-4" /> User management</CardTitle>
          <CardDescription>Toggle Admin / User roles. Regular users only see their own analytics on Dashboard.</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>User</TableHead>
                <TableHead>Jobs</TableHead>
                <TableHead>Shifts</TableHead>
                <TableHead>Earned</TableHead>
                <TableHead>Spent</TableHead>
                <TableHead>Goals</TableHead>
                <TableHead>Roles</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {analytics.users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>
                    <div><p className="font-medium">{user.firstName} {user.lastName}</p><p className="text-xs text-muted-foreground">{user.email}</p></div>
                  </TableCell>
                  <TableCell>{user.jobCount}</TableCell>
                  <TableCell>{user.shiftCount}</TableCell>
                  <TableCell>{money(user.totalEarnings)}</TableCell>
                  <TableCell>{money(user.totalExpenses)}</TableCell>
                  <TableCell>{user.savingsGoalCount}</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {roleOptions.map((role) => (
                        <Button
                          key={role}
                          size="sm"
                          variant={user.roles.includes(role) ? 'default' : 'outline'}
                          onClick={() => void handleRoleToggle(user.id, user.roles, role)}
                        >
                          {role}
                        </Button>
                      ))}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
