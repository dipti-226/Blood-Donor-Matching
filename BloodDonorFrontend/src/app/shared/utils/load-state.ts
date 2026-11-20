// The three states every list and detail screen shows: loading, ready (possibly empty) and error.
export type LoadState<T> =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: T };

export const LOADING: LoadState<never> = { status: 'loading' };

export function loadError(error: unknown): LoadState<never> {
  return {
    status: 'error',
    message: error instanceof Error ? error.message : 'Something went wrong. Please try again.',
  };
}