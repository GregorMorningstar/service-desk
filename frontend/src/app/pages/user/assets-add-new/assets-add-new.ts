import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { AssetForm, AssetFormValue } from '../../../components/asset-form/asset-form';
import { AssetsService } from '../../../services/assets.service';
import { requestErrorMessage } from '../../../services/request-error';
import { PageShell } from '../../../components/page-shell/page-shell';

/** A user adds a device. It is assigned to them once an admin approves it. */
@Component({
  imports: [AssetForm, PageShell],
  selector: 'app-assets-add-new',
  styleUrl: './assets-add-new.css',
  templateUrl: './assets-add-new.html',
})
export class AssetsAddNew {
  private readonly api = inject(AssetsService);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected save({ data, photos }: AssetFormValue): void {
    this.saving.set(true);
    this.error.set(null);

    let created = false;
    this.api
      .request(data)
      .pipe(
        switchMap((asset) => {
          created = true;
          return this.api.uploadPhotos(asset.id, photos);
        }),
      )
      .subscribe({
        next: () => this.router.navigate(['/user/assets']),
        error: (e) => {
          this.saving.set(false);
          this.error.set(
            created
              ? 'Urządzenie zostało dodane, ale nie wszystkie zdjęcia udało się wysłać.'
              : requestErrorMessage(e, 'Nie udało się dodać urządzenia.'),
          );
        },
      });
  }
}
