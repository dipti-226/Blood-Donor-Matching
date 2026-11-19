import {
  BloodComponent,
  BloodGroup,
  DonorRequestStatus,
  Gender,
  UrgencyLevel,
  VerificationStatus,
} from './enums';

export interface DonorProfile {
  id: string;
  fullName: string;
  dateOfBirth: string;
  gender: Gender;
  address: string;
  city: string;
  area: string;
  pincode: string;
  createdAt: string;
  updatedAt: string;
}

// Used for both creating and updating a profile.
export interface DonorProfileRequest {
  fullName: string;
  dateOfBirth: string;
  gender: Gender;
  address: string;
  city: string;
  area: string;
  pincode: string;
}

export interface DonorAvailability {
  isAvailable: boolean;
}

export interface SetDonorAvailabilityRequest {
  isAvailable: boolean;
}

export interface DeclareBloodGroupRequest {
  bloodGroup: BloodGroup;
}

export interface ResubmitBloodGroupRequest {
  bloodGroup: BloodGroup;
}

export interface DeclareBloodGroupResponse {
  declaredBloodGroup: BloodGroup;
  status: VerificationStatus;
}

export interface DonorBloodGroupStatus {
  declaredBloodGroup: BloodGroup;
  verifiedBloodGroup: BloodGroup | null;
  status: VerificationStatus;
  rejectionReason: string | null;
  createdAt: string;
  verifiedAt: string | null;
}

// A request a hospital sent to this donor.
export interface DonorOwnRequest {
  donorRequestId: string;
  status: DonorRequestStatus;
  hospitalName: string;
  hospitalAddress: string;
  hospitalArea: string;
  hospitalCity: string;
  bloodGroupNeeded: BloodGroup;
  component: BloodComponent;
  unitsRequired: number;
  urgency: UrgencyLevel;
  sharedContactNumber: string | null;
  createdAt: string;
  respondedAt: string | null;
}

export interface AcceptDonorRequestRequest {
  contactNumber: string;
}