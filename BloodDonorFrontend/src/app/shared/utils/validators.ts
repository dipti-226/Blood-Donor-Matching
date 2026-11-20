import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { todayIso } from './date-input';
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

// A value made only of spaces counts as empty (the API rejects it too).
export const notBlank: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0 ? { required: true } : null;
};

// Mirrors the backend phone rule: 10 to 15 digits with an optional leading +. Format only, not verified.
export const phoneNumber: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = ((control.value ?? '') as string).trim();
  return !value || /^\+?[0-9]{10,15}$/.test(value) ? null : { phone: true };
};

// For a date input ("yyyy-MM-dd"): not after today, not before 1900.
export const pastDate: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (!value) {
    return null;
  }
  if (value > todayIso()) {
    return { futureDate: true };
  }
  return value < '1900-01-01' ? { date: true } : null;
};
// For number inputs: whole numbers only (an empty field is left to the required validator).
export const wholeNumber: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return value === null || value === undefined || value === '' || Number.isInteger(Number(value))
    ? null
    : { whole: true };
};