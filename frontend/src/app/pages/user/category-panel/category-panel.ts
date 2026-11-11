import { Component, inject, signal } from '@angular/core';
import { CategoryTile } from '../../../components/category-tile/category-tile';
import { AssetCategoryDto } from '../../../models';
import { AssetCategoriesService } from '../../../services/asset-categories.service';

/** Read-only list of device categories for a logged-in user: tiles without links or actions. */
@Component({
  imports: [CategoryTile],
  selector: 'app-category-panel',
  styleUrl: './category-panel.css',
  templateUrl: './category-panel.html',
})
export class CategoryPanel {
  protected readonly categories = signal<AssetCategoryDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);

  constructor() {
    inject(AssetCategoriesService)
      .getAll()
      .subscribe({
        next: (list) => {
          this.categories.set(list);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set(true);
        },
      });
  }
}
