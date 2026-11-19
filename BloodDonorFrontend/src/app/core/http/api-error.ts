export class ApiError extends Error {
  readonly status: number;
  readonly fieldErrors: Record<string, string[]>;
  readonly retryAfterSeconds: number | null;

  constructor(
    status: number,
    message: string,
    fieldErrors: Record<string, string[]> = {},
    retryAfterSeconds: number | null = null,
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.fieldErrors = fieldErrors;
    this.retryAfterSeconds = retryAfterSeconds;
  }
}