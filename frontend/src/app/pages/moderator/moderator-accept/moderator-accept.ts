import { Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Alert } from '../../../components/alert/alert';
import { PageShell } from '../../../components/page-shell/page-shell';
import { Pagination } from '../../../components/pagination/pagination';
import { PriorityBadge } from '../../../components/priority-badge/priority-badge';
import { STATUS_LABELS, TicketListItem } from '../../../models';
import { requestErrorMessage } from '../../../services/request-error';
import { TicketsService } from '../../../services/tickets.service';
import { createPager } from '../../../utils/pager';

/**
 * Tickets assigned to the logged-in moderator that wait to be finished. "Akceptuj" asks for the changes made
 * and, for a ticket about a device, optionally for the next service date.
 */
@Component({
  selector: 'app-moderator-accept',
  imports: [ReactiveFormsModule, Alert, PageShell, Pagination, PriorityBadge],
  styleUrl: './moderator-accept.css',
  templateUrl: './moderator-accept.html',
})
export class ModeratorAccept {
  private readonly api = inject(TicketsService);

  protected readonly statusLabels = STATUS_LABELS;

  protected readonly tickets = signal<TicketListItem[]>([]);
  protected readonly pager = createPager(() => this.tickets(), 10);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  /** Ticket whose form is open. */
  protected readonly openId = signal<number | null>(null);
  protected readonly saving = signal(false);

  protected readonly open = computed(() => this.tickets().find((t) => t.id === this.openId()) ?? null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    resolution: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(10000)]],
    nextInspectionDate: [''],
  });

  constructor() {
    this.load();
  }

  protected toggle(ticket: TicketListItem): void {
    this.error.set(null);
    this.form.reset();
    this.openId.set(this.openId() === ticket.id ? null : ticket.id);
  }

  protected accept(): void {
    const ticket = this.open();
    if (!ticket) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { resolution, nextInspectionDate } = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);

    this.api
      .accept(ticket.id, {
        resolution: resolution.trim(),
        nextInspectionDate: ticket.asset && nextInspectionDate ? nextInspectionDate : null,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.openId.set(null);
          this.tickets.update((list) => list.filter((t) => t.id !== ticket.id));
          this.success.set(`${ticket.number}: zlecenie zaakceptowane.`);
        },
        error: (e) => {
          this.saving.set(false);
          this.error.set(requestErrorMessage(e, 'Nie udało się zaakceptować zlecenia.'));
        },
      });
  }

  private load(): void {
    // The backend returns only the tickets assigned to the caller.
    this.api.getList({ assignedToMe: true, pageSize: 100 }).subscribe({
      next: (page) => {
        this.tickets.set(page.items.filter((t) => t.status === 'New' || t.status === 'InProgress'));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Nie udało się pobrać zgłoszeń.');
      },
    });
  }
}
