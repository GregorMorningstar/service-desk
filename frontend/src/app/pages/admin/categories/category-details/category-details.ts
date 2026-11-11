import { HttpErrorResponse } from '@angular/common/http';
import { Barcode } from '../../../../components/barcode/barcode';
import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AssetCategoryDto,
  CategoryTicket,
  PRIORITY_LABELS,
  ProblemDetails,
  STATUS_LABELS,
} from '../../../../models';
import { AssetCategoriesService } from '../../../../services/asset-categories.service';

/** Details of one device category, with edit and delete. Admin only. */
@Component({
  selector: 'app-category-details',
  imports: [RouterLink, DatePipe, Barcode],
  styleUrl: './category-details.css',
  templateUrl: './category-details.html',
})
export class CategoryDetails {
  private readonly api = inject(AssetCategoriesService);
  private readonly router = inject(Router);

  protected readonly id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id'));

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly priorityLabels = PRIORITY_LABELS;

  protected readonly category = signal<AssetCategoryDto | null>(null);
  protected readonly tickets = signal<CategoryTicket[]>([]);
  protected readonly ticketsLoading = signal(true);
  protected readonly ticketsError = signal(false);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly confirming = signal(false);
  protected readonly deleting = signal(false);

  constructor() {
    this.api.get(this.id).subscribe({
      next: (c) => {
        this.category.set(c);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Nie znaleziono kategorii.');
      },
    });

    this.api.getTickets(this.id).subscribe({
      next: (list) => {
        this.tickets.set(list);
        this.ticketsLoading.set(false);
      },
      error: () => {
        this.ticketsLoading.set(false);
        this.ticketsError.set(true);
      },
    });
  }

  protected remove(): void {
    this.deleting.set(true);
    this.error.set(null);

    this.api.delete(this.id).subscribe({
      next: () => this.router.navigate(['/admin/categories']),
      error: (e: HttpErrorResponse) => {
        this.deleting.set(false);
        this.confirming.set(false);
        const problem = e.error as ProblemDetails | null;
        this.error.set(problem?.detail ?? 'Nie udało się usunąć kategorii.');
      },
    });
  }
}
