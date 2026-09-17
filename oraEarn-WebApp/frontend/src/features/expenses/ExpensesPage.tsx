import { useEffect, useMemo, useState } from 'react';
import { createExpenseCategory, deleteExpenseCategory, getExpenseCategories } from '@/features/expenses/categoriesApi';
import { calculateExpenses, createExpense, deleteExpense, getExpenses, getExpenseSummary, getPaymentMethods, updateExpense } from '@/features/expenses/api';
import { CategoryModal } from '@/features/expenses/components/CategoryModal';
import { ExpenseModal } from '@/features/expenses/components/ExpenseModal';
import { getErrorMessage } from '@/shared/api/client';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatBar } from '@/shared/components/StatBar';
import { StatCard } from '@/shared/components/StatCard';
import { FormField } from '@/shared/form-field';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import type { Expense, ExpenseCalculatorResult, ExpenseSummary } from '@/shared/types';
import { expenseKindLabel, expenseKindOptions } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { Calculator, FolderOpen, Pencil, Plus, Receipt, Tags, Trash2, Wallet } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

const fallbackPaymentMethods = ['Card', 'Cash', 'Bank Transfer', 'Other'];

function formatExpenseDate(date: string) {
  const d = new Date(date);
  return d.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: '2-digit' });
}

export function ExpensesPage() {
  const [categories, setCategories] = useState<{ id: string; title: string }[]>([]);
  const [paymentMethods, setPaymentMethods] = useState<string[]>(fallbackPaymentMethods);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [summary, setSummary] = useState<ExpenseSummary | null>(null);
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null);
  const [monthlyIncome, setMonthlyIncome] = useState('4000');
  const [calculatorResult, setCalculatorResult] = useState<ExpenseCalculatorResult | null>(null);

  const [expenseOpen, setExpenseOpen] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | null>(null);
  const [categoryOpen, setCategoryOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = async () => {
    const [cats, list, expenseSummary, methods] = await Promise.all([
      getExpenseCategories(), getExpenses(), getExpenseSummary(), getPaymentMethods(),
    ]);
    setCategories(cats);
    setExpenses(list);
    setSummary(expenseSummary);
    if (methods.length > 0) setPaymentMethods(methods.map((m) => m.name));
  };

  useEffect(() => { void load(); }, []);

  const filteredExpenses = useMemo(() => {
    if (!selectedCategoryId) return expenses;
    return expenses.filter((e) => e.categoryId === selectedCategoryId);
  }, [expenses, selectedCategoryId]);

  const openCreateExpense = () => {
    setEditingExpense(null);
    setError('');
    setExpenseOpen(true);
  };

  const openEditExpense = (expense: Expense) => {
    setEditingExpense(expense);
    setError('');
    setExpenseOpen(true);
  };

  return (
    <div className="space-y-6">
      <PageHeader
        icon={Receipt}
        title="Expenses & budgets"
        description="Organise spending by category, track fixed vs variable costs, and see how much you can save each month."
        badge={<Badge variant="secondary" className="font-normal">Step 3 of 4</Badge>}
        action={
          <Button className="w-full gap-2 sm:w-auto" onClick={openCreateExpense}>
            <Plus className="h-4 w-4" /> Add expense
          </Button>
        }
      />

      <FeatureTip title="Tip" variant="accent">
        Create a category first (Rent, Food, Transport…), then log each expense in the pop-out form.
      </FeatureTip>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard icon={Receipt} label="Transactions" value={summary?.expenseCount ?? 0} hint="All time" tone="amber" />
        <StatCard icon={Wallet} label="Total spent" value={money(summary?.totalExpenses ?? 0)} hint="Across categories" tone="rose" />
        <StatCard icon={Tags} label="Categories" value={categories.length} hint="Custom budget groups" tone="violet" />
      </div>

      <Tabs defaultValue="expenses">
        <TabsList>
          <TabsTrigger value="expenses">Transactions</TabsTrigger>
          <TabsTrigger value="categories">Categories</TabsTrigger>
          <TabsTrigger value="calculator">Calculator</TabsTrigger>
        </TabsList>

        <TabsContent value="expenses" className="space-y-4">
          <div className="-mx-1 flex gap-2 overflow-x-auto px-1 pb-1">
            <Button
              type="button"
              size="sm"
              variant={selectedCategoryId === null ? 'default' : 'outline'}
              onClick={() => setSelectedCategoryId(null)}
            >
              All
            </Button>
            {categories.map((c) => (
              <Button
                key={c.id}
                type="button"
                size="sm"
                variant={selectedCategoryId === c.id ? 'default' : 'outline'}
                className="shrink-0"
                onClick={() => setSelectedCategoryId(c.id)}
              >
                {c.title}
              </Button>
            ))}
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Expense list</CardTitle>
              <CardDescription>
                {selectedCategoryId
                  ? `Showing ${filteredExpenses.length} in selected category`
                  : `All ${filteredExpenses.length} expenses`}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {filteredExpenses.length === 0 ? (
                <EmptyState
                  icon={Receipt}
                  title="No expenses yet"
                  description={categories.length === 0
                    ? 'Add a category on the Categories tab, then record your first expense.'
                    : 'Log rent, food, transport and more — analytics will show where your money goes.'}
                  className="border-0 bg-transparent py-8"
                  action={<Button className="gap-2" onClick={openCreateExpense}><Plus className="h-4 w-4" /> Add expense</Button>}
                />
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Date</TableHead>
                      <TableHead>Category</TableHead>
                      <TableHead className="hidden md:table-cell">Kind</TableHead>
                      <TableHead className="hidden md:table-cell">Payment</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead />
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredExpenses.map((expense) => (
                      <TableRow key={expense.id}>
                        <TableCell className="whitespace-nowrap font-medium">{formatExpenseDate(expense.date)}</TableCell>
                        <TableCell className="max-w-[8rem] truncate sm:max-w-none">{expense.categoryTitle}</TableCell>
                        <TableCell className="hidden md:table-cell"><Badge variant="outline">{expenseKindLabel(expense.expenseKind)}</Badge></TableCell>
                        <TableCell className="hidden text-muted-foreground md:table-cell">{expense.paymentMethod}</TableCell>
                        <TableCell className="font-medium">{money(expense.amount)}</TableCell>
                        <TableCell>
                          <div className="flex justify-end gap-1">
                            <Button variant="ghost" size="icon" title="Edit expense" onClick={() => openEditExpense(expense)}>
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button variant="ghost" size="icon" title="Delete expense" onClick={() => void deleteExpense(expense.id).then(load)}>
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="categories">
          <Card>
            <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <CardTitle>Manage categories</CardTitle>
                <CardDescription>Create groups like Rent or Groceries, then use them when adding expenses.</CardDescription>
              </div>
              <Button className="w-full gap-2 sm:w-auto" onClick={() => { setError(''); setCategoryOpen(true); }}>
                <Plus className="h-4 w-4" /> Add category
              </Button>
            </CardHeader>
            <CardContent>
              {categories.length === 0 ? (
                <EmptyState
                  icon={FolderOpen}
                  title="No categories"
                  description="Add categories like Rent, Groceries, or Transport."
                  className="border-0 bg-transparent py-6"
                  action={<Button className="gap-2" onClick={() => { setError(''); setCategoryOpen(true); }}><Plus className="h-4 w-4" /> Add category</Button>}
                />
              ) : (
                <ul className="space-y-3">
                  {categories.map((c) => {
                    const spent = summary?.byCategory[c.title] ?? 0;
                    const max = Math.max(...Object.values(summary?.byCategory ?? {}), 1);
                    return (
                      <li key={c.id} className={cn('rounded-lg border p-3')}>
                        <div className="mb-2 flex items-center justify-between gap-2">
                          <p className="font-medium">{c.title}</p>
                          <Button variant="ghost" size="icon" title="Delete category" onClick={() => void deleteExpenseCategory(c.id).then(load)}>
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                        <StatBar label="Spent" value={spent} max={max} display={money(spent)} />
                      </li>
                    );
                  })}
                </ul>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="calculator">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2"><Calculator className="h-4 w-4" /> Budget calculator</CardTitle>
              <CardDescription>See what is left after your logged expenses.</CardDescription>
            </CardHeader>
            <CardContent className="max-w-md space-y-4">
              <FormField label="Monthly income">
                <Input type="number" min="0" value={monthlyIncome} onChange={(e) => setMonthlyIncome(e.target.value)} />
              </FormField>
              <Button
                variant="secondary"
                onClick={() =>
                  void calculateExpenses(
                    Number(monthlyIncome),
                    expenses.map((e) => ({
                      categoryId: e.categoryId,
                      amount: e.amount,
                      date: e.date.slice(0, 10),
                      expenseKind: typeof e.expenseKind === 'number' ? expenseKindOptions[e.expenseKind].value : e.expenseKind,
                      paymentMethod: e.paymentMethod,
                    })),
                  ).then(setCalculatorResult)
                }
              >
                Calculate
              </Button>
              {calculatorResult && (
                <div className="grid gap-3 rounded-lg border p-4 text-sm">
                  <div className="flex justify-between"><span>Total expenses</span><strong>{money(calculatorResult.totalExpenses)}</strong></div>
                  <div className="flex justify-between"><span>Remaining</span><strong>{money(calculatorResult.remainingBalance)}</strong></div>
                  <div className="flex justify-between"><span>Savings rate</span><strong>{calculatorResult.savingsRatePercent}%</strong></div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <ExpenseModal
        open={expenseOpen}
        expense={editingExpense}
        categories={categories}
        paymentMethods={paymentMethods}
        defaultCategoryId={selectedCategoryId ?? undefined}
        saving={saving}
        error={error}
        onClose={() => { if (!saving) { setExpenseOpen(false); setEditingExpense(null); setError(''); } }}
        onSubmit={async (values) => {
          setSaving(true);
          setError('');
          try {
            if (editingExpense) await updateExpense(editingExpense.id, values);
            else await createExpense(values);
            await load();
            setExpenseOpen(false);
            setEditingExpense(null);
          } catch (err) {
            setError(getErrorMessage(err));
          } finally {
            setSaving(false);
          }
        }}
      />

      <CategoryModal
        open={categoryOpen}
        saving={saving}
        error={error}
        onClose={() => { if (!saving) { setCategoryOpen(false); setError(''); } }}
        onSubmit={async (title) => {
          setSaving(true);
          setError('');
          try {
            await createExpenseCategory(title);
            await load();
            setCategoryOpen(false);
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
