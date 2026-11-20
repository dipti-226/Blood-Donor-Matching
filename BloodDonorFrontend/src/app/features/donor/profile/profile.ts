import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, map, of, switchMap } from 'rxjs';
import { DonorApi } from '../../../core/donor/donor-api.service';
import { ApiError } from '../../../core/http/api-error';
import { DonorProfile, DonorProfileRequest } from '../../../core/models/donor.models';
import { Gender } from '../../../core/models/enums';
import { GENDER_LABELS, GENDER_OPTIONS } from '../../../core/models/labels';
import { ToastService } from '../../../core/ui/toast.service';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { todayIso } from '../../../shared/utils/date-input';
import { formatDateTime } from '../../../shared/utils/dates';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';
import { notBlank, pastDate } from '../../../shared/utils/validators';
import { AvailabilityCard } from '../availability-card/availability-card';

type ProfileField = 'fullName' | 'dateOfBirth' | 'gender' | 'address' | 'city' | 'area' | 'pincode';

interface ProfileView {
  // Null means the donor has not created a profile yet.
  profile: DonorProfile | null;
  available: boolean;
}

@Component({
  selector: 'app-donor-profile-page',
  imports: [
    ReactiveFormsModule,
    PageHeader,
    LoadingState,
    ErrorBanner,
    FormField,
    AvailabilityCard,
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class DonorProfilePage implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(DonorApi);
  private readonly toast = inject(ToastService);

  protected readonly genders = GENDER_OPTIONS;
  protected readonly genderLabels = GENDER_LABELS;
  protected readonly today = todayIso();
  protected readonly formatDateTime = formatDateTime;

  protected readonly form = this.fb.group({
    fullName: ['', [Validators.required, notBlank, Validators.maxLength(200)]],
    dateOfBirth: ['', [Validators.required, pastDate]],
    gender: ['' as Gender | '', [Validators.required]],
    address: ['', [Validators.required, notBlank, Validators.maxLength(500)]],
    city: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    area: ['', [Validators.required, notBlank, Validators.maxLength(100)]],
    pincode: ['', [Validators.required, notBlank, Validators.maxLength(10)]],
  });

  protected readonly state = signal<LoadState<ProfileView>>(LOADING);
  protected readonly busy = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly view = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : null;
  });
  protected readonly profile = computed(() => this.view()?.profile ?? null);
  protected readonly available = computed(() => this.view()?.available ?? false);
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

    this.loadView().subscribe({
      next: (data) => {
        this.fillForm(data.profile);
        this.state.set({ status: 'ready', data });
      },
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected fieldError(name: ProfileField): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected setAvailable(available: boolean): void {
    const view = this.view();
    if (view) {
      this.state.set({ status: 'ready', data: { ...view, available } });
    }
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
    if (raw.gender === '') {
      return;
    }

    const body: DonorProfileRequest = {
      fullName: raw.fullName.trim(),
      dateOfBirth: raw.dateOfBirth,
      gender: raw.gender,
      address: raw.address.trim(),
      city: raw.city.trim(),
      area: raw.area.trim(),
      pincode: raw.pincode.trim(),
    };

    const creating = this.profile() === null;
    this.busy.set(true);
    this.formError.set(null);

    const request = creating ? this.api.createProfile(body) : this.api.updateProfile(body);

    request.subscribe({
      next: (saved) => {
        this.busy.set(false);
        this.fillForm(saved);
        // A new profile starts as not available. An edit leaves availability as it was.
        this.state.set({ status: 'ready', data: { profile: saved, available: creating ? false : this.available() } });
        this.toast.success(creating ? 'Profile created.' : 'Profile saved.');
      },
      error: (err: unknown) => this.onError(err),
    });
  }

  private loadView(): Observable<ProfileView> {
    return this.api.getProfile().pipe(
      switchMap((profile) => {
        if (!profile) {
          return of<ProfileView>({ profile: null, available: false });
        }

        return this.api
          .getAvailability()
          .pipe(map((availability): ProfileView => ({ profile, available: availability?.isAvailable ?? false })));
      }),
    );
  }

  private fillForm(profile: DonorProfile | null): void {
    this.form.reset({
      fullName: profile?.fullName ?? '',
      dateOfBirth: profile?.dateOfBirth ?? '',
      gender: profile?.gender ?? '',
      address: profile?.address ?? '',
      city: profile?.city ?? '',
      area: profile?.area ?? '',
      pincode: profile?.pincode ?? '',
    });
  }

  private onError(err: unknown): void {
    this.busy.set(false);

    if (!(err instanceof ApiError)) {
      this.formError.set('Something went wrong. Please try again.');
      return;
    }

    // The profile changed on the server (created elsewhere, or gone): show the message and reload.
    if (err.status === 404 || err.status === 409) {
      this.toast.error(err.message);
      this.load();
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.formError.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}