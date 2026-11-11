import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UpcomingService } from '../../models';
import { createPager } from '../../utils/pager';
import { Pagination } from '../pagination/pagination';

/** Card with the nearest services of devices, the most urgent first. Overdue ones are marked red. */
@Component({
  selector: 'app-upcoming-services',
  imports: [DatePipe, RouterLink, Pagination],
  styleUrl: './upcoming-services.css',
  templateUrl: './upcoming-services.html',
})
export class UpcomingServices {
  readonly services = input.required<UpcomingService[]>();
  /** Admin only: the inventory number links to the device edit page. */
  readonly linkToEdit = input(false);
  readonly moreLink = input<string | null>(null);
  readonly moreLabel = input('Cały sprzęt →');

  protected readonly pager = createPager(() => this.services(), 5);

  /** "za 5 dni", "dziś" or "po terminie o 3 dni". */
  protected when(s: UpcomingService): string {
    const n = Math.abs(s.daysLeft);
    const days = n === 1 ? '1 dzień' : `${n} dni`;
    if (s.daysLeft < 0) return `po terminie o ${days}`;
    return s.daysLeft === 0 ? 'dziś' : `za ${days}`;
  }

  protected tone(s: UpcomingService): string {
    if (s.daysLeft < 0) return 'late';
    return s.daysLeft <= 14 ? 'soon' : 'ok';
  }
}
