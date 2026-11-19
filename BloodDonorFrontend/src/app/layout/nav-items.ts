import { Role, Roles } from '../core/models/enums';

export interface NavItem {
  label: string;
  route: string;
  icon: string;
  roles: readonly Role[];
  exact?: boolean;
}

const HOSPITAL: Role[] = [Roles.HospitalAdmin, Roles.HospitalStaff];

export const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/donor/dashboard', icon: 'bi-house-door', roles: [Roles.Donor] },
  { label: 'Profile', route: '/donor/profile', icon: 'bi-person', roles: [Roles.Donor] },
  { label: 'Blood group', route: '/donor/blood-group', icon: 'bi-droplet', roles: [Roles.Donor] },
  { label: 'My requests', route: '/donor/requests', icon: 'bi-inbox', roles: [Roles.Donor] },

  { label: 'Dashboard', route: '/hospital/dashboard', icon: 'bi-speedometer2', roles: HOSPITAL },
  { label: 'Blood requests', route: '/hospital/requests', icon: 'bi-droplet-half', roles: HOSPITAL },
  { label: 'Verification queue', route: '/hospital/verification', icon: 'bi-patch-check', roles: HOSPITAL },
  { label: 'Compatibility', route: '/hospital/compatibility', icon: 'bi-arrow-left-right', roles: HOSPITAL },
  { label: 'Hospital profile', route: '/hospital/profile', icon: 'bi-hospital', roles: HOSPITAL },
  { label: 'Staff', route: '/hospital/staff', icon: 'bi-people', roles: [Roles.HospitalAdmin] },

  { label: 'Hospitals', route: '/admin/hospitals', icon: 'bi-hospital', roles: [Roles.SuperAdmin], exact: true },
  { label: 'New hospital', route: '/admin/hospitals/new', icon: 'bi-plus-circle', roles: [Roles.SuperAdmin] },

  {
    label: 'Notifications',
    route: '/notifications',
    icon: 'bi-bell',
    roles: [Roles.Donor, Roles.HospitalAdmin, Roles.HospitalStaff],
  },
];