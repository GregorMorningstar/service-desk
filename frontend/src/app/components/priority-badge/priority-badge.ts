import { Component, computed, input } from '@angular/core';
import { PRIORITY_LABELS, TicketPriority } from '../../models';

const LEVEL: Record<TicketPriority, number> = { Low: 1, Medium: 2, High: 3, Critical: 4 };

/** Priority of a ticket as a label with a level meter (1-4 bars) in the colour of the level. */
@Component({
  selector: 'app-priority-badge',
  styleUrl: './priority-badge.css',
  templateUrl: './priority-badge.html',
})
export class PriorityBadge {
  readonly priority = input.required<TicketPriority>();

  protected readonly label = computed(() => PRIORITY_LABELS[this.priority()]);
  protected readonly level = computed(() => LEVEL[this.priority()]);
  protected readonly bars = [1, 2, 3, 4];
}
