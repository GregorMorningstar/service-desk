import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Role } from '../constants/roles';

/** Pages only for logged-out users (login, register). */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isLoggedIn() ? inject(Router).parseUrl(auth.homeUrl()) : true;
};

/**
 * Requires login and one of the roles from `data.roles`.
 * Works as canActivate and canActivateChild; children use the nearest parent's roles.
 */
export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) return router.parseUrl('/login');

  const roles = requiredRoles(route);
  return roles.length === 0 || auth.hasAnyRole(roles) ? true : router.parseUrl('/forbidden');
};

function requiredRoles(route: ActivatedRouteSnapshot): Role[] {
  for (let r: ActivatedRouteSnapshot | null = route; r; r = r.parent) {
    const roles = r.routeConfig?.data?.['roles'] as Role[] | undefined;
    if (roles) return roles;
  }
  return [];
}
