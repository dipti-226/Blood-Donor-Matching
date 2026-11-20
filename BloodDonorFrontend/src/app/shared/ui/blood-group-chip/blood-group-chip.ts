import { Component, computed, input } from '@angular/core';
import { BloodGroup } from '../../../core/models/enums';
import { BLOOD_GROUP_LABELS } from '../../../core/models/labels';

@Component({
  selector: 'app-blood-group-chip',
  templateUrl: './blood-group-chip.html',
  styleUrl: './blood-group-chip.css',
})
export class BloodGroupChip {
  readonly group = input<BloodGroup | null>(null);

  protected readonly text = computed(() => {
    const group = this.group();
    return group ? BLOOD_GROUP_LABELS[group] : '-';
  });
}