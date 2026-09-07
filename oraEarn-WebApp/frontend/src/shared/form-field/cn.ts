/** Minimal className helper — copy this folder as-is into any React + Tailwind project. */
export function cn(...classes: Array<string | false | null | undefined>) {
  return classes.filter(Boolean).join(' ');
}
