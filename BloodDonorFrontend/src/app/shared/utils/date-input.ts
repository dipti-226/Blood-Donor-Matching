// Helpers for date-only values ("yyyy-MM-dd"), such as a date of birth. No time zone is involved.

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

// Today in the browser's local time, in the format a date input uses.
export function todayIso(): string {
  const now = new Date();
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

export function formatDateOnly(value: string | null | undefined): string {
  const match = value ? /^(\d{4})-(\d{2})-(\d{2})/.exec(value) : null;
  if (!match) {
    return '-';
  }

  return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])).toLocaleDateString('en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}