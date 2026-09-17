import { type FormEvent, useEffect, useState } from 'react';
import {
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  pickErrors,
  useFormValidation,
  validators,
} from '@/shared/form-field';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Loader2 } from 'lucide-react';

type Fields = 'title' | '_form';

type CategoryModalProps = {
  open: boolean;
  saving: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (title: string) => Promise<void>;
};

export function CategoryModal({ open, saving, error, onClose, onSubmit }: CategoryModalProps) {
  const [title, setTitle] = useState('');
  const { errors, clearError, validate, setErrors, clearAll } = useFormValidation<Fields>();

  useEffect(() => {
    if (!open) return;
    setTitle('');
    clearAll();
  }, [open, clearAll]);

  useEffect(() => {
    if (open && error) setErrors((prev) => ({ ...prev, _form: error }));
  }, [error, open, setErrors]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!validate(() => pickErrors([['title', validators.required(title, 'Enter a category name.')]]))) return;
    await onSubmit(title.trim());
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && !saving && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add category</DialogTitle>
          <DialogDescription>e.g. Rent, Groceries, Transport — used when logging expenses.</DialogDescription>
        </DialogHeader>
        <FormErrorSummary errors={errors} />
        <form className="space-y-4" noValidate onSubmit={(e) => void handleSubmit(e)}>
          <FormField label="Category name" error={errors.title} required>
            <Input
              placeholder="Rent"
              disabled={saving}
              className={fieldInvalidClass(errors.title)}
              value={title}
              onChange={(e) => {
                setTitle(e.target.value);
                clearError('title');
              }}
            />
          </FormField>
          <div className="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" disabled={saving} onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              {saving ? 'Saving…' : 'Save category'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
