import { useState } from 'react';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { StatBar } from '@/shared/components/StatBar';
import { FormField, fieldInvalidClass, validators } from '@/shared/form-field';
import { money } from '@/shared/utils/dates';
import { Plus, Tag } from 'lucide-react';
import { Input } from '@/shared/ui/input';
import type { ExpenseSummary } from '@/shared/types';

type Category = { id: string; title: string };

interface CategorySidebarProps {
  categories: Category[];
  summary: ExpenseSummary | null;
  selectedCategoryId: string | null;
  onSelectCategory: (id: string | null) => void;
  newCategory: string;
  onNewCategoryChange: (v: string) => void;
  onAddCategory: () => void;
  showManage?: boolean;
}

export function CategorySidebar({
  categories,
  summary,
  selectedCategoryId,
  onSelectCategory,
  newCategory,
  onNewCategoryChange,
  onAddCategory,
  showManage = false,
}: CategorySidebarProps) {
  const [categoryError, setCategoryError] = useState<string>();
  const maxCategory = Math.max(...Object.values(summary?.byCategory ?? {}), 1);

  const handleAddCategory = () => {
    const error = validators.required(newCategory, 'Enter a category name.');
    setCategoryError(error);
    if (error) return;
    onAddCategory();
    setCategoryError(undefined);
  };

  return (
    <Card className="h-fit lg:sticky lg:top-4">
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <Tag className="h-4 w-4" /> Categories
        </CardTitle>
        <CardDescription>Filter expenses or manage categories</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {showManage && (
          <FormField label="New category" error={categoryError}>
            <div className="flex gap-2">
              <Input
                placeholder="e.g. Rent, Food"
                className={fieldInvalidClass(categoryError)}
                value={newCategory}
                onChange={(e) => {
                  onNewCategoryChange(e.target.value);
                  setCategoryError(undefined);
                }}
                onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddCategory())}
              />
              <Button type="button" variant="secondary" size="icon" onClick={handleAddCategory}>
                <Plus className="h-4 w-4" />
              </Button>
            </div>
          </FormField>
        )}

        <div className="space-y-1">
          <button
            type="button"
            onClick={() => onSelectCategory(null)}
            className={`w-full rounded-md px-3 py-2 text-left text-sm transition-colors ${selectedCategoryId === null ? 'bg-muted font-medium' : 'hover:bg-muted/60'}`}
          >
            All categories
          </button>
          {categories.map((c) => {
            const spent = summary?.byCategory[c.title] ?? 0;
            return (
              <button
                key={c.id}
                type="button"
                onClick={() => onSelectCategory(c.id)}
                className={`w-full rounded-md px-3 py-2 text-left text-sm transition-colors flex items-center justify-between gap-2 ${selectedCategoryId === c.id ? 'bg-muted font-medium' : 'hover:bg-muted/60'}`}
              >
                <span>{c.title}</span>
                <Badge variant="outline" className="font-normal">{money(spent)}</Badge>
              </button>
            );
          })}
        </div>

        {summary && Object.keys(summary.byCategory).length > 0 && (
          <div className="space-y-2 pt-2 border-t">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Spending breakdown</p>
            {Object.entries(summary.byCategory).map(([cat, amt]) => (
              <StatBar key={cat} label={cat} value={amt} max={maxCategory} display={money(amt)} />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
