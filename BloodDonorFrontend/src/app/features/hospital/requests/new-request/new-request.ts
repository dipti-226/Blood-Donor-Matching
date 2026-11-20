import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { BloodComponent, BloodGroup, UrgencyLevel } from '../../../../core/models/enums';
import {
  BLOOD_COMPONENT_LABELS,
  BLOOD_COMPONENT_OPTIONS,
  BLOOD_GROUP_LABELS,
  BLOOD_GROUP_OPTIONS,
  URGENCY_LABELS,
  URGENCY_OPTIONS,
} from '../../../../core/models/labels';
import { ToastService } from '../../../../core/ui/toast.service';
import { FormField } from '../../../../shared/ui/form-field/form-field';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { applyServerErrors, errorMessage } from '../../../../shared/utils/form-errors';
import { wholeNumber } from '../../../../shared/utils/validators';

type RequestField = 'patientBloodGroup' | 'component' | 'unitsRequired' | 'urgency';

@Component({
  selector: 'app-new-request',
  imports: [ReactiveFormsModule, RouterLink, PageHeader, FormField],
  templateUrl: './new-request.html',
  styleUrl: './new-request.css',
})
export class NewRequest {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly groups = BLOOD_GROUP_OPTIONS;
  protected readonly groupLabels = BLOOD_GROUP_LABELS;
  protected readonly components = BLOOD_COMPONENT_OPTIONS;
  protected readonly componentLabels = BLOOD_COMPONENT_LABELS;
  protected readonly urgencies = URGENCY_OPTIONS;
  protected readonly urgencyLabels = URGENCY_LABELS;

  // Blood group and component start empty so the choice is always deliberate.
  protected readonly form = this.fb.group({
    patientBloodGroup: ['' as BloodGroup | '', [Validators.required]],
    component: ['' as BloodComponent | '', [Validators.required]],
    unitsRequired: [1, [Validators.required, wholeNumber, Validators.min(1), Validators.max(50)]],
    urgency: [UrgencyLevel.Normal as UrgencyLevel, [Validators.required]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected fieldError(name: RequestField): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    const raw = this.form.getRawValue();
    if (this.form.invalid || raw.patientBloodGroup === '' || raw.component === '') {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.api
      .createBloodRequest({
        patientBloodGroup: raw.patientBloodGroup,
        component: raw.component,
        unitsRequired: raw.unitsRequired,
        urgency: raw.urgency,
      })
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.success('Blood request created.');
          void this.router.navigate(['/hospital/requests']);
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

    const unmatched = applyServerErrors(this.form, err);
    this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}