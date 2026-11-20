import { Component, input, output } from '@angular/core';
import { DonorOwnRequest } from '../../../../core/models/donor.models';
import { DonorRequestStatus, UrgencyLevel } from '../../../../core/models/enums';
import { BloodGroupChip } from '../../../../shared/ui/blood-group-chip/blood-group-chip';
import { ComponentLabelPipe } from '../../../../shared/pipes/component-label';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time';
import { StatusBadge } from '../../../../shared/ui/status-badge/status-badge';

// One request a hospital sent to the donor. It only reports what the donor clicks.
@Component({
  selector: 'app-request-card',
  imports: [StatusBadge, BloodGroupChip, ComponentLabelPipe, RelativeTimePipe],
  templateUrl: './request-card.html',
  styleUrl: './request-card.css',
})
export class RequestCard {
  readonly request = input.required<DonorOwnRequest>();

  readonly accept = output<DonorOwnRequest>();
  readonly decline = output<DonorOwnRequest>();

  protected readonly Pending = DonorRequestStatus.Pending;
  protected readonly Accepted = DonorRequestStatus.Accepted;
  protected readonly Critical = UrgencyLevel.Critical;
}