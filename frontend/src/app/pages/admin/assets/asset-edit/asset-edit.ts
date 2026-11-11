import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { Barcode } from '../../../../components/barcode/barcode';
import { AssetForm, AssetFormValue } from '../../../../components/asset-form/asset-form';
import { AssetDto } from '../../../../models';
import { AdminAssetsService } from '../../../../services/admin-assets.service';
import { AssetsService } from '../../../../services/assets.service';
import { requestErrorMessage } from '../../../../services/request-error';
import { Alert } from '../../../../components/alert/alert';
import { PageShell } from '../../../../components/page-shell/page-shell';

/** Add (/new) or edit (/:id/edit) a device for any user. Admin only. */
@Component({
  selector: 'app-asset-edit',
  imports: [AssetForm, Barcode, Alert, PageShell],
  styleUrl: './asset-edit.css',
  templateUrl: './asset-edit.html',
})
export class AssetEdit {
  private readonly api = inject(AdminAssetsService);
  private readonly photosApi = inject(AssetsService);
  private readonly router = inject(Router);

  /** Set when editing. */
  protected readonly id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id')) || null;

  protected readonly asset = signal<AssetDto | null>(null);
  protected readonly loading = signal(this.id !== null);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    if (this.id === null) return;

    this.api.get(this.id).subscribe({
      next: (a) => {
        this.asset.set(a);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Nie znaleziono urządzenia.');
      },
    });
  }

  protected save({ data, photos }: AssetFormValue): void {
    this.saving.set(true);
    this.error.set(null);

    let saved = false;
    const request = this.id === null ? this.api.create(data) : this.api.update(this.id, data);
    request
      .pipe(
        switchMap((asset) => {
          saved = true;
          return this.photosApi.uploadPhotos(asset.id, photos);
        }),
      )
      .subscribe({
        next: () => this.router.navigate(['/admin/assets']),
        error: (e) => {
          this.saving.set(false);
          this.error.set(
            saved
              ? 'Urządzenie zapisano, ale nie wszystkie zdjęcia udało się wysłać.'
              : requestErrorMessage(e, 'Nie udało się zapisać urządzenia.'),
          );
        },
      });
  }
}
