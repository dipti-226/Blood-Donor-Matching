import { Component, computed, inject } from '@angular/core';
import { HospitalContextService } from '../../core/auth/hospital-context.service';
import { HospitalStatus } from '../../core/models/enums';

// Shown above every hospital screen while the hospital is not Active: the API refuses
// requests, donor matching and reviews for such a hospital, so the screens would only show errors.
@Component({
  selector: 'app-hospital-status-notice',
  templateUrl: './hospital-status-notice.html',
  styleUrl: './hospital-status-notice.css',
})
export class HospitalStatusNotice {
  private readonly hospitalContext = inject(HospitalContextService);

  protected readonly message = computed(() => {
    const hospital = this.hospitalContext.hospital();

    if (!hospital || hospital.status === HospitalStatus.Active) {
      return null;
    }

    return hospital.status === HospitalStatus.Pending
      ? `${hospital.name} is waiting to be activated. Blood requests, donor matching and reviews are unavailable until a system administrator activates it.`
      : `${hospital.name} is suspended. Blood requests, donor matching and reviews are unavailable until a system administrator reactivates it.`;
  });
}