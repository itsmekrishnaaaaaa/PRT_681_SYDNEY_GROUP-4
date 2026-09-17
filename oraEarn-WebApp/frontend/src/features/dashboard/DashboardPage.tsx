import { useCallback, useEffect, useState } from 'react';
import { getErrorMessage } from '@/shared/api/client';
import { getMyAnalytics } from '@/features/dashboard/api';
import { AnalyticsPeriodFilter } from '@/features/dashboard/components/AnalyticsPeriodFilter';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatBar } from '@/shared/components/StatBar';
import { StatCard } from '@/shared/components/StatCard';
import { TimelineChart } from '@/features/dashboard/components/TimelineChart';
import { VisaComplianceCard } from '@/features/shifts/components/VisaComplianceCard';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import type { AnalyticsPeriodKey, UserAnalytics } from '@/shared/types';
import { downloadAnalyticsExcel, downloadAnalyticsPdf } from '@/features/dashboard/utils/analyticsExport';
import { money } from '@/shared/utils/dates';
import { ArrowDownRight, ArrowUpRight, Clock, FileSpreadsheet, FileText, LayoutDashboard, Loader2, Receipt, Target, TrendingUp, Wallet } from 'lucide-react';

export function DashboardPage() {
  const [period, setPeriod] = useState<AnalyticsPeriodKey>('7d');
  const [analytics, setAnalytics] = useState<UserAnalytics | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  const loadAnalytics = useCallback(async (selectedPeriod: AnalyticsPeriodKey) => {
    setLoading(true);
    try {
      const data = await getMyAnalytics(selectedPeriod);
      setAnalytics(data);
      setError('');
    } catch (err) {
      setError(getErrorMessage(err, 'Could not load analytics.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadAnalytics(period);
  }, [period, loadAnalytics]);

  if (error && !analytics) {
    return <p className="text-destructive">{error}</p>;
  }

  if (!analytics) {
    return (
      <div className="flex items-center gap-2 text-muted-foreground">
        <Loader2 className="h-4 w-4 animate-spin" />
        Loading your analytics…
      </div>
    );
  }

  const maxCategory = Math.max(...Object.values(analytics.expensesByCategory), 1);
  const netPositive = analytics.netBalance >= 0;
  const timeline = analytics.timelineBreakdown ?? [];

  return (
    <div className="space-y-6">
      <PageHeader
        icon={LayoutDashboard}
        title="My analytics"
        description="Your personal command centre — earnings, spending, savings progress, and visa compliance in one place."
        action={
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" size="sm" onClick={() => downloadAnalyticsExcel(analytics)}>
              <FileSpreadsheet className="h-4 w-4" />
              Download Excel
            </Button>
            <Button variant="outline" size="sm" onClick={() => downloadAnalyticsPdf(analytics)}>
              <FileText className="h-4 w-4" />
              Download PDF
            </Button>
          </div>
        }
      />

      <FeatureTip variant="accent" title="Recommended view">
        Start with <strong>7 days</strong> for daily shift patterns, switch to <strong>1 month</strong> for spending trends, or <strong>12 months</strong> for the big picture.
      </FeatureTip>

      <AnalyticsPeriodFilter value={period} onChange={setPeriod} />

      <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
        <Badge variant="outline">{analytics.periodLabel}</Badge>
        <span>
          {analytics.dateFrom} → {analytics.dateTo}
        </span>
        {loading && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          icon={Wallet}
          label="Net balance"
          value={money(analytics.netBalance)}
          hint="Earnings minus expenses in period"
          tone={netPositive ? 'emerald' : 'rose'}
          trend={netPositive ? <ArrowUpRight className="h-4 w-4" /> : <ArrowDownRight className="h-4 w-4" />}
        />
        <StatCard icon={TrendingUp} label="Earnings" value={money(analytics.totalEarnings)} hint={`${analytics.shiftCount} shifts · ${analytics.totalHours.toFixed(1)} hrs`} tone="violet" />
        <StatCard icon={Receipt} label="Expenses" value={money(analytics.totalExpenses)} hint={`${analytics.expenseCount} transactions`} tone="amber" />
        <StatCard icon={Target} label="Savings targets" value={money(analytics.totalSavingsTarget)} hint={`${analytics.savingsGoalCount} goals · ${analytics.totalAllocationPercent}% allocated`} tone="blue" />
      </div>

      {analytics.visaCompliance?.applies && (
        <VisaComplianceCard compliance={analytics.visaCompliance} />
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Shift hours</CardTitle>
            <CardDescription>{analytics.periodLabel} — hours worked per bucket</CardDescription>
          </CardHeader>
          <CardContent>
            <TimelineChart title="Shift activity" data={timeline} mode="shifts" />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Expenses</CardTitle>
            <CardDescription>{analytics.periodLabel} — spending per bucket</CardDescription>
          </CardHeader>
          <CardContent>
            <TimelineChart title="Expense activity" data={timeline} mode="expenses" />
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Spending by category</CardTitle>
            <CardDescription>Where your money goes in this period</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {Object.keys(analytics.expensesByCategory).length === 0 ? (
              <EmptyState icon={Receipt} title="No expenses in this period" description="Log expenses to see category breakdowns here." className="border-0 bg-transparent py-6" />
            ) : (
              Object.entries(analytics.expensesByCategory).map(([cat, amt]) => (
                <StatBar key={cat} label={cat} value={amt} max={maxCategory} display={money(amt)} />
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Expense types</CardTitle>
            <CardDescription>Fixed · Variable · One-off</CardDescription>
          </CardHeader>
          <CardContent className="flex flex-wrap gap-2">
            {Object.entries(analytics.expensesByKind).length === 0 ? (
              <p className="text-sm text-muted-foreground">No data in this period.</p>
            ) : (
              Object.entries(analytics.expensesByKind).map(([kind, count]) => (
                <Badge key={kind} variant="secondary">{kind}: {count}</Badge>
              ))
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><Clock className="h-4 w-4" /> Work summary</CardTitle>
          <CardDescription>Totals for {analytics.periodLabel.toLowerCase()}</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-3 text-sm">
          <div><p className="text-muted-foreground">Jobs</p><p className="text-xl font-semibold">{analytics.jobCount}</p></div>
          <div><p className="text-muted-foreground">Shifts logged</p><p className="text-xl font-semibold">{analytics.shiftCount}</p></div>
          <div><p className="text-muted-foreground">Hours worked</p><p className="text-xl font-semibold">{analytics.totalHours.toFixed(1)}</p></div>
        </CardContent>
      </Card>
    </div>
  );
}
