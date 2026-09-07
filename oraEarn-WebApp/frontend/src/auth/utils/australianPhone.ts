/** Normalise to +61XXXXXXXXX (no spaces). Accepts 04XX local or 4XX international. */
export function normaliseAustralianPhone(input: string): string {
  let digits = input.replace(/\D/g, '');

  if (digits.startsWith('61')) {
    digits = digits.slice(2);
  } else if (digits.startsWith('0')) {
    digits = digits.slice(1);
  }

  // Fix pasted values like +610450… where 0 belongs to local dialling prefix
  while (digits.startsWith('0')) {
    digits = digits.slice(1);
  }

  return `+61${digits}`;
}

/** National number without +61 — max 9 digits, no leading 0. */
export function sanitiseAustralianLocalDigits(raw: string): string {
  let digits = raw.replace(/\D/g, '');
  if (digits.startsWith('0')) {
    digits = digits.slice(1);
  }
  return digits.slice(0, 9);
}

/** Australian numbers: +61 followed by 9 digits (mobile 4, landline 2/3/7/8). */
export function isValidAustralianPhone(input: string): boolean {
  const normalised = normaliseAustralianPhone(input);
  return /^\+61[23478]\d{8}$/.test(normalised);
}

export function australianPhoneValidationError(input: string): string | undefined {
  const national = normaliseAustralianPhone(input).replace('+61', '');
  if (!national) {
    return 'Enter your mobile number (9 digits after +61).';
  }
  if (national.length < 9) {
    return `Mobile needs 9 digits after +61 (${national.length}/9). Drop the leading 0 — e.g. 450 321 630 not 0450 321 630.`;
  }
  if (national.length > 9) {
    return 'Too many digits — enter 9 digits after +61 only.';
  }
  if (!/^[23478]/.test(national)) {
    return 'Australian numbers start with 2, 3, 4, 7, or 8 after +61 (mobiles usually start with 4).';
  }
  return undefined;
}

export function formatAustralianPhoneDisplay(input: string): string {
  const n = normaliseAustralianPhone(input).replace('+61', '');
  if (n.length <= 3) return n;
  if (n.length <= 6) return `${n.slice(0, 3)} ${n.slice(3)}`;
  return `${n.slice(0, 3)} ${n.slice(3, 6)} ${n.slice(6, 9)}`;
}

export const AU_PHONE_PREFIX = '+61';
