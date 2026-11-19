import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

// One page for 403, 404 and "built in a later step", driven by the route's data.
@Component({
  selector: 'app-status-page',
  imports: [RouterLink],
  templateUrl: './status-page.html',
  styleUrl: './status-page.css',
})
export class StatusPage {
  private readonly data = inject(ActivatedRoute).snapshot.data;

  protected readonly code = (this.data['code'] as string | undefined) ?? '';
  protected readonly title = (this.data['title'] as string | undefined) ?? 'Page';
  protected readonly message = (this.data['message'] as string | undefined) ?? '';
}