import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Barcode } from '../../../components/barcode/barcode';
import { AssetStatus } from '../../../components/asset-status/asset-status';
import { AssetActions } from '../../../components/asset-actions/asset-actions';
import { AssetDto } from '../../../models';
import { AssetsService } from '../../../services/assets.service';
import { PageShell } from '../../../components/page-shell/page-shell';
import { Pagination } from '../../../components/pagination/pagination';
import { createPager } from '../../../utils/pager';

/** The logged-in user's own devices (the backend returns only those for regular users). */
@Component({
  imports: [DatePipe, RouterLink, AssetActions, AssetStatus, Barcode, PageShell, Pagination],
  selector: 'app-assets-panel',
  styleUrl: './assets-panel.css',
  templateUrl: './assets-panel.html',
})
export class AssetsPanel {
  protected readonly assets = signal<AssetDto[]>([]);
  protected readonly pager = createPager(() => this.assets(), 10);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);

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

  /** Inspection is overdue when the planned day has already passed. */
  protected isOverdue(asset: AssetDto): boolean {
    return !!asset.nextInspectionDate && asset.nextInspectionDate < new Date().toISOString().slice(0, 10);
  }
}
