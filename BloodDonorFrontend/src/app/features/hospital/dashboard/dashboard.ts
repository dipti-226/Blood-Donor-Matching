import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable, forkJoin, map, of, switchMap } from 'rxjs';
import { HospitalContextService } from '../../../core/auth/hospital-context.service';
import { HospitalApi } from '../../../core/hospital/hospital-api.service';
import { BloodRequest } from '../../../core/models/blood-request.models';
import { BloodRequestStatus, HospitalStatus, UrgencyLevel, VerificationStatus } from '../../../core/models/enums';
import { Hospital } from '../../../core/models/hospital.models';
import { UnreadCountService } from '../../../core/notifications/unread-count.service';
import { BloodGroupChip } from '../../../shared/ui/blood-group-chip/blood-group-chip';
import { ComponentLabelPipe } from '../../../shared/pipes/component-label';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatCard } from '../../../shared/ui/stat-card/stat-card';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';

interface Overview {
  hospital: Hospital;
  // False while the hospital is Pending or Suspended: the API would refuse the other calls.
  active: boolean;
  open: BloodRequest[];
  pendingVerifications: number;
}

const URGENCY_RANK: Record<UrgencyLevel, number> = { Critical: 3, Urgent: 2, Normal: 1 };
const RECENT_LIMIT = 5;

@Component({
  selector: 'app-hospital-dashboard',
  imports: [
    RouterLink,
    PageHeader,
    LoadingState,
    ErrorBanner,
    EmptyState,
    StatCard,
    StatusBadge,
    BloodGroupChip,
    ComponentLabelPipe,
    RelativeTimePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class HospitalDashboard implements OnInit {
  private readonly api = inject(HospitalApi);
  private readonly hospitalContext = inject(HospitalContextService);

  protected readonly unread = inject(UnreadCountService);
  protected readonly state = signal<LoadState<Overview>>(LOADING);

  protected readonly overview = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });

  protected readonly acceptedDonors = computed(
    () => this.overview()?.open.reduce((total, request) => total + request.acceptedCount, 0) ?? 0,
  );

  // Most urgent first, then newest first.
  protected readonly recent = computed(() =>
    [...(this.overview()?.open ?? [])]
      .sort(
        (a, b) =>
          URGENCY_RANK[b.urgency] - URGENCY_RANK[a.urgency] ||
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      )
      .slice(0, RECENT_LIMIT),
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);

    this.loadOverview().subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  private loadOverview(): Observable<Overview> {
    // The hospital is read first because its status decides whether the other calls can work.
    return this.hospitalContext.load().pipe(
      switchMap((hospital) => {
        if (hospital.status !== HospitalStatus.Active) {
          return of<Overview>({ hospital, active: false, open: [], pendingVerifications: 0 });
        }

        return forkJoin({
          open: this.api.listBloodRequests(BloodRequestStatus.Open),
          pending: this.api.listVerifications(VerificationStatus.Pending),
        }).pipe(
          map(
            ({ open, pending }): Overview => ({
              hospital,
              active: true,
              open,
              pendingVerifications: pending.length,
            }),
          ),
        );
      }),
    );
  }
}