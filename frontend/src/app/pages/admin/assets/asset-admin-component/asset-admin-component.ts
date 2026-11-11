import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { faPlus, faTableList } from '@fortawesome/free-solid-svg-icons';
import { SubNav, SubNavItem } from '../../../../components/sub-nav/sub-nav';

/** Layout of the admin device pages: submenu on top, the child route below. */
@Component({
  selector: 'app-asset-admin',
  imports: [RouterOutlet, SubNav],
  template: `
    <app-sub-nav title="Sprzęt" [items]="items" />
    <router-outlet />
  `,
  styles: ':host { display: block; }',
})
export class AssetAdminComponent {
  protected readonly items: SubNavItem[] = [
    { link: '/admin/assets', label: 'Lista urządzeń', icon: faTableList, exact: true },
    { link: '/admin/assets/new', label: 'Dodaj urządzenie', icon: faPlus },
  ];
}
