import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DonorApi } from '../../../core/donor/donor-api.service';
import { ApiError } from '../../../core/http/api-error';
import { DonorBloodGroupStatus } from '../../../core/models/donor.models';
import { BloodGroup, VerificationStatus } from '../../../core/models/enums';
import { BLOOD_GROUP_LABELS, BLOOD_GROUP_OPTIONS } from '../../../core/models/labels';
import { ToastService } from '../../../core/ui/toast.service';
import { BloodGroupChip } from '../../../shared/ui/blood-group-chip/blood-group-chip';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../shared/utils/dates';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';

@Component({
  selector: 'app-donor-blood-group-page',
  imports: [ReactiveFormsModule, PageHeader, LoadingState, ErrorBanner, FormField, StatusBadge, BloodGroupChip],
  templateUrl: './blood-group.html',
  styleUrl: './blood-group.css',
})
export class DonorBloodGroupPage implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(DonorApi);
  private readonly toast = inject(ToastService);

  protected readonly options = BLOOD_GROUP_OPTIONS;
  protected readonly labels = BLOOD_GROUP_LABELS;
  protected readonly formatDateTime = formatDateTime;

  protected readonly form = this.fb.group({
    bloodGroup: ['' as BloodGroup | '', [Validators.required]],
  });

  // Null data means no blood group has been declared yet.
  protected readonly state = signal<LoadState<DonorBloodGroupStatus | null>>(LOADING);
  protected readonly busy = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly current = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly isPending = computed(() => this.current()?.status === VerificationStatus.Pending);
  protected readonly isVerified = computed(() => this.current()?.status === VerificationStatus.Verified);
  protected readonly isRejected = computed(() => this.current()?.status === VerificationStatus.Rejected);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);
    this.formError.set(null);
    this.form.reset({ bloodGroup: '' });

    this.api.getBloodGroup().subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected fieldError(): string | null {
    return errorMessage(this.form.controls.bloodGroup);
  }

  // Declares for the first time, or resubmits after a rejection.
  protected submit(): void {
    if (this.busy()) {
      return;
    }

    const group = this.form.controls.bloodGroup.value;
    if (this.form.invalid || group === '') {
      this.form.markAllAsTouched();
      return;
    }

    const resubmitting = this.isRejected();
    this.busy.set(true);
    this.formError.set(null);

    const request = resubmitting ? this.api.resubmitBloodGroup(group) : this.api.declareBloodGroup(group);

    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(
          resubmitting ? 'Your blood group was sent for verification again.' : 'Your blood group was sent for verification.',
        );
        this.load();
      },
      error: (err: unknown) => this.onError(err),
    });
  }

  private onError(err: unknown): void {
    this.busy.set(false);

    if (!(err instanceof ApiError)) {
      this.formError.set('Something went wrong. Please try again.');
      return;
    }

    // The record changed on the server (declared elsewhere, already reviewed): show why and reload.
    if (err.status === 404 || err.status === 409) {
      this.toast.error(err.message);
      this.load();
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.formError.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}