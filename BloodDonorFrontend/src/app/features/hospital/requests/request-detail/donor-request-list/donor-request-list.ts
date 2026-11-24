import { Component, computed, input, output } from '@angular/core';
import { HospitalDonorRequest } from '../../../../../core/models/blood-request.models';
import { DonorRequestStatus } from '../../../../../core/models/enums';
import { RelativeTimePipe } from '../../../../../shared/pipes/relative-time';
import { BloodGroupChip } from '../../../../../shared/ui/blood-group-chip/blood-group-chip';
import { StatusBadge } from '../../../../../shared/ui/status-badge/status-badge';

// Accepted donors first, because they are the ones staff act on.
const RANK: Record<DonorRequestStatus, number> = { Accepted: 0, Pending: 1, Declined: 2, Cancelled: 3 };

// The donors this blood request was sent to. Donors stay anonymous here: only their blood group and area.
@Component({
  selector: 'app-donor-request-list',
  imports: [StatusBadge, BloodGroupChip, RelativeTimePipe],
  templateUrl: './donor-request-list.html',
  styleUrl: './donor-request-list.css',
})
export class DonorRequestList {
  readonly items = input.required<HospitalDonorRequest[]>();
  // False once the blood request is closed: contact details are then no longer available.
  readonly canAct = input<boolean>(false);

  readonly viewContact = output<HospitalDonorRequest>();
  readonly cancel = output<HospitalDonorRequest>();

  protected readonly Accepted = DonorRequestStatus.Accepted;
  protected readonly Pending = DonorRequestStatus.Pending;

  protected readonly sorted = computed(() =>
    [...this.items()].sort(
      (a, b) =>
        RANK[a.status] - RANK[b.status] || new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    ),
  );
}