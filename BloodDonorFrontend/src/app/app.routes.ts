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
          { path: 'dashboard', loadComponent: statusPage, data: { title: 'Donor dashboard', message: SOON } },
          { path: 'profile', loadComponent: statusPage, data: { title: 'My profile', message: SOON } },
          { path: 'blood-group', loadComponent: statusPage, data: { title: 'My blood group', message: SOON } },
          { path: 'requests', loadComponent: statusPage, data: { title: 'My requests', message: SOON } },
        ],
      },
      {
        path: 'hospital',
        canActivate: [roleGuard],
        data: { roles: [Roles.HospitalAdmin, Roles.HospitalStaff] },
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
          { path: 'dashboard', loadComponent: statusPage, data: { title: 'Hospital dashboard', message: SOON } },
          { path: 'requests', loadComponent: statusPage, data: { title: 'Blood requests', message: SOON } },
          { path: 'requests/new', loadComponent: statusPage, data: { title: 'New blood request', message: SOON } },
          { path: 'requests/:id', loadComponent: statusPage, data: { title: 'Blood request', message: SOON } },
          { path: 'verification', loadComponent: statusPage, data: { title: 'Verification queue', message: SOON } },
          { path: 'compatibility', loadComponent: statusPage, data: { title: 'Compatibility checker', message: SOON } },
          { path: 'profile', loadComponent: statusPage, data: { title: 'Hospital profile', message: SOON } },
          {
            path: 'staff',
            canActivate: [roleGuard],
            loadComponent: statusPage,
            data: { roles: [Roles.HospitalAdmin], title: 'Staff', message: SOON },
          },
        ],
      },
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { roles: [Roles.SuperAdmin] },
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'hospitals' },
          { path: 'hospitals', loadComponent: statusPage, data: { title: 'Hospitals', message: SOON } },
          { path: 'hospitals/new', loadComponent: statusPage, data: { title: 'New hospital', message: SOON } },
          { path: 'hospitals/:id', loadComponent: statusPage, data: { title: 'Hospital', message: SOON } },
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