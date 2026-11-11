import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Alert } from '../../../components/alert/alert';
import { PageShell } from '../../../components/page-shell/page-shell';
import { Pagination } from '../../../components/pagination/pagination';
import { PriorityBadge } from '../../../components/priority-badge/priority-badge';
import { PagedResult, STATUS_LABELS, TicketListItem, TicketStatus } from '../../../models';
import { TicketsService } from '../../../services/tickets.service';

const PAGE_SIZE = 10;

/** Repair history of the logged-in moderator: only the tickets assigned to them, any status. */
@Component({
  selector: 'app-moderator-history',
  imports: [DatePipe, Alert, PageShell, Pagination, PriorityBadge],
  styleUrl: './moderator-history.css',
  templateUrl: './moderator-history.html',
})
export class ModeratorHistory {
  private readonly api = inject(TicketsService);

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly statuses = Object.keys(STATUS_LABELS) as TicketStatus[];

  protected readonly status = signal<TicketStatus | ''>('');
  protected readonly page = signal(1);
  protected readonly result = signal<PagedResult<TicketListItem> | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected setStatus(value: string): void {
    this.status.set(value as TicketStatus | '');
    this.goTo(1);
  }

  protected goTo(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getList({ assignedToMe: true, status: this.status() || undefined, page: this.page(), pageSize: PAGE_SIZE })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Nie udało się pobrać historii.');
        },
      });
  }
}
