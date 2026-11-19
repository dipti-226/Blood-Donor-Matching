import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Mirrors the backend password rule: 6+ characters with upper, lower, digit and a symbol.
export const passwordPolicy: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (!value) {
    return null;
  }

  const valid =
    value.length >= 6 &&
    /[a-z]/.test(value) &&
    /[A-Z]/.test(value) &&
    /[0-9]/.test(value) &&
    /[^a-zA-Z0-9]/.test(value);

  return valid ? null : { passwordPolicy: true };
};

// For the confirm-password control: must equal the sibling "password" control.
export const matchesPassword: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.parent?.get('password')?.value;
  return control.value === password ? null : { mismatch: true };
};