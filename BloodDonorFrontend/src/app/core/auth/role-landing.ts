import { Roles } from '../models/enums';

// Where each kind of account lands after signing in.
// Null means the account has no screens yet (Patient).
export function roleLanding(roles: readonly string[]): string | null {
  if (roles.includes(Roles.SuperAdmin)) {
    return '/admin/hospitals';
  }
  if (roles.includes(Roles.HospitalAdmin) || roles.includes(Roles.HospitalStaff)) {
    return '/hospital/dashboard';
  }
  if (roles.includes(Roles.Donor)) {
    return '/donor/dashboard';
  }
  return null;
}