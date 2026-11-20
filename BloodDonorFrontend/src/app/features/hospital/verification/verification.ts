import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HospitalApi } from '../../../core/hospital/hospital-api.service';
import { VerificationListItem } from '../../../core/models/blood-request.models';
import { VerificationStatus } from '../../../core/models/enums';
import { VERIFICATION_STATUS_LABELS } from '../../../core/models/labels';
import { ToastService } from '../../../core/ui/toast.service';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time';
import { BloodGroupChip } from '../../../shared/ui/blood-group-chip/blood-group-chip';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../shared/utils/dates';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { ApproveDialog } from './approve-dialog/approve-dialog';
import { RejectDialog } from './reject-dialog/reject-dialog';

@Component({
  selector: 'app-verification',
  imports: [
    PageHeader,
    LoadingState,
    EmptyState,
    ErrorBanner,
    StatusBadge,
    BloodGroupChip,
    RelativeTimePipe,
    ApproveDialog,
    RejectDialog,
  ],
  templateUrl: './verification.html',
  styleUrl: './verification.css',
})
export class Verification implements OnInit {
  private readonly api = inject(HospitalApi);
  private readonly toast = inject(ToastService);

  protected readonly tabs = Object.values(VerificationStatus);
  protected readonly tabLabels = VERIFICATION_STATUS_LABELS;
  protected readonly formatDateTime = formatDateTime;
  protected readonly Pending = VerificationStatus.Pending;
  protected readonly Rejected = VerificationStatus.Rejected;

  protected readonly status = signal<VerificationStatus>(VerificationStatus.Pending);
  protected readonly state = signal<LoadState<VerificationListItem[]>>(LOADING);
  protected readonly approveTarget = signal<VerificationListItem | null>(null);
  protected readonly rejectTarget = signal<VerificationListItem | null>(null);

  protected readonly items = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(showSpinner = true): void {
    if (showSpinner) {
      this.state.set(LOADING);
    }

    this.api.listVerifications(this.status()).subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected setStatus(status: VerificationStatus): void {
    if (this.status() === status) {
      return;
    }

    this.status.set(status);
    this.load();
  }

  protected onApproved(): void {
    this.approveTarget.set(null);
    this.toast.success('Blood group approved.');
    this.load(false);
  }

  protected onRejected(): void {
    this.rejectTarget.set(null);
    this.toast.success('Blood group rejected.');
    this.load(false);
  }

  // Someone else decided it first, or it no longer exists.
  protected onStale(message: string): void {
    this.approveTarget.set(null);
    this.rejectTarget.set(null);
    this.toast.error(message);
    this.load(false);
  }
}