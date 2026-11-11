import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AssetDto, AssetHistoryEntry } from '../../../models';
import { AssetsService } from '../../../services/assets.service';
import { Alert } from '../../../components/alert/alert';
import { PageShell } from '../../../components/page-shell/page-shell';
import { Pagination } from '../../../components/pagination/pagination';
import { createPager } from '../../../utils/pager';

/** Names of the fields the backend writes into the device history. */
const FIELD_LABELS: Record<string, string> = {
  Created: 'Dodano urządzenie',
  Name: 'Nazwa',
  Category: 'Kategoria',
  SerialNumber: 'Numer seryjny',
  InventoryNumber: 'Numer inwentarzowy',
  Location: 'Lokalizacja',
  Address: 'Adres',
  Owner: 'Właściciel',
  InspectionInterval: 'Interwał przeglądu (mies.)',
  NextInspection: 'Następny przegląd',
};

/** Change history of one of the user's devices, newest first. */
@Component({
  imports: [DatePipe, Alert, PageShell, Pagination],
  selector: 'app-asset-history',
  styleUrl: './asset-history.css',
  templateUrl: './asset-history.html',
})
export class AssetHistory {
  private readonly api = inject(AssetsService);

  protected readonly asset = signal<AssetDto | null>(null);
  protected readonly entries = signal<AssetHistoryEntry[]>([]);
  protected readonly pager = createPager(() => this.entries(), 10);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    const id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id'));

    this.api.get(id).subscribe({
      next: (a) => this.asset.set(a),
      error: () => this.fail('Nie znaleziono urządzenia.'),
    });

    this.api.getHistory(id).subscribe({
      next: (list) => {
        // The backend sends the oldest entry first.
        this.entries.set([...list].reverse());
        this.loading.set(false);
      },
      error: () => this.fail('Nie znaleziono urządzenia.'),
    });
  }

  protected fieldLabel(field: string): string {
    return FIELD_LABELS[field] ?? field;
  }

  private fail(message: string): void {
    this.loading.set(false);
    this.error.set(message);
  }
}
