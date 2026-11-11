import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import {
  PRIORITY_LABELS,
  PagedResult,
  STATUS_LABELS,
  TicketCategory,
  TicketListItem,
  TicketPriority,
  TicketStatus,
  UserRef,
} from '../../../models';
import { Barcode } from '../../../components/barcode/barcode';
import { TicketCategoriesService } from '../../../services/ticket-categories.service';
import { TicketsService } from '../../../services/tickets.service';
import { UsersService } from '../../../services/users.service';
import { Pagination } from '../../../components/pagination/pagination';
import { PriorityBadge } from '../../../components/priority-badge/priority-badge';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-admin-tickets',
  imports: [DatePipe, Barcode, Pagination, PriorityBadge],
  templateUrl: './admin-tickets.html',
  styleUrl: './admin-tickets.css',
  host: { class: 'container' },
})
export class AdminTickets {
  private readonly ticketsApi = inject(TicketsService);
  private readonly usersApi = inject(UsersService);
  private readonly categoriesApi = inject(TicketCategoriesService);

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly priorityLabels = PRIORITY_LABELS;
  protected readonly statuses = Object.keys(STATUS_LABELS) as TicketStatus[];
  protected readonly priorities = Object.keys(PRIORITY_LABELS) as TicketPriority[];
  protected readonly categories = signal<TicketCategory[]>([]);

  protected readonly status = signal<TicketStatus | ''>('');
  protected readonly priority = signal<TicketPriority | ''>('');
  /** Selected category id as the select gives it (string), empty = all. */
  protected readonly category = signal('');
  protected readonly search = signal('');
  protected readonly page = signal(1);

  protected readonly result = signal<PagedResult<TicketListItem> | null>(null);
  protected readonly technicians = signal<UserRef[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly savingId = signal<number | null>(null);

  protected readonly tickets = computed(() => this.result()?.items ?? []);
  protected readonly totalPages = computed(() => Math.max(this.result()?.totalPages ?? 1, 1));

  constructor() {
    this.load();
    this.categoriesApi.getAll().subscribe({ next: (list) => this.categories.set(list) });
    this.usersApi.getTechnicians().subscribe({
      next: (list) => this.technicians.set(list),
      error: () => this.error.set('Nie udało się pobrać listy techników.'),
    });
  }

  protected load(): void {
    this.loading.set(true);
    this.ticketsApi
      .getList({
        status: this.status() || undefined,
        priority: this.priority() || undefined,
        categoryId: this.category() ? Number(this.category()) : undefined,
        search: this.search().trim(),
        page: this.page(),
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Nie udało się pobrać zgłoszeń.');
          this.loading.set(false);
        },
      });
  }

  protected applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  protected resetFilters(): void {
    this.status.set('');
    this.priority.set('');
    this.category.set('');
    this.search.set('');
    this.applyFilters();
  }

  protected goTo(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.page.set(page);
    this.load();
  }

  protected assign(ticket: TicketListItem, assigneeId: string): void {
    const id = assigneeId || null;
    this.savingId.set(ticket.id);
    this.error.set(null);
    this.success.set(null);

    this.ticketsApi.assign(ticket.id, id).subscribe({
      next: () => {
        const assignee = this.technicians().find((t) => t.id === id) ?? null;
        this.result.update((r) =>
          r && { ...r, items: r.items.map((t) => (t.id === ticket.id ? { ...t, assignee } : t)) },
        );
        this.savingId.set(null);
        this.success.set(
          assignee
            ? `${ticket.number}: przypisano do ${assignee.fullName || assignee.email}.`
            : `${ticket.number}: usunięto przypisanie.`,
        );
      },
      error: (e: HttpErrorResponse) => {
        this.savingId.set(null);
        this.error.set(e.error?.detail ?? 'Nie udało się zmienić przypisania.');
        // Reload so the select shows the real assignee again
        this.load();
      },
    });
  }

  protected statusClass(status: TicketStatus): string {
    return `status status-${status.toLowerCase()}`;
  }
}
