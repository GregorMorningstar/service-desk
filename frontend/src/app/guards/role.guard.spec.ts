import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Route, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';
import { Role } from '../constants/roles';

function fakeAuth(roles: Role[] | null) {
  return {
    isLoggedIn: () => roles !== null,
    hasAnyRole: (required: readonly Role[]) => required.some((r) => roles?.includes(r)),
  };
}

/** Child snapshot whose parent has `data.roles`, like /admin/zgloszenia. */
function childOf(parentRoles: Role[]): ActivatedRouteSnapshot {
  const parent = { routeConfig: { data: { roles: parentRoles } } as Route, parent: null };
  return { routeConfig: { path: 'zgloszenia' } as Route, parent } as unknown as ActivatedRouteSnapshot;
}

function run(roles: Role[] | null, route: ActivatedRouteSnapshot) {
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: AuthService, useValue: fakeAuth(roles) }],
  });
  const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as RouterStateSnapshot));
  const router = TestBed.inject(Router);
  return result instanceof UrlTree ? router.serializeUrl(result) : result;
}

describe('roleGuard', () => {
  it('lets an admin into an admin child route', () => {
    expect(run(['Administrator'], childOf(['Administrator']))).toBe(true);
  });

  it('sends a regular user to /forbidden', () => {
    expect(run(['User'], childOf(['Administrator']))).toBe('/forbidden');
  });

  it('sends a logged-out visitor to /login', () => {
    expect(run(null, childOf(['Administrator']))).toBe('/login');
  });

  it('lets a moderator into a staff route', () => {
    expect(run(['Moderator'], childOf(['Administrator', 'Moderator']))).toBe(true);
  });
});
