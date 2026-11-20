import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HospitalApi } from '../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../core/http/api-error';
import { CompatibilityResult } from '../../../core/models/blood-request.models';
import { BloodComponent, BloodGroup } from '../../../core/models/enums';
import {
  BLOOD_COMPONENT_LABELS,
  BLOOD_COMPONENT_OPTIONS,
  BLOOD_GROUP_LABELS,
  BLOOD_GROUP_OPTIONS,
} from '../../../core/models/labels';
import { BloodGroupChip } from '../../../shared/ui/blood-group-chip/blood-group-chip';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';

@Component({
  selector: 'app-compatibility',
  imports: [ReactiveFormsModule, PageHeader, FormField, BloodGroupChip],
  templateUrl: './compatibility.html',
  styleUrl: './compatibility.css',
})
export class Compatibility {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);

  protected readonly groups = BLOOD_GROUP_OPTIONS;
  protected readonly groupLabels = BLOOD_GROUP_LABELS;
  protected readonly components = BLOOD_COMPONENT_OPTIONS;
  protected readonly componentLabels = BLOOD_COMPONENT_LABELS;

  protected readonly form = this.fb.group({
    recipientBloodGroup: ['' as BloodGroup | '', [Validators.required]],
    component: ['' as BloodComponent | '', [Validators.required]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<CompatibilityResult | null>(null);

  constructor() {
    // An old answer must never stay on screen after the question changes.
    this.form.valueChanges.subscribe(() => this.result.set(null));
  }

  protected fieldError(name: 'recipientBloodGroup' | 'component'): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    const raw = this.form.getRawValue();
    if (this.form.invalid || raw.recipientBloodGroup === '' || raw.component === '') {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.api.getCompatibility(raw.recipientBloodGroup, raw.component).subscribe({
      next: (result) => {
        this.busy.set(false);
        this.result.set(result);
      },
      error: (err: unknown) => {
        this.busy.set(false);
        this.result.set(null);

        if (err instanceof ApiError) {
          const unmatched = applyServerErrors(this.form, err);
          this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
        } else {
          this.error.set('Something went wrong. Please try again.');
        }
      },
    });
  }
}