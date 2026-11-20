// The message to show for a failed call. ApiError messages are already written for people.
export function errorText(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong. Please try again.';
}