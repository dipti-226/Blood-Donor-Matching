import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-error-banner',
  templateUrl: './error-banner.html',
  styleUrl: './error-banner.css',
})
export class ErrorBanner {
  readonly message = input.required<string>();
  readonly showRetry = input<boolean>(true);
  readonly retry = output<void>();
}