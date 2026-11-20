const HAS_ZONE = /(Z|[+-]\d{2}:?\d{2})$/;

// The API stores UTC, but SQL Server hands dates back without the "Z",
// so a date with no zone is treated as UTC. Otherwise times would be off by the local offset.
export function parseUtc(value: string): Date {
  return new Date(HAS_ZONE.test(value) ? value : `${value}Z`);
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '-';
  }

  return parseUtc(value).toLocaleString('en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  });
}

export function relativeTime(value: string, now: Date = new Date()): string {
  const seconds = Math.round((now.getTime() - parseUtc(value).getTime()) / 1000);

  if (seconds < 45) {
    return 'just now';
  }

  const minutes = Math.round(seconds / 60);
  if (minutes < 60) {
    return `${minutes} min ago`;
  }

  const hours = Math.round(minutes / 60);
  if (hours < 24) {
    return `${hours} h ago`;
  }

  const days = Math.round(hours / 24);
  if (days < 7) {
    return `${days} d ago`;
  }

  return formatDateTime(value);
}