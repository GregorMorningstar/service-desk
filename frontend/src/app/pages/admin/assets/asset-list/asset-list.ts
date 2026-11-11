import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, Subject, catchError, debounceTime, map, merge, of, startWith, switchMap, tap } from 'rxjs';
import { Barcode } from '../../../../components/barcode/barcode';
import { AssetStatus } from '../../../../components/asset-status/asset-status';
import { ApprovalStatus, AssetDto } from '../../../../models';
import { AdminAssetsService } from '../../../../services/admin-assets.service';
import { Alert } from '../../../../components/alert/alert';
import { PageShell } from '../../../../components/page-shell/page-shell';
import { Pagination } from '../../../../components/pagination/pagination';
import { createPager } from '../../../../utils/pager';

/** Admin list of devices with filters (name, user, address) and the approve / switch off / delete actions. */
@Component({
  selector: 'app-asset-list',
  imports: [DatePipe, ReactiveFormsModule, RouterLink, AssetStatus, Barcode, Alert, PageShell, Pagination],
  styleUrl: './asset-list.css',
  templateUrl: './asset-list.html',
})
export class AssetList {
  private readonly api = inject(AdminAssetsService);
  private readonly route = inject(ActivatedRoute);

  protected readonly filters = inject(NonNullableFormBuilder).group({
    search: '',
    owner: '',
    address: '',
    status: '' as ApprovalStatus | '',
    isActive: '',
  });

  private readonly categoryId = this.route.snapshot.queryParamMap.has('categoryId')
    ? Number(this.route.snapshot.queryParamMap.get('categoryId'))
    : null;

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  private readonly reload = new Subject<void>();

  protected readonly assets = toSignal(
    merge(this.filters.valueChanges.pipe(debounceTime(300)), this.reload).pipe(
      startWith(null),
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(() => this.fetch()),
      tap(() => this.loading.set(false)),
    ),
    { initialValue: [] as AssetDto[] },
  );

  protected readonly pager = createPager(() => this.assets(), 10);

  /** Category name for the heading, taken from the loaded devices. */
  protected categoryName(): string | null {
    return this.categoryId === null ? null : (this.assets()[0]?.category.name ?? null);
  }

  protected showBack(): boolean {
    return this.categoryId !== null;
  }

  protected isOverdue(a: AssetDto): boolean {
    return !!a.nextInspectionDate && a.nextInspectionDate < new Date().toISOString().slice(0, 10);
  }

  protected approve(a: AssetDto): void {
    this.act(this.api.approve(a.id), 'Nie udało się zatwierdzić urządzenia.');
  }

  protected reject(a: AssetDto): void {
    this.act(this.api.reject(a.id), 'Nie udało się odrzucić urządzenia.');
  }

  protected toggleActive(a: AssetDto): void {
    this.act(this.api.setActive(a.id, !a.isActive), 'Nie udało się zmienić statusu urządzenia.');
  }

  protected remove(a: AssetDto): void {
    if (!confirm(`Usunąć urządzenie ${a.inventoryNumber}?`)) return;
    this.act(this.api.delete(a.id), 'Nie udało się usunąć urządzenia.');
  }

  private fetch(): Observable<AssetDto[]> {
    const f = this.filters.getRawValue();
    return this.api
      .getAll({
        search: f.search.trim(),
        owner: f.owner.trim(),
        address: f.address.trim(),
        status: f.status || null,
        isActive: f.isActive === '' ? null : f.isActive === 'true',
        categoryId: this.categoryId,
      })
      .pipe(
        catchError(() => {
          this.error.set('Nie udało się pobrać sprzętu.');
          return of<AssetDto[]>([]);
        }),
      );
  }

  private act(request: Observable<unknown>, failure: string): void {
    request
      .pipe(
        map(() => null),
        catchError(() => {
          this.error.set(failure);
          return of(null);
        }),
      )
      .subscribe(() => this.reload.next());
  }
}
