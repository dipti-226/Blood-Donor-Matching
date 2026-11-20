import { AbstractControl, FormGroup } from '@angular/forms';
import { ApiError } from '../../core/http/api-error';

// The message to show under a field, or null while it is fine or untouched.
export function errorMessage(control: AbstractControl | null | undefined): string | null {
  if (!control || !control.errors || !(control.touched || control.dirty)) {
    return null;
  }

  const errors = control.errors;

  if (errors['server']) {
    return errors['server'] as string;
  }
  if (errors['required']) {
    return 'This field is required.';
  }
  if (errors['email']) {
    return 'Enter a valid email address.';
  }
  if (errors['passwordPolicy']) {
    return 'Use at least 6 characters with an upper case letter, a lower case letter, a digit and a symbol.';
  }
  if (errors['mismatch']) {
    return 'The passwords do not match.';
  }
  if (errors['minlength']) {
    return `Enter at least ${errors['minlength'].requiredLength} characters.`;
  }
  if (errors['maxlength']) {
    return `Enter at most ${errors['maxlength'].requiredLength} characters.`;
  }
    if (errors['whole']) {
    return 'Enter a whole number.';
  }
  if (errors['min']) {
    return `Enter ${errors['min'].min} or more.`;
  }
  if (errors['max']) {
    return `Enter ${errors['max'].max} or less.`;
  }
  if (errors['phone']) {
    return 'Enter 10 to 15 digits, with an optional leading +.';
  }
  if (errors['futureDate']) {
    return 'This date cannot be in the future.';
  }
  return 'Enter a valid value.';
}

// Puts the API's field errors onto the matching controls (names compared ignoring case).
// Returns the messages that matched no control, so the page can show them in a banner.
export function applyServerErrors(form: FormGroup, error: ApiError): string[] {
  const unmatched: string[] = [];

  for (const [key, messages] of Object.entries(error.fieldErrors)) {
    const cleanKey = key.replace(/^\$\./, '').toLowerCase();
    const name = Object.keys(form.controls).find((controlName) => controlName.toLowerCase() === cleanKey);
    const control = name ? form.get(name) : null;

    if (control) {
      control.setErrors({ server: messages[0] });
      control.markAsTouched();
    } else {
      unmatched.push(...messages);
    }
  }

  return unmatched;
}