import {BloodComponent,BloodGroup,BloodRequestStatus,DonorRequestStatus,UrgencyLevel,VerificationStatus,} from './enums';

export interface BloodRequest {
  id: string;
  hospitalId: string;
  patientBloodGroup: BloodGroup;
  component: BloodComponent;
  unitsRequired: number;
  urgency: UrgencyLevel;
  status: BloodRequestStatus;
  createdAt: string;
  closedAt: string | null;
  cancellationReason: string | null;
  // Donor requests sent and not cancelled.
  askedCount: number;
  // Donor requests currently accepted.
  acceptedCount: number;
}

export interface CreateBloodRequestRequest {
  patientBloodGroup: BloodGroup;
  component: BloodComponent;
  unitsRequired: number;
  urgency: UrgencyLevel;
}

export interface CancelRequestRequest {
  reason: string;
}

export interface DonorMatch {
  // Opaque reference, not a user id.
  donorReference: string;
  verifiedBloodGroup: BloodGroup;
  city: string;
  area: string;
  isExactBloodGroupMatch: boolean;
}

export interface DonorMatchList {
  bloodRequestId: string;
  totalCount: number;
  matches: DonorMatch[];
  notice: string;
}

export interface SendDonorRequestsRequest {
  donorReferences: string[];
}

// The hospital's view of a request sent to a donor: no name and no contact.
export interface HospitalDonorRequest {
  id: string;
  bloodRequestId: string;
  donorReference: string;
  status: DonorRequestStatus;
  verifiedBloodGroup: BloodGroup | null;
  city: string;
  area: string;
  createdAt: string;
  respondedAt: string | null;
}

// Only returned for an accepted request, and each read is logged by the API.
export interface DonorContact {
  donorRequestId: string;
  fullName: string;
  contactNumber: string;
}

export interface CompatibilityResult {
  recipientBloodGroup: BloodGroup;
  component: BloodComponent;
  compatibleDonorBloodGroups: BloodGroup[];
}

export interface VerificationListItem {
  id: string;
  donorFullName: string | null;
  declaredBloodGroup: BloodGroup;
  verifiedBloodGroup: BloodGroup | null;
  status: VerificationStatus;
  rejectionReason: string | null;
  createdAt: string;
  verifiedAt: string | null;
}

export interface ApproveBloodGroupRequest {
  verifiedBloodGroup: BloodGroup;
}

export interface RejectBloodGroupRequest {
  reason: string;
}

export interface BloodGroupReview {
  id: string;
  declaredBloodGroup: BloodGroup;
  verifiedBloodGroup: BloodGroup | null;
  status: VerificationStatus;
  verifiedAt: string | null;
  rejectionReason: string | null;
}