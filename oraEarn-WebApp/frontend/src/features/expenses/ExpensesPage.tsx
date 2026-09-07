import { type FormEvent, useEffect, useMemo, useState } from 'react';
import { createExpenseCategory, getExpenseCategories } from '@/features/expenses/categoriesApi';
import { calculateExpenses, createExpense, deleteExpense, getExpenses, getExpenseSummary, getPaymentMethods } from '@/features/expenses/api';
import { getErrorMessage } from '@/shared/api/client';
import { CategorySidebar } from '@/features/expenses/components/CategorySidebar';
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import type { Expense, ExpenseCalculatorResult, ExpenseKind, ExpenseSummary } from '@/shared/types';
import { expenseKindLabel, expenseKindOptions } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { Calculator, CreditCard, FolderOpen, Receipt, Tags, Trash2, Wallet } from 'lucide-react';

const defaultForm = {
  categoryId: '',
  amount: '',
  date: new Date().toISOString().slice(0, 10),
  expenseKind: 'Fixed' as ExpenseKind,
  paymentMethod: 'Card',
};

const fallbackPaymentMethods = ['Card', 'Cash', 'Bank Transfer', 'Other'];

type ExpenseFields = 'categoryId' | 'amount' | 'date' | '_form';

function formatExpenseDate(date: string) {
  const d = new Date(date);
  return d.toLocaleDateString(undefined, { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' });
}

export function ExpensesPage() {
  const [categories, setCategories] = useState<{ id: string; title: string }[]>([]);
  const [paymentMethods, setPaymentMethods] = useState<string[]>(fallbackPaymentMethods);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [summary, setSummary] = useState<ExpenseSummary | null>(null);
  const [form, setForm] = useState(defaultForm);
  const [newCategory, setNewCategory] = useState('');
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null);
  const [monthlyIncome, setMonthlyIncome] = useState('4000');
  const [calculatorResult, setCalculatorResult] = useState<ExpenseCalculatorResult | null>(null);
  const { errors, clearError, validate, setFormError } = useFormValidation<ExpenseFields>();

  const load = async () => {
    const [cats, list, expenseSummary, methods] = await Promise.all([
      getExpenseCategories(), getExpenses(), getExpenseSummary(), getPaymentMethods(),
    ]);
    setCategories(cats);
    setExpenses(list);
    setSummary(expenseSummary);
    if (methods.length > 0) setPaymentMethods(methods.map((m) => m.name));
    if (cats.length > 0) setForm((f) => ({ ...f, categoryId: f.categoryId || cats[0].id }));
  };

  useEffect(() => { void load(); }, []);

  const filteredExpenses = useMemo(() => {
    if (!selectedCategoryId) return expenses;
    return expenses.filter((e) => e.categoryId === selectedCategoryId);
  }, [expenses, selectedCategoryId]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['categoryId', validators.required(form.categoryId, 'Select a category.')],
          ['amount', validators.minNumber(form.amount, 0.01, 'Enter a valid amount.')],
          ['date', validators.dateRequired(form.date, 'Date is required.')],
        ]),
      )
    ) {
      return;
    }

    try {
      await createExpense({
        categoryId: form.categoryId,
        amount: Number(form.amount),
        date: form.date,
        expenseKind: form.expenseKind,
        paymentMethod: form.paymentMethod,
      });
      setForm({ ...defaultForm, categoryId: form.categoryId, date: new Date().toISOString().slice(0, 10) });
      await load();
    } catch (err) { setFormError(getErrorMessage(err)); }
  };

  const addCategory = () => {
    void createExpenseCategory(newCategory.trim()).then(() => {
      setNewCategory('');
      return load();
    });
  };

  return (
    <div className="space-y-6">
      <PageHeader
        icon={Receipt}
        title="Expenses & budgets"
        description="Organise spending by category, track fixed vs variable costs, and see how much you can save each month."
        badge={<Badge variant="secondary" className="font-normal">Step 3 of 4</Badge>}
      />

      <FeatureTip title="Tip" variant="accent">
        Create categories first (Rent, Food, Transport…), then log each expense with date and payment method for accurate analytics.
      </FeatureTip>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard icon={Receipt} label="Transactions" value={summary?.expenseCount ?? 0} hint="All time" tone="amber" />
        <StatCard icon={Wallet} label="Total spent" value={money(summary?.totalExpenses ?? 0)} hint="Across categories" tone="rose" />
        <StatCard icon={Tags} label="Categories" value={categories.length} hint="Custom budget groups" tone="violet" />
      </div>

      <Tabs defaultValue="expenses">
        <TabsList>
          <TabsTrigger value="expenses">Expenses</TabsTrigger>
          <TabsTrigger value="categories">Categories</TabsTrigger>
        </TabsList>

        <TabsContent value="expenses">
          <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
            <CategorySidebar
              categories={categories}
              summary={summary}
              selectedCategoryId={selectedCategoryId}
              onSelectCategory={setSelectedCategoryId}
              newCategory={newCategory}
              onNewCategoryChange={setNewCategory}
              onAddCategory={addCategory}
            />

            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2"><CreditCard className="h-4 w-4" /> Add expense</CardTitle>
                  <CardDescription>Date is required — pick when the expense occurred.</CardDescription>
                </CardHeader>
                <CardContent>
                  <FormErrorSummary errors={errors} />
                  <form className="grid gap-4 sm:grid-cols-2" noValidate onSubmit={handleSubmit}>
                    <FormField label="Category" error={errors.categoryId} required className="sm:col-span-2">
                      <Select
                        value={form.categoryId || undefined}
                        onValueChange={(v) => {
                          setForm({ ...form, categoryId: v });
                          clearError('categoryId');
                        }}
                        disabled={categories.length === 0}
                      >
                        <SelectTrigger className={fieldInvalidClass(errors.categoryId)}><SelectValue placeholder="Select category" /></SelectTrigger>
                        <SelectContent>{categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.title}</SelectItem>)}</SelectContent>
                      </Select>
                    </FormField>
                    <FormField label="Date" error={errors.date} required>
                      <Input
                        type="date"
                        className={fieldInvalidClass(errors.date)}
                        value={form.date}
                        onChange={(e) => {
                          setForm({ ...form, date: e.target.value });
                          clearError('date');
                        }}
                      />
                    </FormField>
                    <FormField label="Amount" error={errors.amount} required>
                      <Input
                        type="number"
                        min="0"
                        step="0.01"
                        className={fieldInvalidClass(errors.amount)}
                        value={form.amount}
                        onChange={(e) => {
                          setForm({ ...form, amount: e.target.value });
                          clearError('amount');
                        }}
                      />
                    </FormField>
                    <FormField label="Kind">
                      <Select value={form.expenseKind} onValueChange={(v) => setForm({ ...form, expenseKind: v as ExpenseKind })}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>{expenseKindOptions.map((o) => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}</SelectContent>
                      </Select>
                    </FormField>
                    <FormField label="Payment">
                      <Select value={form.paymentMethod} onValueChange={(v) => setForm({ ...form, paymentMethod: v })}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>{paymentMethods.map((m) => <SelectItem key={m} value={m}>{m}</SelectItem>)}</SelectContent>
                      </Select>
                    </FormField>
                    <div className="sm:col-span-2">
                      <Button type="submit" disabled={categories.length === 0}>Save expense</Button>
                    </div>
                  </form>
                </CardContent>
              </Card>

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
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Date</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>Kind</TableHead>
                        <TableHead>Payment</TableHead>
                        <TableHead>Amount</TableHead>
                        <TableHead />
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredExpenses.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={6}>
                            <EmptyState
                              icon={Receipt}
                              title="No expenses yet"
                              description={categories.length === 0
                                ? 'Add a category in the sidebar, then record your first expense here.'
                                : 'Log rent, food, transport and more — analytics will show where your money goes.'}
                              className="border-0 bg-transparent py-8"
                            />
                          </TableCell>
                        </TableRow>
                      ) : filteredExpenses.map((expense) => (
                        <TableRow key={expense.id}>
                          <TableCell className="font-medium whitespace-nowrap">{formatExpenseDate(expense.date)}</TableCell>
                          <TableCell>{expense.categoryTitle}</TableCell>
                          <TableCell><Badge variant="outline">{expenseKindLabel(expense.expenseKind)}</Badge></TableCell>
                          <TableCell className="text-muted-foreground">{expense.paymentMethod}</TableCell>
                          <TableCell className="font-medium">{money(expense.amount)}</TableCell>
                          <TableCell>
                            <Button variant="ghost" size="icon" onClick={() => void deleteExpense(expense.id).then(load)}>
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2"><Calculator className="h-4 w-4" /> Budget calculator</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <FormField label="Monthly income">
                      <Input type="number" min="0" value={monthlyIncome} onChange={(e) => setMonthlyIncome(e.target.value)} />
                    </FormField>
                  </div>
                  <Button variant="secondary" onClick={() => void calculateExpenses(Number(monthlyIncome), expenses.map((e) => ({
                    categoryId: e.categoryId, amount: e.amount, date: e.date.slice(0, 10),
                    expenseKind: typeof e.expenseKind === 'number' ? expenseKindOptions[e.expenseKind].value : e.expenseKind,
                    paymentMethod: e.paymentMethod,
                  }))).then(setCalculatorResult)}>Calculate</Button>
                  {calculatorResult && (
                    <div className="grid gap-3 rounded-lg border p-4 text-sm">
                      <div className="flex justify-between"><span>Total expenses</span><strong>{money(calculatorResult.totalExpenses)}</strong></div>
                      <div className="flex justify-between"><span>Remaining</span><strong>{money(calculatorResult.remainingBalance)}</strong></div>
                      <div className="flex justify-between"><span>Savings rate</span><strong>{calculatorResult.savingsRatePercent}%</strong></div>
                    </div>
                  )}
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="categories">
          <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
            <CategorySidebar
              categories={categories}
              summary={summary}
              selectedCategoryId={selectedCategoryId}
              onSelectCategory={setSelectedCategoryId}
              newCategory={newCategory}
              onNewCategoryChange={setNewCategory}
              onAddCategory={addCategory}
              showManage
            />
            <Card>
              <CardHeader>
                <CardTitle>Manage categories</CardTitle>
                <CardDescription>Create categories in the sidebar, then use them when adding expenses.</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2">
                  {categories.length === 0 ? (
                    <EmptyState
                      icon={FolderOpen}
                      title="No categories"
                      description="Use the sidebar to add categories like Rent, Groceries, or Transport."
                      className="w-full border-0 bg-transparent py-6"
                    />
                  ) : categories.map((c) => (
                    <Badge key={c.id} variant="outline" className="text-sm py-1 px-3">{c.title}</Badge>
                  ))}
                </div>
                {summary && (
                  <div className="mt-6 space-y-2">
                    {Object.entries(summary.byCategory).map(([cat, amt]) => (
                      <StatBar key={cat} label={cat} value={amt} max={Math.max(...Object.values(summary.byCategory), 1)} display={money(amt)} />
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
