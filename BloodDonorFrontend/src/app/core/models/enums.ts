export const Roles = {
  SuperAdmin: 'SuperAdmin',
  HospitalAdmin: 'HospitalAdmin',
  HospitalStaff: 'HospitalStaff',
  Donor: 'Donor',
  Patient: 'Patient',
} as const;
export type Role = (typeof Roles)[keyof typeof Roles];

export const BloodGroup = {
  APlus: 'APlus',
  AMinus: 'AMinus',
  BPlus: 'BPlus',
  BMinus: 'BMinus',
  ABPlus: 'ABPlus',
  ABMinus: 'ABMinus',
  OPlus: 'OPlus',
  OMinus: 'OMinus',
} as const;
export type BloodGroup = (typeof BloodGroup)[keyof typeof BloodGroup];

export const BloodComponent = {
  WholeBlood: 'WholeBlood',
  RedBloodCells: 'RedBloodCells',
  Plasma: 'Plasma',
  Platelets: 'Platelets',
} as const;
export type BloodComponent = (typeof BloodComponent)[keyof typeof BloodComponent];

export const UrgencyLevel = {
  Normal: 'Normal',
  Urgent: 'Urgent',
  Critical: 'Critical',
} as const;
export type UrgencyLevel = (typeof UrgencyLevel)[keyof typeof UrgencyLevel];

export const BloodRequestStatus = {
  Open: 'Open',
  Fulfilled: 'Fulfilled',
  Cancelled: 'Cancelled',
} as const;
export type BloodRequestStatus = (typeof BloodRequestStatus)[keyof typeof BloodRequestStatus];

export const DonorRequestStatus = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Declined: 'Declined',
  Cancelled: 'Cancelled',
} as const;
export type DonorRequestStatus = (typeof DonorRequestStatus)[keyof typeof DonorRequestStatus];

export const VerificationStatus = {
  Pending: 'Pending',
  Verified: 'Verified',
  Rejected: 'Rejected',
} as const;
export type VerificationStatus = (typeof VerificationStatus)[keyof typeof VerificationStatus];

export const HospitalStatus = {
  Pending: 'Pending',
  Active: 'Active',
  Suspended: 'Suspended',
} as const;
export type HospitalStatus = (typeof HospitalStatus)[keyof typeof HospitalStatus];

export const Gender = {
  Male: 'Male',
  Female: 'Female',
  Other: 'Other',
} as const;
export type Gender = (typeof Gender)[keyof typeof Gender];

export const NotificationType = {
  DonorRequestReceived: 'DonorRequestReceived',
  DonorRequestAccepted: 'DonorRequestAccepted',
  DonorRequestDeclined: 'DonorRequestDeclined',
  DonorRequestCancelled: 'DonorRequestCancelled',
  BloodRequestClosed: 'BloodRequestClosed',
} as const;
export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType];