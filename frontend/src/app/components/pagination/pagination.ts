import { Component, input, output } from '@angular/core';

/** Previous / next buttons with the page counter. Hidden when there is only one page. */
@Component({
  selector: 'app-pagination',
  styleUrl: './pagination.css',
  template: `
    @if (totalPages() > 1) {
      <nav class="pager" aria-label="Stronicowanie">
        <button type="button" class="btn btn-ghost btn-sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">
          ← Poprzednia
        </button>
        <span>Strona {{ page() }} z {{ totalPages() }}</span>
        <button
          type="button"
          class="btn btn-ghost btn-sm"
          [disabled]="page() >= totalPages()"
          (click)="pageChange.emit(page() + 1)"
        >
          Następna →
        </button>
      </nav>
    }
  `,
})
export class Pagination {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly pageChange = output<number>();
}
