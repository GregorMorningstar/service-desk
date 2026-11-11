import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { AssetPhoto } from '../../models';
import { AssetsService } from '../../services/assets.service';

interface LoadedPhoto {
  photo: AssetPhoto;
  /** Object URL of the downloaded image. */
  url: string;
}

/** Photos already stored for a device, with an optional delete button. */
@Component({
  selector: 'app-asset-photos',
  styleUrl: './asset-photos.css',
  templateUrl: './asset-photos.html',
})
export class AssetPhotos {
  private readonly api = inject(AssetsService);

  readonly assetId = input.required<number>();
  readonly editable = input(false);

  protected readonly photos = signal<LoadedPhoto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.photos().forEach((p) => URL.revokeObjectURL(p.url)));

    effect(() => {
      const id = this.assetId();
      this.api.getPhotos(id).subscribe({
        next: (list) => {
          this.loading.set(false);
          list.forEach((photo) => this.load(id, photo));
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Nie udało się pobrać zdjęć.');
        },
      });
    });
  }

  protected remove(item: LoadedPhoto): void {
    if (!confirm('Usunąć to zdjęcie?')) return;

    this.api.deletePhoto(this.assetId(), item.photo.id).subscribe({
      next: () => {
        URL.revokeObjectURL(item.url);
        this.photos.update((list) => list.filter((p) => p !== item));
      },
      error: () => this.error.set('Nie udało się usunąć zdjęcia.'),
    });
  }

  private load(assetId: number, photo: AssetPhoto): void {
    this.api.getPhotoBlob(assetId, photo.id).subscribe({
      next: (blob) =>
        this.photos.update((list) => [...list, { photo, url: URL.createObjectURL(blob) }]),
      error: () => this.error.set('Nie udało się pobrać zdjęć.'),
    });
  }
}
