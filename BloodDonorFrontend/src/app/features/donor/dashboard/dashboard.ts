import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DonorApi } from '../../../core/donor/donor-api.service';
import { DonorAvailability, DonorBloodGroupStatus, DonorOwnRequest } from '../../../core/models/donor.models';
import { DonorRequestStatus, VerificationStatus } from '../../../core/models/enums';
import { BloodGroupChip } from '../../../shared/ui/blood-group-chip/blood-group-chip';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatCard } from '../../../shared/ui/stat-card/stat-card';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { AvailabilityCard } from '../availability-card/availability-card';

interface Overview {
  // Null means the donor has no profile yet.
  availability: DonorAvailability | null;
  // Null means no blood group has been declared yet.
  bloodGroup: DonorBloodGroupStatus | null;
  requests: DonorOwnRequest[];
}

interface ChecklistStep {
  label: string;
  done: boolean;
  detail: string;
  link: string | null;
  linkLabel: string;
}

@Component({
  selector: 'app-donor-dashboard',
  imports: [
    RouterLink,
    PageHeader,
    LoadingState,
    ErrorBanner,
    StatCard,
    StatusBadge,
    BloodGroupChip,
    AvailabilityCard,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DonorDashboard implements OnInit {
  private readonly api = inject(DonorApi);

  protected readonly state = signal<LoadState<Overview>>(LOADING);

  protected readonly overview = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });

  protected readonly hasProfile = computed(() => (this.overview()?.availability ?? null) !== null);
  protected readonly isAvailable = computed(() => this.overview()?.availability?.isAvailable ?? false);
  protected readonly bloodGroup = computed(() => this.overview()?.bloodGroup ?? null);

  protected readonly pendingCount = computed(
    () => this.overview()?.requests.filter((r) => r.status === DonorRequestStatus.Pending).length ?? 0,
  );
  protected readonly acceptedCount = computed(
    () => this.overview()?.requests.filter((r) => r.status === DonorRequestStatus.Accepted).length ?? 0,
  );

  protected readonly steps = computed<ChecklistStep[]>(() => {
    const bloodGroup = this.bloodGroup();
    const status = bloodGroup?.status ?? null;

    let verifyDetail = 'Declare your blood group first.';
    if (status === VerificationStatus.Pending) {
      verifyDetail = 'Waiting for a hospital reviewer to confirm it.';
    } else if (status === VerificationStatus.Rejected) {
      verifyDetail = 'Your declaration was rejected. Submit it again.';
    } else if (status === VerificationStatus.Verified) {
      verifyDetail = '';
    }

    return [
      {
        label: 'Create your profile',
        done: this.hasProfile(),
        detail: this.hasProfile() ? '' : 'Hospitals find donors by city.',
        link: this.hasProfile() ? null : '/donor/profile',
        linkLabel: 'Create profile',
      },
      {
        label: 'Declare your blood group',
        done: bloodGroup !== null,
        detail: '',
        link: bloodGroup === null ? '/donor/blood-group' : null,
        linkLabel: 'Declare',
      },
      {
        label: 'Get your blood group verified',
        done: status === VerificationStatus.Verified,
        detail: verifyDetail,
        link: status === VerificationStatus.Rejected ? '/donor/blood-group' : null,
        linkLabel: 'Submit again',
      },
      {
        label: 'Switch on availability',
        done: this.isAvailable(),
        detail: this.isAvailable() ? '' : 'Use the switch below once your profile exists.',
        link: null,
        linkLabel: '',
      },
    ];
  });

  protected readonly ready = computed(() => this.steps().every((step) => step.done));

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);

    forkJoin({
      availability: this.api.getAvailability(),
      bloodGroup: this.api.getBloodGroup(),
      requests: this.api.listRequests(),
    }).subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected setAvailable(isAvailable: boolean): void {
    const state = this.state();
    if (state.status === 'ready') {
      this.state.set({
        status: 'ready',
        data: { ...state.data, availability: { isAvailable } },
      });
    }
  }
}