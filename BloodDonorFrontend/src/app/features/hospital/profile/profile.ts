import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HospitalContextService } from '../../../core/auth/hospital-context.service';
import { SessionService } from '../../../core/auth/session.service';
import { HospitalApi } from '../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../core/http/api-error';
import { Roles } from '../../../core/models/enums';
import { Hospital } from '../../../core/models/hospital.models';
import { ToastService } from '../../../core/ui/toast.service';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { StatusBadge } from '../../../shared/ui/status-badge/status-badge';
import { formatDateTime } from '../../../shared/utils/dates';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { notBlank } from '../../../shared/utils/validators';

type HospitalField = 'name' | 'address' | 'city' | 'area' | 'pincode' | 'contactNumber' | 'email';

@Component({
  selector: 'app-hospital-profile-page',
  imports: [ReactiveFormsModule, PageHeader, LoadingState, ErrorBanner, FormField, StatusBadge],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class HospitalProfilePage implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);
  private readonly hospitalContext = inject(HospitalContextService);
  private readonly toast = inject(ToastService);
  private readonly session = inject(SessionService);

  protected readonly formatDateTime = formatDateTime;
  // Only a hospital admin can change the details. Staff see them read-only.
  protected readonly canEdit = computed(() => this.session.hasRole(Roles.HospitalAdmin));

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, notBlank, Validators.maxLength(200)]],
    address: ['', [Validators.required, notBlank, Validators.maxLength(500)]],
    city: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    area: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    pincode: ['', [Validators.required, notBlank, Validators.maxLength(10)]],
    contactNumber: ['', [Validators.required, notBlank, Validators.maxLength(20)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  protected readonly state = signal<LoadState<Hospital>>(LOADING);
  protected readonly busy = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly hospital = computed(() => {
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

  protected load(): void {
    this.state.set(LOADING);
    this.formError.set(null);

    // Going through the context keeps the hospital name in the top bar current.
    this.hospitalContext.load().subscribe({
      next: (data) => {
        this.fillForm(data);
        this.state.set({ status: 'ready', data });
      },
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected fieldError(name: HospitalField): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    const hospital = this.hospital();
    if (!hospital || this.busy()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    this.busy.set(true);
    this.formError.set(null);

    this.api
      .updateHospital(hospital.id, {
        name: raw.name.trim(),
        address: raw.address.trim(),
        city: raw.city.trim(),
        area: raw.area.trim(),
        pincode: raw.pincode.trim(),
        contactNumber: raw.contactNumber.trim(),
        email: raw.email.trim(),
      })
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.success('Hospital details saved.');
          this.load();
        },
        error: (err: unknown) => this.onError(err),
      });
  }

  private fillForm(hospital: Hospital): void {
    this.form.reset({
      name: hospital.name,
      address: hospital.address,
      city: hospital.city,
      area: hospital.area,
      pincode: hospital.pincode,
      contactNumber: hospital.contactNumber,
      email: hospital.email,
    });
  }

  private onError(err: unknown): void {
    this.busy.set(false);

    if (!(err instanceof ApiError)) {
      this.formError.set('Something went wrong. Please try again.');
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.formError.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}