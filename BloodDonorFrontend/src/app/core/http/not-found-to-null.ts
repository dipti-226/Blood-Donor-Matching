import { Observable, OperatorFunction, catchError, of, throwError } from 'rxjs';
import { ApiError } from './api-error';

// For "does this exist yet?" calls: a 404 becomes null, every other error passes through.
export function notFoundToNull<T>(): OperatorFunction<T, T | null> {
  return (source: Observable<T>) =>
    source.pipe(
      catchError((error: unknown) =>
        error instanceof ApiError && error.status === 404 ? of(null) : throwError(() => error),
      ),
    );
}