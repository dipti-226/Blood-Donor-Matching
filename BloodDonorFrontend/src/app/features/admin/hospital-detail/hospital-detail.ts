import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminApi } from '../../../core/admin/admin-api.service';
import { ApiError } from '../../../core/http/api-error';
import { HospitalStatus } from '../../../core/models/enums';
import { Hospital, HospitalMember } from '../../../core/models/hospital.models';
import { ToastService } from '../../../core/ui/toast.service';
import { ConfirmDialog } from '../../../shared/ui/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../shared/utils/dates';
import { errorText } from '../../../shared/utils/error-text';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { AddAdminDialog } from './add-admin-dialog/add-admin-dialog';

interface StatusAction {
  target: HospitalStatus;
  label: string;
  title: string;
  message: string;
  danger: boolean;
}

// One sensible next step per status. The API would accept any status, but these are the real situations.
const STATUS_ACTIONS: Record<HospitalStatus, StatusAction> = {
  Pending: {
    target: HospitalStatus.Active,
    label: 'Activate hospital',
    title: 'Activate this hospital?',
    message:
      'Its staff will be able to raise blood requests, ask donors and review blood groups. Make sure it has an admin so someone can sign in.',
    danger: false,
  },
  Active: {
    target: HospitalStatus.Suspended,
    label: 'Suspend hospital',
    title: 'Suspend this hospital?',
    message:
      'Its staff can still sign in, but they will not be able to raise blood requests, ask donors or review blood groups until you reactivate it.',
    danger: true,
  },
  Suspended: {
    target: HospitalStatus.Active,
    label: 'Reactivate hospital',
    title: 'Reactivate this hospital?',
    message: 'Its staff will be able to raise blood requests, ask donors and review blood groups again.',
    danger: false,
  },
};

@Component({
  selector: 'app-admin-hospital-detail',
  imports: [
    RouterLink,
    PageHeader,
    LoadingState,
    EmptyState,
    ErrorBanner,
    StatusBadge,
    ConfirmDialog,
    AddAdminDialog,
  ],
  templateUrl: './hospital-detail.html',
  styleUrl: './hospital-detail.css',
})
export class AdminHospitalDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AdminApi);
  private readonly toast = inject(ToastService);

  protected readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly formatDateTime = formatDateTime;

  protected readonly state = signal<LoadState<Hospital>>(LOADING);
  protected readonly admins = signal<LoadState<HospitalMember[]>>(LOADING);
  protected readonly notFound = signal(false);

  protected readonly statusOpen = signal(false);
  protected readonly changing = signal(false);
  protected readonly addOpen = signal(false);

  protected readonly hospital = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly adminError = computed(() => {
    const state = this.admins();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly adminList = computed(() => {
    const state = this.admins();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly action = computed(() => {
    const hospital = this.hospital();
    return hospital ? STATUS_ACTIONS[hospital.status] : null;
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(showSpinner = true): void {
    if (showSpinner) {
      this.state.set(LOADING);
      this.notFound.set(false);
    }

    this.api.getHospital(this.id).subscribe({
      next: (data) => {
        this.state.set({ status: 'ready', data });
        this.loadAdmins(showSpinner);
      },
      error: (err: unknown) => {
        this.notFound.set(err instanceof ApiError && err.status === 404);
        this.state.set(loadError(err));
      },
    });
  }

  protected loadAdmins(showSpinner = true): void {
    if (showSpinner) {
      this.admins.set(LOADING);
    }

    this.api.listAdmins(this.id).subscribe({
      next: (data) => this.admins.set({ status: 'ready', data }),
      error: (err: unknown) => this.admins.set(loadError(err)),
    });
  }

  protected confirmStatus(): void {
    const action = this.action();
    if (!action || this.changing()) {
      return;
    }

    this.changing.set(true);

    this.api.setHospitalStatus(this.id, action.target).subscribe({
      next: (updated) => {
        this.changing.set(false);
        this.statusOpen.set(false);
        this.state.set({ status: 'ready', data: updated });
        this.toast.success(`${updated.name} is now ${updated.status}.`);
      },
      error: (err: unknown) => {
        this.changing.set(false);
        this.statusOpen.set(false);
        this.toast.error(errorText(err));
        this.load(false);
      },
    });
  }

  protected closeStatus(): void {
    if (!this.changing()) {
      this.statusOpen.set(false);
    }
  }

  protected onAdminCreated(): void {
    this.addOpen.set(false);
    this.toast.success('Hospital admin created. Share the temporary password with them securely.');
    this.loadAdmins(false);
  }
}