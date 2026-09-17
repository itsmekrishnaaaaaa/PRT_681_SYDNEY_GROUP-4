export const nationalityOptions = ['Australian'] as const;

export type Nationality = (typeof nationalityOptions)[number];
