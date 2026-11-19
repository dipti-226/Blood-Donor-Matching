import { HospitalStatus } from './enums';

export interface Hospital {
  id: string;
  name: string;
  registrationNumber: string;
  address: string;
  city: string;
  area: string;
  pincode: string;
  contactNumber: string;
  email: string;
  status: HospitalStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateHospitalRequest {
  name: string;
  registrationNumber: string;
  address: string;
  city: string;
  area: string;
  pincode: string;
  contactNumber: string;
  email: string;
}

// The registration number, status, id and creation time cannot be changed here.
export interface UpdateHospitalRequest {
  name: string;
  address: string;
  city: string;
  area: string;
  pincode: string;
  contactNumber: string;
  email: string;
}

export interface UpdateHospitalStatusRequest {
  status: HospitalStatus;
}

// Used for creating a hospital admin and for creating hospital staff.
export interface CreateHospitalUserRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface HospitalMember {
  hospitalUserId: string;
  hospitalId: string;
  userId: string;
  email: string;
  createdAt: string;
}