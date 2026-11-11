import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TicketListItem } from '../../models';
import { createPager } from '../../utils/pager';
import { Pagination } from '../pagination/pagination';
import { PriorityBadge } from '../priority-badge/priority-badge';

/** Card with open tickets (failures) and their priority level. */
@Component({
  selector: 'app-open-tickets',
  imports: [RouterLink, Pagination, PriorityBadge],
  styleUrl: './open-tickets.css',
  templateUrl: './open-tickets.html',
})
export class OpenTickets {
  readonly tickets = input.required<TicketListItem[]>();
  readonly moreLink = input<string | null>(null);
  readonly moreLabel = input('Wszystkie zgłoszenia →');

  protected readonly pager = createPager(() => this.tickets(), 5);
}
