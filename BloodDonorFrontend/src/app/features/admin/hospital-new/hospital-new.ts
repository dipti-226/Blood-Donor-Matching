import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AdminApi } from '../../../core/admin/admin-api.service';
import { ApiError } from '../../../core/http/api-error';
import { ToastService } from '../../../core/ui/toast.service';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';
import { notBlank } from '../../../shared/utils/validators';

type HospitalField =
  | 'name'
  | 'registrationNumber'
  | 'address'
  | 'city'
  | 'area'
  | 'pincode'
  | 'contactNumber'
  | 'email';

@Component({
  selector: 'app-admin-hospital-new',
  imports: [ReactiveFormsModule, RouterLink, PageHeader, FormField],
  templateUrl: './hospital-new.html',
  styleUrl: './hospital-new.css',
})
export class AdminHospitalNew {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(AdminApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, notBlank, Validators.maxLength(200)]],
    registrationNumber: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    address: ['', [Validators.required, notBlank, Validators.maxLength(500)]],
    city: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    area: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    pincode: ['', [Validators.required, notBlank, Validators.maxLength(10)]],
    contactNumber: ['', [Validators.required, notBlank, Validators.maxLength(20)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected fieldError(name: HospitalField): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    this.busy.set(true);
    this.error.set(null);

    this.api
      .createHospital({
        name: raw.name.trim(),
        registrationNumber: raw.registrationNumber.trim(),
        address: raw.address.trim(),
        city: raw.city.trim(),
        area: raw.area.trim(),
        pincode: raw.pincode.trim(),
        contactNumber: raw.contactNumber.trim(),
        email: raw.email.trim(),
      })
      .subscribe({
        next: (created) => {
          this.busy.set(false);
          this.toast.success('Hospital registered. Create its admin and activate it next.');
          void this.router.navigate(['/admin/hospitals', created.id]);
        },
        error: (err: unknown) => this.onError(err),
      });
  }

  private onError(err: unknown): void {
    this.busy.set(false);

    if (!(err instanceof ApiError)) {
      this.error.set('Something went wrong. Please try again.');
      return;
    }

    // The API answers 409 only when the registration number is already taken.
    if (err.status === 409) {
      this.form.controls.registrationNumber.setErrors({ server: err.message });
      this.form.controls.registrationNumber.markAsTouched();
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}