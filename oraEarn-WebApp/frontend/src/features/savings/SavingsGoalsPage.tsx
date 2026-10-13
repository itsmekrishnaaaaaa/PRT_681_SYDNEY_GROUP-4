import { useEffect, useState } from 'react';
import {
  contributeToSavingsGoal,
  createSavingsGoal,
  deleteSavingsGoal,
  getSavingsGoalAnalytics,
  getSavingsGoals,
  savingsStatusLabel,
  updateSavingsGoal,
} from '@/features/savings/api';
import { SavingsGoalModal } from '@/features/savings/components/SavingsGoalModal';
import { getErrorMessage } from '@/shared/api/client';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatBar } from '@/shared/components/StatBar';
import { StatCard } from '@/shared/components/StatCard';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import type { SavingsGoal, SavingsGoalAnalytics } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { BarChart3, Calendar, Clock, Pencil, PiggyBank, Plus, Target, Trash2, TrendingUp } from 'lucide-react';

const statusVariant = (status: string) => {
  if (status === 'Completed') return 'default';
  if (status === 'InProgress') return 'secondary';
  return 'outline';
};

export function SavingsGoalsPage() {
  const [goals, setGoals] = useState<SavingsGoal[]>([]);
  const [analytics, setAnalytics] = useState<SavingsGoalAnalytics | null>(null);
  const [contributeAmounts, setContributeAmounts] = useState<Record<string, string>>({});
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<SavingsGoal | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = async () => {
    const [goalList, analyticsData] = await Promise.all([
      getSavingsGoals(),
      getSavingsGoalAnalytics(),
    ]);
    setGoals(goalList);
    setAnalytics(analyticsData);
  };

  useEffect(() => { void load(); }, []);

  const totalAllocation = goals.reduce((sum, g) => sum + g.allocationPercentage, 0);
  const totalTarget = goals.reduce((sum, g) => sum + g.targetAmount, 0);
  const totalSaved = goals.reduce((sum, g) => sum + g.currentAmount, 0);
  const allocationOk = totalAllocation <= 100;
  const analyticsByGoal = new Map(analytics?.goals.map((g) => [g.goalId, g]) ?? []);

  const openCreate = () => {
    setEditing(null);
    setError('');
    setModalOpen(true);
  };

  const handleContribute = async (goalId: string) => {
    const amount = Number(contributeAmounts[goalId]);
    if (!amount || amount <= 0) return;
    try {
      await contributeToSavingsGoal(goalId, amount);
      setContributeAmounts((a) => ({ ...a, [goalId]: '' }));
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        icon={PiggyBank}
        title="Savings goals"
        description="Leftover money (shift pay minus expenses) is split into each goal by its allocation %. Log a shift or expense and the saved amount updates by itself."
        badge={<Badge variant="secondary" className="font-normal">Step 4 of 4</Badge>}
        action={
          <Button className="w-full gap-2 sm:w-auto" onClick={openCreate}>
            <Plus className="h-4 w-4" /> Add goal
          </Button>
        }
      />

      <FeatureTip title="Updates with your money">
        Each goal takes its % of leftover pay (all shift earnings minus all expenses). Add extra with the box on the card if you already put cash aside.
      </FeatureTip>

      <FeatureTip variant="accent" title="Aura can help">
        Ask Aura (bottom-right chat) how many hours you need, whether you&apos;re on track, or if visa limits affect your plan.
      </FeatureTip>

      {error && !modalOpen && (
        <p className="rounded-lg border px-3 py-2 text-sm text-destructive">{error}</p>
      )}

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard icon={Target} label="Active goals" value={goals.length} hint="In progress or pending" tone="violet" />
        <StatCard icon={PiggyBank} label="Saved so far" value={money(totalSaved)} hint="Leftover pay × allocation" tone="emerald" />
        <StatCard icon={TrendingUp} label="Combined target" value={money(totalTarget)} hint="All goal targets" tone="blue" />
        <StatCard
          icon={BarChart3}
          label="Income allocated"
          value={`${totalAllocation.toFixed(1)}%`}
          hint={allocationOk ? 'Within 100% limit' : 'Over 100% — reduce allocations'}
          tone={allocationOk ? 'slate' : 'rose'}
        />
      </div>

      <Tabs defaultValue="goals">
        <TabsList>
          <TabsTrigger value="goals">Goals</TabsTrigger>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
        </TabsList>
        <TabsContent value="goals">
          {goals.length === 0 ? (
            <EmptyState
              icon={PiggyBank}
              title="No savings goals yet"
              description="Create a goal and set the % of leftover pay to put aside. Logging a shift or expense updates the saved amount automatically."
              action={<Button className="gap-2" onClick={openCreate}><Plus className="h-4 w-4" /> Add goal</Button>}
            />
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {goals.map((goal) => {
                const ga = analyticsByGoal.get(goal.id);
                return (
                  <Card key={goal.id} className="relative overflow-hidden">
                    <CardHeader className="pb-3">
                      <div className="flex items-start justify-between gap-2">
                        <div>
                          <CardTitle className="text-base flex items-center gap-2 flex-wrap">
                            <Target className="h-4 w-4" />{goal.title}
                            <Badge variant={statusVariant(goal.status)}>{savingsStatusLabel[goal.status] ?? goal.status}</Badge>
                          </CardTitle>
                          <CardDescription>{money(goal.currentAmount)} / {money(goal.targetAmount)}</CardDescription>
                        </div>
                        <div className="flex shrink-0">
                          <Button variant="ghost" size="icon" title="Edit goal" onClick={() => { setEditing(goal); setError(''); setModalOpen(true); }}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" title="Delete goal" onClick={() => void deleteSavingsGoal(goal.id).then(load)}>
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <StatBar label="Progress" value={goal.progressPercent} max={100} display={`${goal.progressPercent}%`} />
                      <StatBar label="Allocation" value={goal.allocationPercentage} max={100} display={`${goal.allocationPercentage}%`} />
                      {goal.targetDate && (
                        <p className="text-xs text-muted-foreground flex items-center gap-1">
                          <Calendar className="h-3 w-3" /> Target date: {new Date(goal.targetDate).toLocaleDateString()}
                        </p>
                      )}
                      {ga && goal.status !== 'Completed' && (
                        <p className="text-xs text-muted-foreground flex items-center gap-1">
                          <Clock className="h-3 w-3" /> ~{ga.hoursNeeded} hrs needed · finish ~{ga.estimatedFinishDate ? new Date(ga.estimatedFinishDate).toLocaleDateString() : '—'}
                        </p>
                      )}
                      {goal.status !== 'Completed' && (
                        <div className="flex flex-col gap-2 pt-1 sm:flex-row">
                          <Input
                            type="number"
                            min="0"
                            step="0.01"
                            placeholder="Add savings"
                            value={contributeAmounts[goal.id] ?? ''}
                            onChange={(e) => setContributeAmounts((a) => ({ ...a, [goal.id]: e.target.value }))}
                          />
                          <Button type="button" variant="secondary" className="sm:w-auto" onClick={() => void handleContribute(goal.id)}>Add</Button>
                        </div>
                      )}
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>

        <TabsContent value="analytics">
          {!analytics ? (
            <p className="text-sm text-muted-foreground">Loading analytics…</p>
          ) : (
            <div className="space-y-6">
              <div className="grid gap-4 sm:grid-cols-3">
                <Card>
                  <CardHeader className="pb-2">
                    <CardDescription className="flex items-center gap-1"><TrendingUp className="h-3.5 w-3.5" /> Est. monthly income</CardDescription>
                    <CardTitle>{money(analytics.monthlyIncomeEstimate)}</CardTitle>
                  </CardHeader>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardDescription>Net balance</CardDescription>
                    <CardTitle className={analytics.netBalance < 0 ? 'text-destructive' : ''}>{money(analytics.netBalance)}</CardTitle>
                  </CardHeader>
                </Card>
                <Card>
                  <CardHeader className="pb-2">
                    <CardDescription className="flex items-center gap-1"><BarChart3 className="h-3.5 w-3.5" /> Allocation used</CardDescription>
                    <CardTitle>{analytics.totalAllocationPercent.toFixed(1)}%</CardTitle>
                  </CardHeader>
                </Card>
              </div>

              <Card>
                <CardHeader>
                  <CardTitle>Goal analytics</CardTitle>
                  <CardDescription>Hours to work, estimated finish, pending vs completed</CardDescription>
                </CardHeader>
                <CardContent className="grid gap-4 md:grid-cols-2">
                  {analytics.goals.length === 0 ? (
                    <EmptyState icon={BarChart3} title="No analytics yet" description="Create a savings goal to see hours needed, finish dates, and monthly estimates." className="md:col-span-2 border-0 bg-transparent" />
                  ) : analytics.goals.map((g) => (
                    <div key={g.goalId} className="rounded-lg border p-4 space-y-2">
                      <div className="flex items-center justify-between gap-2">
                        <p className="font-medium">{g.title}</p>
                        <Badge variant={statusVariant(g.status)}>{savingsStatusLabel[g.status]}</Badge>
                      </div>
                      <StatBar label="Progress" value={g.progressPercent} max={100} display={`${g.progressPercent}%`} />
                      <div className="grid grid-cols-2 gap-2 text-xs text-muted-foreground">
                        <span>Remaining: {money(g.remainingAmount)}</span>
                        <span>Monthly save: {money(g.estimatedMonthlySavings)}</span>
                        <span className="flex items-center gap-1"><Clock className="h-3 w-3" /> {g.hoursNeeded} hrs needed</span>
                        <span>@ ${g.avgHourlyRate}/hr · {g.avgHoursPerWeek} hrs/wk</span>
                        <span className="flex items-center gap-1"><Calendar className="h-3 w-3" /> Finish: {g.estimatedFinishDate ? new Date(g.estimatedFinishDate).toLocaleDateString() : '—'}</span>
                        <span>{g.weeksToComplete != null ? `${g.weeksToComplete} weeks` : '—'}</span>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
            </div>
          )}
        </TabsContent>
      </Tabs>

      <SavingsGoalModal
        open={modalOpen}
        goal={editing}
        saving={saving}
        error={error}
        onClose={() => { if (!saving) { setModalOpen(false); setEditing(null); setError(''); } }}
        onSubmit={async (values) => {
          setSaving(true);
          setError('');
          try {
            if (editing) await updateSavingsGoal(editing.id, values);
            else await createSavingsGoal(values);
            await load();
            setModalOpen(false);
            setEditing(null);
          } catch (err) {
            setError(getErrorMessage(err));
          } finally {
            setSaving(false);
          }
        }}
      />
    </div>
  );
}
