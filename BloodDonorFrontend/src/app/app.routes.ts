import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/auth/auth.guards';
import { Roles } from './core/models/enums';

const statusPage = () =>
  import('./features/errors/status-page/status-page').then((m) => m.StatusPage);

const SOON = 'This screen is built in a later step.';

export const routes: Routes = [
  // Public pages
  {
    path: '',
    loadComponent: () => import('./layout/public-layout/public-layout').then((m) => m.PublicLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/landing/landing/landing').then((m) => m.Landing),
      },
      {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
      },
      {
        path: 'register',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
      },
    ],
  },

  // Signed-in area
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/app-shell/app-shell').then((m) => m.AppShell),
    children: [
      {
        path: 'donor',
        canActivate: [roleGuard],
        data: { roles: [Roles.Donor] },
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
          {
            path: 'dashboard',
            loadComponent: () => import('./features/donor/dashboard/dashboard').then((m) => m.DonorDashboard),
          },
          {
            path: 'profile',
            loadComponent: () => import('./features/donor/profile/profile').then((m) => m.DonorProfilePage),
          },
          {
            path: 'blood-group',
            loadComponent: () => import('./features/donor/blood-group/blood-group').then((m) => m.DonorBloodGroupPage),
          },
          {
            path: 'requests',
            loadComponent: () => import('./features/donor/requests/requests').then((m) => m.DonorRequestsPage),
          },
        ]
      },
      {
        path: 'hospital',
        canActivate: [roleGuard],
        data: { roles: [Roles.HospitalAdmin, Roles.HospitalStaff] },
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
          {
            path: 'dashboard',
            loadComponent: () => import('./features/hospital/dashboard/dashboard').then((m) => m.HospitalDashboard),
          },
          {
            path: 'requests',
            loadComponent: () =>
              import('./features/hospital/requests/request-list/request-list').then((m) => m.RequestList),
          },
          {
            path: 'requests/new',
            loadComponent: () =>
              import('./features/hospital/requests/new-request/new-request').then((m) => m.NewRequest),
          },
          {
            path: 'requests/:id',
            loadComponent: () =>
              import('./features/hospital/requests/request-detail/request-detail').then((m) => m.RequestDetail),
          }, {
            path: 'verification',
            loadComponent: () => import('./features/hospital/verification/verification').then((m) => m.Verification),
          },
          {
            path: 'compatibility',
            loadComponent: () =>
              import('./features/hospital/compatibility/compatibility').then((m) => m.Compatibility),
          },
          {
            path: 'profile',
            loadComponent: () => import('./features/hospital/profile/profile').then((m) => m.HospitalProfilePage),
          },
          {
            path: 'staff',
            canActivate: [roleGuard],
            data: { roles: [Roles.HospitalAdmin] },
            loadComponent: () => import('./features/hospital/staff/staff').then((m) => m.StaffPage),
          },
        ],
      },
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { roles: [Roles.SuperAdmin] },
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'hospitals' },
          {
            path: 'hospitals',
            loadComponent: () =>
              import('./features/admin/hospital-list/hospital-list').then((m) => m.AdminHospitalList),
          },
          {
            path: 'hospitals/new',
            loadComponent: () =>
              import('./features/admin/hospital-new/hospital-new').then((m) => m.AdminHospitalNew),
          },
          {
            path: 'hospitals/:id',
            loadComponent: () =>
              import('./features/admin/hospital-detail/hospital-detail').then((m) => m.AdminHospitalDetail),
          },
        ],
      },
      {
        path: 'notifications',
        canActivate: [roleGuard],
        data: { roles: [Roles.Donor, Roles.HospitalAdmin, Roles.HospitalStaff] },
        loadComponent: () =>
          import('./features/notifications/notifications/notifications').then((m) => m.Notifications),
      },
    ],
  },

  // Standalone status pages
  {
    path: 'forbidden',
    loadComponent: statusPage,
    data: { code: '403', title: 'You do not have access to this page', message: 'Your account role cannot open this page.' },
  },
  {
    path: '**',
    loadComponent: statusPage,
    data: { code: '404', title: 'Page not found', message: 'The page you are looking for does not exist or has moved.' },
  },
];