import { Component, computed, input } from '@angular/core';
import { badgeClass } from '../../../core/models/labels';

// Always shows the status as text. Colour is only an extra cue.
@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.css',
})
export class StatusBadge {
  readonly value = input.required<string>();
  readonly label = input<string>('');

  protected readonly cssClass = computed(() => badgeClass(this.value()));
  protected readonly text = computed(() => this.label() || this.value());
}