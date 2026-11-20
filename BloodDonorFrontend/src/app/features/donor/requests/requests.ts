import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DonorApi } from '../../../core/donor/donor-api.service';
import { ApiError } from '../../../core/http/api-error';
import { DonorOwnRequest } from '../../../core/models/donor.models';
import { DonorRequestStatus } from '../../../core/models/enums';
import { ToastService } from '../../../core/ui/toast.service';
import { ConfirmDialog } from '../../../shared/ui/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { errorText } from '../../../shared/utils/error-text';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { AcceptRequestDialog } from './accept-request-dialog/accept-request-dialog';
import { RequestCard } from './request-card/request-card';

@Component({
  selector: 'app-donor-requests-page',
  imports: [
    RouterLink,
    PageHeader,
    LoadingState,
    EmptyState,
    ErrorBanner,
    RequestCard,
    AcceptRequestDialog,
    ConfirmDialog,
  ],
  templateUrl: './requests.html',
  styleUrl: './requests.css',
})
export class DonorRequestsPage implements OnInit {
  private readonly api = inject(DonorApi);
  private readonly toast = inject(ToastService);

  protected readonly state = signal<LoadState<DonorOwnRequest[]>>(LOADING);
  protected readonly acceptTarget = signal<DonorOwnRequest | null>(null);
  protected readonly declineTarget = signal<DonorOwnRequest | null>(null);
  protected readonly declining = signal(false);

  protected readonly items = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly pending = computed(() =>
    this.items().filter((item) => item.status === DonorRequestStatus.Pending),
  );
  protected readonly accepted = computed(() =>
    this.items().filter((item) => item.status === DonorRequestStatus.Accepted),
  );

  protected readonly declineTitle = computed(() =>
    this.declineTarget()?.status === DonorRequestStatus.Accepted ? 'Withdraw your acceptance?' : 'Decline this request?',
  );
  protected readonly declineMessage = computed(() => {
    const target = this.declineTarget();
    if (!target) {
      return '';
    }

    return target.status === DonorRequestStatus.Accepted
      ? `${target.hospitalName} will be told you can no longer help, and the number you shared is removed.`
      : `${target.hospitalName} will be told you declined.`;
  });
  protected readonly declineLabel = computed(() =>
    this.declineTarget()?.status === DonorRequestStatus.Accepted ? 'Withdraw' : 'Decline',
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(showSpinner = true): void {
    if (showSpinner) {
      this.state.set(LOADING);
    }

    this.api.listRequests().subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected openAccept(request: DonorOwnRequest): void {
    this.acceptTarget.set(request);
  }

  protected closeAccept(): void {
    this.acceptTarget.set(null);
  }

  protected onAccepted(): void {
    this.acceptTarget.set(null);
    this.toast.success('Request accepted. The hospital has been notified.');
    this.load(false);
  }

  // The request changed on the server while the dialog was open.
  protected onStale(message: string): void {
    this.acceptTarget.set(null);
    this.toast.error(message);
    this.load(false);
  }

  protected openDecline(request: DonorOwnRequest): void {
    this.declineTarget.set(request);
  }

  protected closeDecline(): void {
    if (!this.declining()) {
      this.declineTarget.set(null);
    }
  }

  protected confirmDecline(): void {
    const target = this.declineTarget();
    if (!target || this.declining()) {
      return;
    }

    const withdrawing = target.status === DonorRequestStatus.Accepted;
    this.declining.set(true);

    this.api.declineRequest(target.donorRequestId).subscribe({
      next: () => {
        this.declining.set(false);
        this.declineTarget.set(null);
        this.toast.success(withdrawing ? 'Your acceptance was withdrawn.' : 'Request declined.');
        this.load(false);
      },
      error: (err: unknown) => {
        this.declining.set(false);
        this.declineTarget.set(null);
        this.toast.error(errorText(err));

        if (err instanceof ApiError && (err.status === 404 || err.status === 409)) {
          this.load(false);
        }
      },
    });
  }
}