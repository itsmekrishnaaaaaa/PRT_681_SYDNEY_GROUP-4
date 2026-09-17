import { Input } from '@/shared/ui/input';
import { cn } from '@/shared/lib/utils';
import {
  AU_PHONE_PREFIX,
  formatAustralianPhoneDisplay,
  sanitiseAustralianLocalDigits,
} from '@/auth/utils/australianPhone';

type Props = {
  value: string;
  onChange: (value: string) => void;
  error?: string;
};

export function AustralianPhoneInput({ value, onChange, error }: Props) {
  const localPart = value.startsWith(AU_PHONE_PREFIX)
    ? value.slice(AU_PHONE_PREFIX.length).replace(/\D/g, '')
    : value.replace(/\D/g, '');

  const handleChange = (raw: string) => {
    const digits = sanitiseAustralianLocalDigits(raw);
    onChange(digits ? `${AU_PHONE_PREFIX}${digits}` : AU_PHONE_PREFIX);
  };

  const displayNational = localPart.startsWith('0')
    ? sanitiseAustralianLocalDigits(localPart)
    : localPart;

  const digitHint = displayNational.length > 0
    ? `${displayNational.length}/9 digits (no leading 0)`
    : 'Mobile without leading 0 — e.g. 450 321 630';

  return (
    <div className="space-y-1">
      <div className={cn(
        'flex rounded-md border bg-background overflow-hidden focus-within:ring-2',
        error ? 'border-destructive focus-within:ring-destructive/30' : 'focus-within:ring-ring',
      )}>
        <span className="inline-flex items-center gap-1.5 border-r bg-muted/50 px-3 text-sm font-medium shrink-0">
          <span aria-hidden className="text-base leading-none">🇦🇺</span>
          {AU_PHONE_PREFIX}
        </span>
        <Input
          type="tel"
          className="border-0 shadow-none focus-visible:ring-0 rounded-none"
          placeholder="450 321 630"
          value={displayNational ? formatAustralianPhoneDisplay(`${AU_PHONE_PREFIX}${displayNational}`).replace('+61 ', '') : ''}
          onChange={(e) => handleChange(e.target.value)}
          inputMode="numeric"
          autoComplete="tel-national"
        />
      </div>
      <p className={cn('text-xs', error ? 'text-destructive font-medium' : 'text-muted-foreground')}>
        {error ?? digitHint}
      </p>
    </div>
  );
}
