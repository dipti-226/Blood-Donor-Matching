import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import {
  ApproveBloodGroupRequest,
  BloodGroupReview,
  BloodRequest,
  CancelRequestRequest,
  CompatibilityResult,
  CreateBloodRequestRequest,
  DonorContact,
  DonorMatchList,
  HospitalDonorRequest,
  RejectBloodGroupRequest,
  SendDonorRequestsRequest,
  VerificationListItem,
} from '../models/blood-request.models';
import {BloodComponent,BloodGroup,BloodRequestStatus,VerificationStatus,} from '../models/enums';
import {CreateHospitalUserRequest,Hospital,HospitalMember,UpdateHospitalRequest,} from '../models/hospital.models';

// What hospital admins and staff can do. Blood request, matching and review calls need the
// user's hospital to be Active. The hospital's own details are read through HospitalContextService.
@Injectable({ providedIn: 'root' })
export class HospitalApi {
  private readonly api = inject(ApiClient);

  // ---- Blood requests

  listBloodRequests(status: BloodRequestStatus): Observable<BloodRequest[]> {
    return this.api.get<BloodRequest[]>('/api/blood-requests', { status });
  }

  getBloodRequest(id: string): Observable<BloodRequest> {
    return this.api.get<BloodRequest>(`/api/blood-requests/${id}`);
  }

  createBloodRequest(body: CreateBloodRequestRequest): Observable<BloodRequest> {
    return this.api.post<BloodRequest>('/api/blood-requests', body);
  }

  fulfillBloodRequest(id: string): Observable<BloodRequest> {
    return this.api.post<BloodRequest>(`/api/blood-requests/${id}/fulfill`);
  }

  cancelBloodRequest(id: string, reason: string): Observable<BloodRequest> {
    const body: CancelRequestRequest = { reason };
    return this.api.post<BloodRequest>(`/api/blood-requests/${id}/cancel`, body);
  }

  // ---- Matching and donor requests

  getMatches(bloodRequestId: string): Observable<DonorMatchList> {
    return this.api.get<DonorMatchList>(`/api/blood-requests/${bloodRequestId}/matches`);
  }

  listDonorRequests(bloodRequestId: string): Observable<HospitalDonorRequest[]> {
    return this.api.get<HospitalDonorRequest[]>(`/api/blood-requests/${bloodRequestId}/donor-requests`);
  }

  sendDonorRequests(bloodRequestId: string, donorReferences: string[]): Observable<HospitalDonorRequest[]> {
    const body: SendDonorRequestsRequest = { donorReferences };
    return this.api.post<HospitalDonorRequest[]>(`/api/blood-requests/${bloodRequestId}/donor-requests`, body);
  }

  cancelDonorRequest(bloodRequestId: string, donorRequestId: string): Observable<HospitalDonorRequest> {
    return this.api.post<HospitalDonorRequest>(
      `/api/blood-requests/${bloodRequestId}/donor-requests/${donorRequestId}/cancel`,
    );
  }

  // Each successful call is recorded by the API as a contact disclosure.
  getDonorContact(bloodRequestId: string, donorRequestId: string): Observable<DonorContact> {
    return this.api.get<DonorContact>(
      `/api/blood-requests/${bloodRequestId}/donor-requests/${donorRequestId}/contact`,
    );
  }

  // ---- Compatibility and blood group verification

  getCompatibility(recipientBloodGroup: BloodGroup, component: BloodComponent): Observable<CompatibilityResult> {
    return this.api.get<CompatibilityResult>('/api/blood-compatibility', { recipientBloodGroup, component });
  }

  listVerifications(status: VerificationStatus): Observable<VerificationListItem[]> {
    return this.api.get<VerificationListItem[]>('/api/blood-group-verifications', { status });
  }

  approveVerification(id: string, verifiedBloodGroup: BloodGroup): Observable<BloodGroupReview> {
    const body: ApproveBloodGroupRequest = { verifiedBloodGroup };
    return this.api.post<BloodGroupReview>(`/api/blood-group-verifications/${id}/approve`, body);
  }

  rejectVerification(id: string, reason: string): Observable<BloodGroupReview> {
    const body: RejectBloodGroupRequest = { reason };
    return this.api.post<BloodGroupReview>(`/api/blood-group-verifications/${id}/reject`, body);
  }

  // ---- Hospital details and staff (hospital admin only)

  updateHospital(id: string, body: UpdateHospitalRequest): Observable<Hospital> {
    return this.api.put<Hospital>(`/api/hospitals/${id}`, body);
  }

  listStaff(hospitalId: string): Observable<HospitalMember[]> {
    return this.api.get<HospitalMember[]>(`/api/hospitals/${hospitalId}/staff`);
  }

  createStaff(hospitalId: string, body: CreateHospitalUserRequest): Observable<HospitalMember> {
    return this.api.post<HospitalMember>(`/api/hospitals/${hospitalId}/staff`, body);
  }
}