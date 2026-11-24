import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Observable, map, of, switchMap } from 'rxjs';
import { HospitalContextService } from '../../../core/auth/hospital-context.service';
import { HospitalApi } from '../../../core/hospital/hospital-api.service';
import { HospitalMember } from '../../../core/models/hospital.models';
import { ToastService } from '../../../core/ui/toast.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { formatDateTime } from '../../../shared/utils/dates';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { AddStaffDialog } from './add-staff-dialog/add-staff-dialog';

interface StaffView {
  hospitalId: string;
  members: HospitalMember[];
}

@Component({
  selector: 'app-staff-page',
  imports: [PageHeader, LoadingState, EmptyState, ErrorBanner, AddStaffDialog],
  templateUrl: './staff.html',
  styleUrl: './staff.css',
})
export class StaffPage implements OnInit {
  private readonly api = inject(HospitalApi);
  private readonly hospitalContext = inject(HospitalContextService);
  private readonly toast = inject(ToastService);

  protected readonly formatDateTime = formatDateTime;

  protected readonly state = signal<LoadState<StaffView>>(LOADING);
  protected readonly addOpen = signal(false);

  protected readonly view = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
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

    this.loadView().subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected onCreated(): void {
    this.addOpen.set(false);
    this.toast.success('Staff member added. Share the temporary password with them securely.');
    this.load(false);
  }

  private loadView(): Observable<StaffView> {
    const existing = this.hospitalContext.hospital();
    const hospital$ = existing ? of(existing) : this.hospitalContext.load();

    return hospital$.pipe(
      switchMap((hospital) =>
        this.api.listStaff(hospital.id).pipe(map((members): StaffView => ({ hospitalId: hospital.id, members }))),
      ),
    );
  }
}