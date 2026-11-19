import { Injectable, effect, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiClient } from '../http/api-client';
import { Hospital } from '../models/hospital.models';
import { SessionService } from './session.service';

// Holds the signed-in hospital user's own hospital (id, name, status).
@Injectable({ providedIn: 'root' })
export class HospitalContextService {
  private readonly api = inject(ApiClient);
  private readonly session = inject(SessionService);
  private readonly state = signal<Hospital | null>(null);

  readonly hospital = this.state.asReadonly();

  constructor() {
    // Personal data never outlives the session.
    effect(() => {
      if (!this.session.isAuthenticated()) {
        this.state.set(null);
      }
    });
  }

  load(): Observable<Hospital> {
    return this.api.get<Hospital>('/api/hospitals/mine').pipe(tap((hospital) => this.state.set(hospital)));
  }
}