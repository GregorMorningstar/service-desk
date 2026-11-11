import { Routes } from '@angular/router';
import { guestGuard, roleGuard } from './guards/role.guard';
import { Role } from './constants/roles';

const ADMIN: Role[] = ['Administrator'];
const STAFF: Role[] = ['Administrator', 'Moderator'];
const EVERYONE: Role[] = ['Administrator', 'Moderator', 'User'];
const USER_ONLY: Role[] = ['User'];

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'ServiceDesk · Centrum wsparcia IT',
    loadComponent: () => import('./pages/public/home/home').then((m) => m.Home),
  },
  {
    path: 'login',
    title: 'Logowanie · ServiceDesk',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/public/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    title: 'Rejestracja · ServiceDesk',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/public/register/register').then((m) => m.Register),
  },
  {
    path: 'admin',
    canActivate: [roleGuard],
    canActivateChild: [roleGuard],
    data: { roles: ADMIN },
    children: [
      {
        path: '',
        title: 'Panel administratora · ServiceDesk',
        loadComponent: () => import('./pages/admin/admin-home/admin-home').then((m) => m.AdminHome),
      },
      {
        path: 'users',
        title: 'Użytkownicy · ServiceDesk',
        loadComponent: () => import('./pages/admin/users/admin-dashboard').then((m) => m.AdminDashboard),
      },
      {
        path: 'zgloszenia',
        title: 'Zgłoszenia · ServiceDesk',
        loadComponent: () => import('./pages/admin/tickets/admin-tickets').then((m) => m.AdminTickets),
      },
      {
        path: 'assets',
        loadComponent: () =>
          import('./pages/admin/assets/asset-admin-component/asset-admin-component').then(
            (m) => m.AssetAdminComponent,
          ),
        children: [
          {
            path: '',
            title: 'Sprzęt · ServiceDesk',
            loadComponent: () => import('./pages/admin/assets/asset-list/asset-list').then((m) => m.AssetList),
          },
          {
            path: 'new',
            title: 'Dodaj urządzenie · ServiceDesk',
            loadComponent: () => import('./pages/admin/assets/asset-edit/asset-edit').then((m) => m.AssetEdit),
          },
          {
            path: ':id/edit',
            title: 'Edycja urządzenia · ServiceDesk',
            loadComponent: () => import('./pages/admin/assets/asset-edit/asset-edit').then((m) => m.AssetEdit),
          },
        ],
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./pages/admin/categories/category-admin-component/category-admin-component').then(
            (m) => m.CategoryAdminComponent,
          ),
        children: [
          {
            path: '',
            title: 'Kategorie zgłoszeń · ServiceDesk',
            loadComponent: () =>
              import('./pages/admin/categories/category-list/category-list').then((m) => m.CategoryList),
          },
          {
            path: 'new',
            title: 'Nowa kategoria · ServiceDesk',
            loadComponent: () =>
              import('./pages/admin/categories/category-form/category-form').then((m) => m.CategoryForm),
          },
          {
            path: ':id/edit',
            title: 'Edycja kategorii · ServiceDesk',
            loadComponent: () =>
              import('./pages/admin/categories/category-form/category-form').then((m) => m.CategoryForm),
          },
          {
            path: ':id',
            title: 'Kategoria · ServiceDesk',
            loadComponent: () =>
              import('./pages/admin/categories/category-details/category-details').then(
                (m) => m.CategoryDetails,
              ),
          },
        ],
      },
    ],
  },
  {
    path: 'moderator',
    canActivate: [roleGuard],
    canActivateChild: [roleGuard],
    data: { roles: STAFF },
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/moderator/moderator-layout/moderator-layout').then((m) => m.ModeratorLayout),
        children: [
          {
            path: '',
            title: 'Panel moderatora · ServiceDesk',
            loadComponent: () =>
              import('./pages/moderator/moderator-dashboard').then((m) => m.ModeratorDashboard),
          },
          {
            path: 'akceptacja',
            title: 'Do akceptacji · ServiceDesk',
            loadComponent: () =>
              import('./pages/moderator/moderator-accept/moderator-accept').then((m) => m.ModeratorAccept),
          },
          {
            path: 'zgloszenia',
            title: 'Historia napraw · ServiceDesk',
            loadComponent: () =>
              import('./pages/moderator/moderator-history/moderator-history').then((m) => m.ModeratorHistory),
          },
          {
            path: 'sprzet',
            title: 'Sprzęt · ServiceDesk',
            loadComponent: () =>
              import('./pages/moderator/moderator-assets/moderator-assets').then((m) => m.ModeratorAssets),
          },
        ],
      },
    ],
  },
  {
    path: 'user',
    canActivate: [roleGuard],
    canActivateChild: [roleGuard],
    data: { roles: EVERYONE },
    children: [
      {
        path: '',
        title: 'Mój panel · ServiceDesk',
        loadComponent: () => import('./pages/user/user-dashboard').then((m) => m.UserDashboard),
      },
      {
        path: 'categories',
        title: 'Kategorie · ServiceDesk',
        loadComponent: () =>
          import('./pages/user/category-panel/category-panel').then((m) => m.CategoryPanel),
      },
      {
        // Devices of a regular user: only for the User role (staff use /admin/assets).
        path: 'assets',
        canActivate: [roleGuard],
        canActivateChild: [roleGuard],
        data: { roles: USER_ONLY },
        children: [
          {
            path: '',
            title: 'Urządzenia · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/assets-panel/assets-panel').then((m) => m.AssetsPanel),
          },
          {
            path: 'new',
            title: 'Dodaj sprzęt · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/assets-add-new/assets-add-new').then((m) => m.AssetsAddNew),
          },
          {
            path: ':id',
            title: 'Szczegóły urządzenia · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/asset-details/asset-details').then((m) => m.AssetDetails),
          },
          {
            path: ':id/history',
            title: 'Historia urządzenia · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/asset-history/asset-history').then((m) => m.AssetHistory),
          },
          {
            path: ':id/inspection',
            title: 'Przegląd urządzenia · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/asset-inspection/asset-inspection').then((m) => m.AssetInspection),
          },
          {
            path: ':id/failure',
            title: 'Zgłoszenie awarii · ServiceDesk',
            loadComponent: () =>
              import('./pages/user/asset-failure/asset-failure').then((m) => m.AssetFailure),
          },
        ],
      },
    ],
  },
  {
    path: 'forbidden',
    title: 'Brak dostępu · ServiceDesk',
    loadComponent: () => import('./pages/public/forbidden/forbidden').then((m) => m.Forbidden),
  },
  { path: '**', redirectTo: '' },
];
