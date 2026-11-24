import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminApi } from '../../../core/admin/admin-api.service';
import { HospitalStatus } from '../../../core/models/enums';
import { Hospital } from '../../../core/models/hospital.models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../shared/utils/dates';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';

type StatusFilter = 'All' | HospitalStatus;

@Component({
  selector: 'app-admin-hospital-list',
  imports: [RouterLink, PageHeader, LoadingState, EmptyState, ErrorBanner, StatusBadge],
  templateUrl: './hospital-list.html',
  styleUrl: './hospital-list.css',
})
export class AdminHospitalList implements OnInit {
  private readonly api = inject(AdminApi);

  protected readonly filters: StatusFilter[] = ['All', ...Object.values(HospitalStatus)];
  protected readonly formatDateTime = formatDateTime;

  protected readonly state = signal<LoadState<Hospital[]>>(LOADING);
  protected readonly filter = signal<StatusFilter>('All');
  protected readonly query = signal('');

  protected readonly all = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });

  protected readonly counts = computed(() => {
    const counts: Record<StatusFilter, number> = { All: this.all().length, Pending: 0, Active: 0, Suspended: 0 };
    for (const hospital of this.all()) {
      counts[hospital.status] += 1;
    }
    return counts;
  });

  protected readonly visible = computed(() => {
    const filter = this.filter();
    const query = this.query().trim().toLowerCase();

    return this.all().filter(
      (hospital) =>
        (filter === 'All' || hospital.status === filter) &&
        (!query ||
          hospital.name.toLowerCase().includes(query) ||
          hospital.registrationNumber.toLowerCase().includes(query) ||
          hospital.city.toLowerCase().includes(query)),
    );
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);

    this.api.listHospitals().subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected onQuery(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
}