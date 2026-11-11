import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterOutlet } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faMagnifyingGlass, faPlus, faTableCellsLarge } from '@fortawesome/free-solid-svg-icons';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { SubNav, SubNavItem } from '../../../../components/sub-nav/sub-nav';

/** Layout of the admin category pages: toolbar with search on top, the child route below. */
@Component({
  imports: [FontAwesomeModule, RouterOutlet, SubNav],
  selector: 'app-category-admin',
  styleUrl: './category-admin-component.css',
  templateUrl: './category-admin-component.html',
})
export class CategoryAdminComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly typed = new Subject<string>();

  protected readonly faSearch = faMagnifyingGlass;

  protected readonly items: SubNavItem[] = [
    { link: '/admin/categories', label: 'Kategorie', icon: faTableCellsLarge, exact: true },
    { link: '/admin/categories/new', label: 'Dodaj nową', icon: faPlus },
  ];

  protected readonly initialSearch = this.route.snapshot.queryParamMap.get('search') ?? '';

  constructor() {
    // The list page reads ?search= and asks the backend.
    this.typed
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(inject(DestroyRef)))
      .subscribe((search) =>
        this.router.navigate(['/admin/categories'], {
          queryParams: { search: search.trim() || null },
        }),
      );
  }

  protected onSearch(value: string): void {
    this.typed.next(value);
  }
}
