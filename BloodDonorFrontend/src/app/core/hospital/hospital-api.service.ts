import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import {
  ApproveBloodGroupRequest,
  BloodGroupReview,
  BloodRequest,
  CompatibilityResult,
  CreateBloodRequestRequest,
  RejectBloodGroupRequest,
  VerificationListItem,
} from '../models/blood-request.models';
import {
  BloodComponent,
  BloodGroup,
  BloodRequestStatus,
  VerificationStatus,
} from '../models/enums';

// What hospital admins and staff can do. Every call needs the user's hospital to be Active.
@Injectable({ providedIn: 'root' })
export class HospitalApi {
  private readonly api = inject(ApiClient);

  listBloodRequests(status: BloodRequestStatus): Observable<BloodRequest[]> {
    return this.api.get<BloodRequest[]>('/api/blood-requests', { status });
  }

  createBloodRequest(body: CreateBloodRequestRequest): Observable<BloodRequest> {
    return this.api.post<BloodRequest>('/api/blood-requests', body);
  }

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
}