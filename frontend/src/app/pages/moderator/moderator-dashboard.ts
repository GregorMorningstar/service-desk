import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Alert } from '../../components/alert/alert';
import { PageShell } from '../../components/page-shell/page-shell';
import { OpenTickets } from '../../components/open-tickets/open-tickets';
import { UpcomingServices } from '../../components/upcoming-services/upcoming-services';
import { AdminOverview, TicketListItem } from '../../models';
import { DashboardService } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';
import { TicketsService } from '../../services/tickets.service';

/** Start page of the moderator: counters, nearest services and their open tickets (failures). */
@Component({
  selector: 'app-moderator-dashboard',
  imports: [RouterLink, Alert, OpenTickets, PageShell, UpcomingServices],
  styleUrl: './moderator-dashboard.css',
  templateUrl: './moderator-dashboard.html',
})
export class ModeratorDashboard {
  protected readonly auth = inject(AuthService);
  protected readonly overview = signal<AdminOverview | null>(null);

  protected readonly tickets = signal<TicketListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly toAccept = computed(() => this.tickets().filter((t) => t.status === 'New' || t.status === 'InProgress').length);
  protected readonly resolved = computed(() => this.tickets().filter((t) => t.status === 'Resolved' || t.status === 'Closed').length);

  constructor() {
    inject(DashboardService)
      .getOverview()
      .subscribe({
        next: (o) => this.overview.set(o),
        error: () => this.error.set('Nie udało się pobrać danych panelu.'),
      });

    inject(TicketsService)
      .getList({ assignedToMe: true, pageSize: 100 })
      .subscribe({
        next: (page) => {
          this.tickets.set(page.items);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nie udało się pobrać zgłoszeń.');
          this.loading.set(false);
        },
      });
  }
}
