import { Component, OnDestroy, effect, inject, input, output, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { AsyncValidatorFn, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { catchError, distinctUntilChanged, map, of, startWith, switchMap, timer } from 'rxjs';
import { AssetCategoryDto, AssetDto, KnownAddress, SaveAsset, UserDto } from '../../models';
import { AssetCategoriesService } from '../../services/asset-categories.service';
import { AssetsService } from '../../services/assets.service';
import { UsersService } from '../../services/users.service';
import { AssetPhotos } from '../asset-photos/asset-photos';
import {
  AddressFields,
  addressFromGroup,
  createAddressGroup,
  patchAddress,
} from '../address-fields/address-fields';

export const MAX_PHOTO_BYTES = 5 * 1024 * 1024;
export const MAX_PHOTOS = 10;
const PHOTO_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

/** What the form emits: the device data and the new photos to upload afterwards. */
export interface AssetFormValue {
  data: SaveAsset;
  photos: File[];
}

interface PhotoPreview {
  file: File;
  url: string;
}

/**
 * Device form used by the user ("Dodaj sprzęt", needs approval) and by the admin (add / edit).
 * Admin mode adds the owner, the active flag and the inspection schedule.
 */
@Component({
  selector: 'app-asset-form',
  imports: [ReactiveFormsModule, RouterLink, AddressFields, AssetPhotos],
  styleUrl: './asset-form.css',
  templateUrl: './asset-form.html',
})
export class AssetForm implements OnDestroy {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly assetsApi = inject(AssetsService);

  /** Device to edit; empty for a new one. */
  readonly asset = input<AssetDto | null>(null);
  readonly admin = input(false);
  readonly saving = input(false);
  readonly error = input<string | null>(null);
  readonly submitLabel = input('Zapisz');
  readonly cancelLink = input.required<string>();

  readonly save = output<AssetFormValue>();

  /** Photos picked in this form, uploaded by the page after the device is saved. */
  protected readonly previews = signal<PhotoPreview[]>([]);
  protected readonly photoError = signal<string | null>(null);

  protected readonly categories = toSignal(
    inject(AssetCategoriesService)
      .getAll()
      .pipe(catchError(() => of<AssetCategoryDto[]>([]))),
    { initialValue: [] as AssetCategoryDto[] },
  );

  private readonly usersApi = inject(UsersService);
  /** Only loaded for the admin (the endpoint is admin-only). */
  protected readonly users = toSignal(
    toObservable(this.admin).pipe(
      switchMap((isAdmin) =>
        isAdmin ? this.usersApi.getAll().pipe(catchError(() => of<UserDto[]>([]))) : of<UserDto[]>([]),
      ),
    ),
    { initialValue: [] as UserDto[] },
  );

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    serialNumber: ['', [Validators.required, Validators.maxLength(50)], [this.notTaken('serialNumber')]],
    inventoryNumber: ['', [Validators.required, Validators.maxLength(30)], [this.notTaken('inventoryNumber')]],
    description: ['', Validators.maxLength(500)],
    location: ['', Validators.maxLength(100)],
    address: createAddressGroup(this.fb),
    ownerId: [''],
    isActive: [true],
    inspectionIntervalMonths: [12, [Validators.min(1), Validators.max(120)]],
    nextInspectionDate: [''],
  });

  /**
   * Addresses to pick from: the user's own and those of their other devices. A user gets their own;
   * the admin gets the ones of the user chosen as the owner.
   */
  protected readonly knownAddresses = toSignal(
    toObservable(this.admin).pipe(
      switchMap((isAdmin) =>
        isAdmin
          ? this.form.controls.ownerId.valueChanges.pipe(
              startWith(this.form.controls.ownerId.value),
              distinctUntilChanged(),
            )
          : of(null),
      ),
      switchMap((ownerId) =>
        ownerId === '' // admin without an owner chosen yet
          ? of<KnownAddress[]>([])
          : this.assetsApi.getKnownAddresses(ownerId).pipe(catchError(() => of<KnownAddress[]>([]))),
      ),
    ),
    { initialValue: [] as KnownAddress[] },
  );

  constructor() {
    effect(() => {
      const a = this.asset();
      if (!a) return;
      this.form.patchValue({
        name: a.name,
        categoryId: a.category.id,
        serialNumber: a.serialNumber,
        inventoryNumber: a.inventoryNumber,
        description: a.description ?? '',
        location: a.location ?? '',
        ownerId: a.owner?.id ?? '',
        isActive: a.isActive,
        inspectionIntervalMonths: a.inspectionIntervalMonths,
        nextInspectionDate: a.nextInspectionDate ?? '',
      });
      patchAddress(this.form.controls.address, a.address);
    });
  }

  /** Asks the backend (after a short pause in typing) whether another device already has this number. */
  private notTaken(field: 'serialNumber' | 'inventoryNumber'): AsyncValidatorFn {
    return (control) => {
      const value = String(control.value ?? '').trim();
      if (!value) return of(null);

      return timer(400).pipe(
        switchMap(() => this.assetsApi.checkAvailability({ [field]: value, exceptId: this.asset()?.id })),
        map((r) => ((field === 'serialNumber' ? r.serialTaken : r.inventoryTaken) ? { taken: true } : null)),
        catchError(() => of(null)), // the backend still rejects a duplicate on save
      );
    };
  }

  ngOnDestroy(): void {
    this.previews().forEach((p) => URL.revokeObjectURL(p.url));
  }

  protected pickPhotos(input: HTMLInputElement): void {
    const files = Array.from(input.files ?? []);
    input.value = ''; // lets the same file be picked again

    const room = MAX_PHOTOS - (this.asset()?.photoCount ?? 0) - this.previews().length;
    const accepted: PhotoPreview[] = [];
    let error: string | null = null;

    for (const file of files) {
      if (!PHOTO_TYPES.includes(file.type)) error = 'Dozwolone formaty zdjęć: JPEG, PNG, WebP.';
      else if (file.size > MAX_PHOTO_BYTES) error = 'Zdjęcie może mieć maksymalnie 5 MB.';
      else if (accepted.length >= room) error = `Urządzenie może mieć maksymalnie ${MAX_PHOTOS} zdjęć.`;
      else accepted.push({ file, url: URL.createObjectURL(file) });
    }

    this.photoError.set(error);
    this.previews.update((list) => [...list, ...accepted]);
  }

  /** Fills the address fields from the suggestion at the given index. */
  protected pickKnownAddress(select: HTMLSelectElement): void {
    const picked = this.knownAddresses()[Number(select.value)];
    select.value = '';
    if (picked) patchAddress(this.form.controls.address, picked.address);
  }

  protected removePreview(item: PhotoPreview): void {
    URL.revokeObjectURL(item.url);
    this.previews.update((list) => list.filter((p) => p !== item));
  }

  protected invalid(name: 'name' | 'categoryId' | 'serialNumber' | 'inventoryNumber'): boolean {
    const c = this.form.controls[name];
    return c.touched && c.invalid;
  }

  protected submit(): void {
    // Wait for the duplicate checks to finish.
    if (this.form.pending) return;

    const address = this.form.controls.address;
    const v = this.form.getRawValue();

    // The address is optional, but a half-filled one is not.
    const hasAddress = [v.address.street, v.address.buildingNumber, v.address.postalCode, v.address.city].some(
      (x) => x.trim(),
    );
    const addressInvalid = hasAddress && address.invalid;

    if (this.form.controls.name.invalid || this.form.controls.categoryId.invalid ||
        this.form.controls.serialNumber.invalid || this.form.controls.inventoryNumber.invalid ||
        this.form.controls.description.invalid || this.form.controls.location.invalid ||
        this.form.controls.inspectionIntervalMonths.invalid || addressInvalid) {
      this.form.markAllAsTouched();
      return;
    }

    const data: SaveAsset = {
      name: v.name.trim(),
      categoryId: v.categoryId,
      serialNumber: v.serialNumber.trim(),
      inventoryNumber: v.inventoryNumber.trim(),
      description: v.description.trim() || null,
      location: v.location.trim() || null,
      address: hasAddress ? addressFromGroup(address) : null,
      ownerId: v.ownerId || null,
      isActive: v.isActive,
      inspectionIntervalMonths: v.inspectionIntervalMonths || null,
      nextInspectionDate: v.nextInspectionDate || null,
    };

    this.save.emit({ data, photos: this.previews().map((p) => p.file) });
  }
}
