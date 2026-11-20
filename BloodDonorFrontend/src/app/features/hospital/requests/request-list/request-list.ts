import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { BloodRequest } from '../../../../core/models/blood-request.models';
import { BloodRequestStatus } from '../../../../core/models/enums';
import { BLOOD_REQUEST_STATUS_LABELS } from '../../../../core/models/labels';
import { ComponentLabelPipe } from '../../../../shared/pipes/component-label';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time';
import { BloodGroupChip } from '../../../../shared/ui/blood-group-chip/blood-group-chip';
import { EmptyState } from '../../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../../shared/utils/dates';
import { LOADING, LoadState, loadError } from '../../../../shared/utils/load-state';

@Component({
  selector: 'app-request-list',
  imports: [
    RouterLink,
    PageHeader,
    LoadingState,
    EmptyState,
    ErrorBanner,
    StatusBadge,
    BloodGroupChip,
    ComponentLabelPipe,
    RelativeTimePipe,
  ],
  templateUrl: './request-list.html',
  styleUrl: './request-list.css',
})
export class RequestList implements OnInit {
  private readonly api = inject(HospitalApi);

  protected readonly tabs = Object.values(BloodRequestStatus);
  protected readonly tabLabels = BLOOD_REQUEST_STATUS_LABELS;
  protected readonly formatDateTime = formatDateTime;

  protected readonly status = signal<BloodRequestStatus>(BloodRequestStatus.Open);
  protected readonly state = signal<LoadState<BloodRequest[]>>(LOADING);

  protected readonly items = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly isOpenTab = computed(() => this.status() === BloodRequestStatus.Open);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);

    this.api.listBloodRequests(this.status()).subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected setStatus(status: BloodRequestStatus): void {
    if (this.status() === status) {
      return;
    }

    this.status.set(status);
    this.load();
  }
}