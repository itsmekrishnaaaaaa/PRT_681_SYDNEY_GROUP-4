import { type FormEvent, useEffect, useState } from 'react';
import {
  contributeToSavingsGoal,
  createSavingsGoal,
  deleteSavingsGoal,
  getSavingsGoalAnalytics,
  getSavingsGoals,
  savingsStatusLabel,
} from '@/features/savings/api';
import { getErrorMessage } from '@/shared/api/client';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatBar } from '@/shared/components/StatBar';
import { StatCard } from '@/shared/components/StatCard';
import {
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  pickErrors,
  useFormValidation,
  validators,
} from '@/shared/form-field';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import type { SavingsGoal, SavingsGoalAnalytics } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { BarChart3, Calendar, Clock, PiggyBank, Target, Trash2, TrendingUp } from 'lucide-react';
const defaultForm = {
  title: '',
  targetAmount: '',
  currentAmount: '0',
  allocationPercentage: '10',
  targetDate: '',
};

type GoalFields = 'title' | 'targetAmount' | 'currentAmount' | 'allocationPercentage' | '_form';

const statusVariant = (status: string) => {
  if (status === 'Completed') return 'default';
  if (status === 'InProgress') return 'secondary';
  return 'outline';
};

export function SavingsGoalsPage() {
  const [goals, setGoals] = useState<SavingsGoal[]>([]);
  const [analytics, setAnalytics] = useState<SavingsGoalAnalytics | null>(null);
  const [form, setForm] = useState(defaultForm);
  const [contributeAmounts, setContributeAmounts] = useState<Record<string, string>>({});
  const { errors, clearError, validate, setFormError } = useFormValidation<GoalFields>();

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

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['title', validators.required(form.title, 'Goal title is required.')],
          ['targetAmount', validators.minNumber(form.targetAmount, 0.01, 'Enter a valid target amount.')],
          ['currentAmount', form.currentAmount ? validators.minNumber(form.currentAmount, 0, 'Starting amount cannot be negative.') : undefined],
          ['allocationPercentage', validators.minNumber(form.allocationPercentage, 0, 'Allocation must be at least 0%.') ?? validators.maxNumber(form.allocationPercentage, 100, 'Allocation cannot exceed 100%.')],
        ]),
      )
    ) {
      return;
    }

    try {
      await createSavingsGoal({
        title: form.title,
        targetAmount: Number(form.targetAmount),
        currentAmount: Number(form.currentAmount),
        allocationPercentage: Number(form.allocationPercentage),
        targetDate: form.targetDate || null,
      });
      setForm(defaultForm);
      await load();
    } catch (err) { setFormError(getErrorMessage(err)); }
  };

  const handleContribute = async (goalId: string) => {
    const amount = Number(contributeAmounts[goalId]);
    if (!amount || amount <= 0) return;
    try {
      await contributeToSavingsGoal(goalId, amount);
      setContributeAmounts((a) => ({ ...a, [goalId]: '' }));
      await load();
    } catch (err) { setFormError(getErrorMessage(err)); }
  };

  const analyticsByGoal = new Map(analytics?.goals.map((g) => [g.goalId, g]) ?? []);

  return (
    <div className="space-y-6">
      <PageHeader
        icon={PiggyBank}
        title="Savings goals"
        description="Set targets, allocate % of income, track progress, and see how many hours you need to reach each goal."
        badge={<Badge variant="secondary" className="font-normal">Step 4 of 4</Badge>}
      />

      <FeatureTip variant="accent" title="Aura can help">
        Ask Aura (bottom-right chat) how many hours you need, whether you&apos;re on track, or if visa limits affect your plan.
      </FeatureTip>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard icon={Target} label="Active goals" value={goals.length} hint="In progress or pending" tone="violet" />
        <StatCard icon={PiggyBank} label="Saved so far" value={money(totalSaved)} hint="Total contributions" tone="emerald" />
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
          <div className="grid gap-6 lg:grid-cols-5">
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><PiggyBank className="h-4 w-4" /> New goal</CardTitle>
              </CardHeader>
              <CardContent>
                <FormErrorSummary errors={errors} />
                <form className="space-y-4" noValidate onSubmit={handleSubmit}>
                  <FormField label="Title" error={errors.title} required>
                    <Input
                      placeholder="Emergency fund"
                      className={fieldInvalidClass(errors.title)}
                      value={form.title}
                      onChange={(e) => {
                        setForm({ ...form, title: e.target.value });
                        clearError('title');
                      }}
                    />
                  </FormField>
                  <FormField label="Target amount" error={errors.targetAmount} required>
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      className={fieldInvalidClass(errors.targetAmount)}
                      value={form.targetAmount}
                      onChange={(e) => {
                        setForm({ ...form, targetAmount: e.target.value });
                        clearError('targetAmount');
                      }}
                    />
                  </FormField>
                  <FormField label="Starting amount (optional)" error={errors.currentAmount}>
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      className={fieldInvalidClass(errors.currentAmount)}
                      value={form.currentAmount}
                      onChange={(e) => {
                        setForm({ ...form, currentAmount: e.target.value });
                        clearError('currentAmount');
                      }}
                    />
                  </FormField>
                  <FormField label="Allocation % of income" error={errors.allocationPercentage} required>
                    <Input
                      type="number"
                      min="0"
                      max="100"
                      step="0.1"
                      className={fieldInvalidClass(errors.allocationPercentage)}
                      value={form.allocationPercentage}
                      onChange={(e) => {
                        setForm({ ...form, allocationPercentage: e.target.value });
                        clearError('allocationPercentage');
                      }}
                    />
                  </FormField>
                  <FormField label="Target date (optional)">
                    <Input
                      type="date"
                      value={form.targetDate}
                      onChange={(e) => setForm({ ...form, targetDate: e.target.value })}
                    />
                  </FormField>
                  <Button type="submit">Create goal</Button>
                </form>
              </CardContent>
            </Card>

            <div className="lg:col-span-3 grid gap-4 sm:grid-cols-2">
              {goals.length === 0 ? (
                <EmptyState
                  icon={PiggyBank}
                  title="No savings goals yet"
                  description="Create your first goal — set a target amount, optional deadline, and % of income to auto-save."
                  className="sm:col-span-2"
                />
              ) : goals.map((goal) => {
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
                        <Button variant="ghost" size="icon" onClick={() => void deleteSavingsGoal(goal.id).then(load)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
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
          </div>
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
      </Tabs>    </div>
  );
}
