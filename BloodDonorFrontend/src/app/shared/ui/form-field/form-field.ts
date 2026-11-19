import { Component, input } from '@angular/core';

@Component({
  selector: 'app-form-field',
  templateUrl: './form-field.html',
  styleUrl: './form-field.css',
})
export class FormField {
  readonly label = input.required<string>();
  readonly controlId = input.required<string>();
  readonly hint = input<string>('');
  readonly error = input<string | null>(null);
}