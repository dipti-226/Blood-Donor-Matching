import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import { HospitalStatus } from '../models/enums';
import {
  CreateHospitalRequest,
  CreateHospitalUserRequest,
  Hospital,
  HospitalMember,
  UpdateHospitalStatusRequest,
} from '../models/hospital.models';

// What the system administrator (SuperAdmin) can do: manage every hospital and its admins.
@Injectable({ providedIn: 'root' })
export class AdminApi {
  private readonly api = inject(ApiClient);

  listHospitals(): Observable<Hospital[]> {
    return this.api.get<Hospital[]>('/api/hospitals');
  }

  getHospital(id: string): Observable<Hospital> {
    return this.api.get<Hospital>(`/api/hospitals/${id}`);
  }

  createHospital(body: CreateHospitalRequest): Observable<Hospital> {
    return this.api.post<Hospital>('/api/hospitals', body);
  }

  setHospitalStatus(id: string, status: HospitalStatus): Observable<Hospital> {
    const body: UpdateHospitalStatusRequest = { status };
    return this.api.patch<Hospital>(`/api/hospitals/${id}/status`, body);
  }

  listAdmins(hospitalId: string): Observable<HospitalMember[]> {
    return this.api.get<HospitalMember[]>(`/api/hospitals/${hospitalId}/admins`);
  }

  createAdmin(hospitalId: string, body: CreateHospitalUserRequest): Observable<HospitalMember> {
    return this.api.post<HospitalMember>(`/api/hospitals/${hospitalId}/admins`, body);
  }
}