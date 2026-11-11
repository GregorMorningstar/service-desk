import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { CategoryTile } from '../../../../components/category-tile/category-tile';
import { AssetCategoryDto } from '../../../../models';
import { AssetCategoriesService } from '../../../../services/asset-categories.service';

/** 3 tiles per row, 3 rows per page. */
const PAGE_SIZE = 9;

/** Admin list of device categories, filtered by ?search= (searched in the backend). */
@Component({
  selector: 'app-category-list',
  imports: [CategoryTile],
  styleUrl: './category-list.css',
  templateUrl: './category-list.html',
})
export class CategoryList {
  private readonly api = inject(AssetCategoriesService);
  private readonly route = inject(ActivatedRoute);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly page = signal(1);

  protected readonly search = toSignal(
    this.route.queryParamMap.pipe(map((p) => p.get('search') ?? '')),
    { initialValue: '' },
  );

  private readonly loaded = toSignal(
    this.route.queryParamMap.pipe(
      map((p) => p.get('search') ?? ''),
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
        this.page.set(1);
      }),
      switchMap((search) =>
        this.api.getAll(search).pipe(
          catchError(() => {
            this.error.set('Nie udało się pobrać kategorii.');
            return of<AssetCategoryDto[]>([]);
          }),
        ),
      ),
      tap(() => this.loading.set(false)),
    ),
    { initialValue: [] as AssetCategoryDto[] },
  );

  protected readonly categories = this.loaded;

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.categories().length / PAGE_SIZE)),
  );

  protected readonly pageItems = computed(() => {
    const start = (Math.min(this.page(), this.pageCount()) - 1) * PAGE_SIZE;
    return this.categories().slice(start, start + PAGE_SIZE);
  });

  protected readonly pages = computed(() =>
    Array.from({ length: this.pageCount() }, (_, i) => i + 1),
  );

  protected goTo(page: number): void {
    this.page.set(Math.min(Math.max(page, 1), this.pageCount()));
  }
}
