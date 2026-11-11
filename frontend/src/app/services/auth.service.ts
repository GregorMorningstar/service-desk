import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, finalize, firstValueFrom, shareReplay, switchMap, tap } from 'rxjs';
import { Role } from '../constants/roles';
import { RegisterRequest, TokenResponse, UserDto } from '../models';

const STORAGE_KEY = 'sd_tokens';

interface StoredTokens {
  accessToken: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _user = signal<UserDto | null>(null);
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);
  readonly roles = computed<Role[]>(() => this._user()?.roles ?? []);

  private tokens: StoredTokens | null = readTokens();
  private refreshInFlight: Observable<TokenResponse> | null = null;

  get accessToken(): string | null {
    return this.tokens?.accessToken ?? null;
  }

  get hasRefreshToken(): boolean {
    return !!this.tokens?.refreshToken;
  }

  /** Called on app start: restores the session from localStorage. */
  async init(): Promise<void> {
    if (!this.tokens) return;
    try {
      this._user.set(await firstValueFrom(this.fetchMe()));
    } catch {
      this.clearSession();
    }
  }

  login(email: string, password: string): Observable<UserDto> {
    return this.http.post<TokenResponse>('/api/v1/auth/login', { email, password }).pipe(
      tap((t) => this.saveTokens(t)),
      switchMap(() => this.fetchMe()),
      tap((user) => this._user.set(user)),
    );
  }

  register(data: RegisterRequest): Observable<void> {
    return this.http.post<void>('/api/v1/auth/register', data);
  }

  /** Refreshes the access token. Concurrent callers share one request. */
  refresh(): Observable<TokenResponse> {
    this.refreshInFlight ??= this.http
      .post<TokenResponse>('/api/v1/auth/refresh', { refreshToken: this.tokens?.refreshToken })
      .pipe(
        tap((t) => this.saveTokens(t)),
        finalize(() => (this.refreshInFlight = null)),
        shareReplay(1),
      );
    return this.refreshInFlight;
  }

  logout(): void {
    // Revoke the refresh token server-side; local logout doesn't wait for it.
    const refreshToken = this.tokens?.refreshToken;
    if (refreshToken) {
      this.http.post('/api/v1/auth/logout', { refreshToken }).subscribe({ error: () => {} });
    }
    this.clearSession();
    this.router.navigateByUrl('/login');
  }

  hasAnyRole(roles: readonly Role[]): boolean {
    const mine = this.roles();
    return roles.some((r) => mine.includes(r));
  }

  /** Dashboard for the user's highest role. */
  homeUrl(): string {
    if (this.hasAnyRole(['Administrator'])) return '/admin';
    if (this.hasAnyRole(['Moderator'])) return '/moderator';
    return '/user';
  }

  private fetchMe(): Observable<UserDto> {
    return this.http.get<UserDto>('/api/v1/auth/me');
  }

  private saveTokens(t: TokenResponse): void {
    this.tokens = { accessToken: t.accessToken, refreshToken: t.refreshToken };
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.tokens));
    } catch {
      /* storage blocked: keep the session in memory only */
    }
  }

  private clearSession(): void {
    this.tokens = null;
    this._user.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      /* ignore */
    }
  }
}

function readTokens(): StoredTokens | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as StoredTokens) : null;
  } catch {
    return null;
  }
}
