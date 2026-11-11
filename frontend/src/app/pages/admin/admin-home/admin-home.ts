import { Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { Alert } from '../../../components/alert/alert';
import { OpenTickets } from '../../../components/open-tickets/open-tickets';
import { PageShell } from '../../../components/page-shell/page-shell';
import { UpcomingServices } from '../../../components/upcoming-services/upcoming-services';
import { AdminOverview, DashboardStats } from '../../../models';
import { DashboardService } from '../../../services/dashboard.service';

/** Start page of the admin: counters, nearest services and the open tickets that need attention first. */
@Component({
  selector: 'app-admin-home',
  imports: [Alert, OpenTickets, PageShell, UpcomingServices],
  styleUrl: './admin-home.css',
  templateUrl: './admin-home.html',
})
export class AdminHome {
  protected readonly stats = signal<DashboardStats | null>(null);
  protected readonly overview = signal<AdminOverview | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    const api = inject(DashboardService);
    forkJoin({ stats: api.getStats(), overview: api.getOverview() }).subscribe({
      next: ({ stats, overview }) => {
        this.stats.set(stats);
        this.overview.set(overview);
      },
      error: () => this.error.set('Nie udało się pobrać danych panelu.'),
    });
  }
}
