import {BloodComponent,BloodGroup,BloodRequestStatus,DonorRequestStatus,Gender,HospitalStatus,UrgencyLevel,VerificationStatus,} from './enums';

export const BLOOD_GROUP_LABELS: Record<BloodGroup, string> = {
  APlus: 'A+',
  AMinus: 'A-',
  BPlus: 'B+',
  BMinus: 'B-',
  ABPlus: 'AB+',
  ABMinus: 'AB-',
  OPlus: 'O+',
  OMinus: 'O-',
};

export const BLOOD_COMPONENT_LABELS: Record<BloodComponent, string> = {
  WholeBlood: 'Whole blood',
  RedBloodCells: 'Red blood cells',
  Plasma: 'Plasma',
  Platelets: 'Platelets',
};

export const URGENCY_LABELS: Record<UrgencyLevel, string> = {
  Normal: 'Normal',
  Urgent: 'Urgent',
  Critical: 'Critical',
};

export const BLOOD_REQUEST_STATUS_LABELS: Record<BloodRequestStatus, string> = {
  Open: 'Open',
  Fulfilled: 'Fulfilled',
  Cancelled: 'Cancelled',
};

export const DONOR_REQUEST_STATUS_LABELS: Record<DonorRequestStatus, string> = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Declined: 'Declined',
  Cancelled: 'Cancelled',
};

export const VERIFICATION_STATUS_LABELS: Record<VerificationStatus, string> = {
  Pending: 'Pending',
  Verified: 'Verified',
  Rejected: 'Rejected',
};

export const HOSPITAL_STATUS_LABELS: Record<HospitalStatus, string> = {
  Pending: 'Pending',
  Active: 'Active',
  Suspended: 'Suspended',
};

export const GENDER_LABELS: Record<Gender, string> = {
  Male: 'Male',
  Female: 'Female',
  Other: 'Other',
};

// Option lists for dropdowns, in a sensible display order.
export const BLOOD_GROUP_OPTIONS: BloodGroup[] = Object.values(BloodGroup);
export const BLOOD_COMPONENT_OPTIONS: BloodComponent[] = Object.values(BloodComponent);
export const URGENCY_OPTIONS: UrgencyLevel[] = Object.values(UrgencyLevel);
export const GENDER_OPTIONS: Gender[] = Object.values(Gender);

// Badge colour variant for every status and urgency value (see styles/utilities.css).
const BADGE_VARIANT: Record<string, string> = {
  Pending: 'warning',
  Open: 'primary',
  Verified: 'success',
  Accepted: 'success',
  Fulfilled: 'success',
  Active: 'success',
  Rejected: 'error',
  Declined: 'error',
  Cancelled: 'neutral',
  Suspended: 'neutral',
  Normal: 'neutral',
  Urgent: 'warning',
  Critical: 'accent',
};

export function badgeClass(value: string | null | undefined): string {
  const variant = value ? BADGE_VARIANT[value] : undefined;
  return `bd-badge bd-badge--${variant ?? 'neutral'}`;
}

export function labelOf<T extends string>(
  labels: Record<T, string>,
  value: T | null | undefined,
): string {
  return value ? labels[value] : '-';
}