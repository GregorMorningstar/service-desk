import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminOverview, DashboardStats } from '../models';

/** Admin-only /api/v1/dashboard. */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>('/api/v1/dashboard/stats');
  }

  /** Nearest services and the most urgent open tickets. */
  getOverview(): Observable<AdminOverview> {
    return this.http.get<AdminOverview>('/api/v1/dashboard/overview');
  }
}
