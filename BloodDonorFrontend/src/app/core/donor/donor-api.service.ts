import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import { notFoundToNull } from '../http/not-found-to-null';
import { BloodGroup } from '../models/enums';
import {
  AcceptDonorRequestRequest,
  DeclareBloodGroupRequest,
  DeclareBloodGroupResponse,
  DonorAvailability,
  DonorBloodGroupStatus,
  DonorOwnRequest,
  DonorProfile,
  DonorProfileRequest,
  ResubmitBloodGroupRequest,
  SetDonorAvailabilityRequest,
} from '../models/donor.models';

// Everything the signed-in donor can do. Methods that ask "does it exist yet?" return null on a 404.
@Injectable({ providedIn: 'root' })
export class DonorApi {
  private readonly api = inject(ApiClient);

  getProfile(): Observable<DonorProfile | null> {
    return this.api.get<DonorProfile>('/api/donor/profile').pipe(notFoundToNull());
  }

  createProfile(body: DonorProfileRequest): Observable<DonorProfile> {
    return this.api.post<DonorProfile>('/api/donor/profile', body);
  }

  updateProfile(body: DonorProfileRequest): Observable<DonorProfile> {
    return this.api.put<DonorProfile>('/api/donor/profile', body);
  }

  // Null means the donor has no profile yet.
  getAvailability(): Observable<DonorAvailability | null> {
    return this.api.get<DonorAvailability>('/api/donor/availability').pipe(notFoundToNull());
  }

  setAvailability(isAvailable: boolean): Observable<DonorAvailability> {
    const body: SetDonorAvailabilityRequest = { isAvailable };
    return this.api.put<DonorAvailability>('/api/donor/availability', body);
  }

  // Null means no blood group has been declared yet.
  getBloodGroup(): Observable<DonorBloodGroupStatus | null> {
    return this.api.get<DonorBloodGroupStatus>('/api/donor/blood-group').pipe(notFoundToNull());
  }

  declareBloodGroup(bloodGroup: BloodGroup): Observable<DeclareBloodGroupResponse> {
    const body: DeclareBloodGroupRequest = { bloodGroup };
    return this.api.post<DeclareBloodGroupResponse>('/api/donor/blood-group', body);
  }

  resubmitBloodGroup(bloodGroup: BloodGroup): Observable<DonorBloodGroupStatus> {
    const body: ResubmitBloodGroupRequest = { bloodGroup };
    return this.api.post<DonorBloodGroupStatus>('/api/donor/blood-group/resubmit', body);
  }

  listRequests(): Observable<DonorOwnRequest[]> {
    return this.api.get<DonorOwnRequest[]>('/api/donor/requests');
  }

  acceptRequest(donorRequestId: string, contactNumber: string): Observable<DonorOwnRequest> {
    const body: AcceptDonorRequestRequest = { contactNumber };
    return this.api.post<DonorOwnRequest>(`/api/donor/requests/${donorRequestId}/accept`, body);
  }

  declineRequest(donorRequestId: string): Observable<DonorOwnRequest> {
    return this.api.post<DonorOwnRequest>(`/api/donor/requests/${donorRequestId}/decline`);
  }
}