import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Pagination } from '../../../components/pagination/pagination';
import { PageShell } from '../../../components/page-shell/page-shell';
import { createPager } from '../../../utils/pager';
import { Barcode } from '../../../components/barcode/barcode';
import { AssetStatus } from '../../../components/asset-status/asset-status';
import { AssetDto } from '../../../models';
import { AssetsService } from '../../../services/assets.service';

/** Read-only list of all devices for staff, with a quick text filter. */
@Component({
  selector: 'app-moderator-assets',
  imports: [DatePipe, Barcode, AssetStatus, PageShell, Pagination],
  styleUrl: './moderator-assets.css',
  templateUrl: './moderator-assets.html',
})
export class ModeratorAssets {
  protected readonly assets = signal<AssetDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly filter = signal('');

  protected readonly visible = computed(() => {
    const term = this.filter().trim().toLowerCase();
    if (!term) return this.assets();

    return this.assets().filter((a) =>
      [a.name, a.inventoryNumber, a.serialNumber, a.owner?.fullName, a.owner?.email, a.address?.city, a.address?.street]
        .some((v) => v?.toLowerCase().includes(term)),
    );
  });

  protected readonly pager = createPager(() => this.visible(), 10);

  constructor() {
    inject(AssetsService)
      .getAll()
      .subscribe({
        next: (list) => {
          this.assets.set(list);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set(true);
        },
      });
  }

  protected isOverdue(a: AssetDto): boolean {
    return !!a.nextInspectionDate && a.nextInspectionDate < new Date().toISOString().slice(0, 10);
  }
}
