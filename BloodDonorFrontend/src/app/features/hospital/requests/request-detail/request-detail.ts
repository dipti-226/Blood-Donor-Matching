import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { BloodRequest, DonorMatchList, HospitalDonorRequest } from '../../../../core/models/blood-request.models';
import { BloodRequestStatus, UrgencyLevel } from '../../../../core/models/enums';
import { ToastService } from '../../../../core/ui/toast.service';
import { ComponentLabelPipe } from '../../../../shared/pipes/component-label';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time';
import { BloodGroupChip } from '../../../../shared/ui/blood-group-chip/blood-group-chip';
import { ConfirmDialog } from '../../../../shared/ui/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../../shared/utils/dates';
import { errorText } from '../../../../shared/utils/error-text';
import { LOADING, LoadState, loadError } from '../../../../shared/utils/load-state';
import { CancelRequestDialog } from './cancel-request-dialog/cancel-request-dialog';
import { ContactDialog } from './contact-dialog/contact-dialog';
import { DonorRequestList } from './donor-request-list/donor-request-list';
import { MatchList } from './match-list/match-list';

@Component({
  selector: 'app-request-detail',
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
    ConfirmDialog,
    CancelRequestDialog,
    ContactDialog,
    DonorRequestList,
    MatchList,
  ],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.css',
})
export class RequestDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(HospitalApi);
  private readonly toast = inject(ToastService);

  protected readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly formatDateTime = formatDateTime;
  protected readonly Critical = UrgencyLevel.Critical;

  protected readonly request = signal<LoadState<BloodRequest>>(LOADING);
  protected readonly donorRequests = signal<LoadState<HospitalDonorRequest[]>>(LOADING);
  protected readonly matches = signal<LoadState<DonorMatchList>>(LOADING);
  protected readonly notFound = signal(false);

  protected readonly fulfillOpen = signal(false);
  protected readonly fulfilling = signal(false);
  protected readonly cancelOpen = signal(false);
  protected readonly contactTarget = signal<HospitalDonorRequest | null>(null);
  protected readonly withdrawTarget = signal<HospitalDonorRequest | null>(null);
  protected readonly withdrawing = signal(false);

  protected readonly current = computed(() => {
    const state = this.request();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly isOpen = computed(() => this.current()?.status === BloodRequestStatus.Open);
  protected readonly isCancelled = computed(() => this.current()?.status === BloodRequestStatus.Cancelled);

  protected readonly requestError = computed(() => {
    const state = this.request();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly donorRequestError = computed(() => {
    const state = this.donorRequests();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly matchError = computed(() => {
    const state = this.matches();
    return state.status === 'error' ? state.message : '';
  });

  protected readonly donorRequestItems = computed(() => {
    const state = this.donorRequests();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly matchList = computed(() => {
    const state = this.matches();
    return state.status === 'ready' ? state.data : null;
  });

  // Every donor who already has a donor request, whatever its status: the API will not ask them twice.
  protected readonly askedReferences = computed<ReadonlySet<string>>(
    () => new Set(this.donorRequestItems().map((item) => item.donorReference)),
  );

  ngOnInit(): void {
    this.load();
  }

  // Loads the request, then its donor requests and, while it is open, the matching donors.
  protected load(showSpinner = true): void {
    if (showSpinner) {
      this.request.set(LOADING);
      this.notFound.set(false);
    }

    this.api.getBloodRequest(this.id).subscribe({
      next: (data) => {
        this.request.set({ status: 'ready', data });
        this.loadDonorRequests(showSpinner);

        if (data.status === BloodRequestStatus.Open) {
          this.loadMatches(showSpinner);
        }
      },
      error: (err: unknown) => {
        this.notFound.set(err instanceof ApiError && err.status === 404);
        this.request.set(loadError(err));
      },
    });
  }

  protected loadDonorRequests(showSpinner = true): void {
    if (showSpinner) {
      this.donorRequests.set(LOADING);
    }

    this.api.listDonorRequests(this.id).subscribe({
      next: (data) => this.donorRequests.set({ status: 'ready', data }),
      error: (err: unknown) => this.donorRequests.set(loadError(err)),
    });
  }

  protected loadMatches(showSpinner = true): void {
    if (showSpinner) {
      this.matches.set(LOADING);
    }

    this.api.getMatches(this.id).subscribe({
      next: (data) => this.matches.set({ status: 'ready', data }),
      error: (err: unknown) => this.matches.set(loadError(err)),
    });
  }

  // ---- Fulfil

  protected confirmFulfill(): void {
    if (this.fulfilling()) {
      return;
    }

    this.fulfilling.set(true);

    this.api.fulfillBloodRequest(this.id).subscribe({
      next: () => {
        this.fulfilling.set(false);
        this.fulfillOpen.set(false);
        this.toast.success('Request marked as fulfilled. Donors were told it is closed.');
        this.load(false);
      },
      error: (err: unknown) => {
        this.fulfilling.set(false);
        this.fulfillOpen.set(false);
        this.toast.error(errorText(err));
        this.reloadIfStale(err);
      },
    });
  }

  protected closeFulfill(): void {
    if (!this.fulfilling()) {
      this.fulfillOpen.set(false);
    }
  }

  // ---- Cancel the blood request

  protected onCancelled(): void {
    this.cancelOpen.set(false);
    this.toast.success('Request cancelled. Donors were told it is closed.');
    this.load(false);
  }

  protected onStale(message: string): void {
    this.cancelOpen.set(false);
    this.contactTarget.set(null);
    this.toast.error(message);
    this.load(false);
  }

  // ---- Cancel a request sent to one donor

  protected confirmWithdraw(): void {
    const target = this.withdrawTarget();
    if (!target || this.withdrawing()) {
      return;
    }

    this.withdrawing.set(true);

    this.api.cancelDonorRequest(this.id, target.id).subscribe({
      next: () => {
        this.withdrawing.set(false);
        this.withdrawTarget.set(null);
        this.toast.success('Request to the donor cancelled.');
        this.load(false);
      },
      error: (err: unknown) => {
        this.withdrawing.set(false);
        this.withdrawTarget.set(null);
        this.toast.error(errorText(err));
        this.reloadIfStale(err);
      },
    });
  }

  protected closeWithdraw(): void {
    if (!this.withdrawing()) {
      this.withdrawTarget.set(null);
    }
  }

  private reloadIfStale(err: unknown): void {
    if (err instanceof ApiError && (err.status === 404 || err.status === 409)) {
      this.load(false);
    }
  }
}