import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  IconDefinition,
  faClockRotateLeft,
  faMagnifyingGlass,
  faScrewdriverWrench,
  faTriangleExclamation,
} from '@fortawesome/free-solid-svg-icons';
import { AssetDto } from '../../models';

export type AssetAction = 'details' | 'history' | 'inspection' | 'report-failure';

interface ActionDef {
  label: string;
  icon: IconDefinition;
  danger: boolean;
  /** Route segments after `<basePath>/<assetId>`. Null = no page for this action yet. */
  path: string[] | null;
}

interface ActionView extends ActionDef {
  action: AssetAction;
  /** Full router link, or null when there is no page or no `basePath`. */
  link: (string | number)[] | null;
}

const ACTIONS: Record<AssetAction, ActionDef> = {
  details: { label: 'Szczegóły', icon: faMagnifyingGlass, danger: false, path: [] },
  history: { label: 'Historia', icon: faClockRotateLeft, danger: false, path: ['history'] },
  inspection: { label: 'Przegląd', icon: faScrewdriverWrench, danger: false, path: ['inspection'] },
  'report-failure': { label: 'Zgłoś awarię', icon: faTriangleExclamation, danger: true, path: ['failure'] },
};

/**
 * Row of small icon buttons for one device. With a `basePath` an action that has a page
 * becomes a router link (`<basePath>/<assetId>/...`); without one they stay plain buttons.
 * `actions` lets a page offer fewer buttons, e.g. a user vs. an admin.
 */
@Component({
  imports: [FontAwesomeModule, RouterLink],
  selector: 'app-asset-actions',
  styleUrl: './asset-actions.css',
  templateUrl: './asset-actions.html',
})
export class AssetActions {
  readonly asset = input.required<AssetDto>();
  readonly actions = input<readonly AssetAction[]>(['details', 'history', 'inspection', 'report-failure']);
  /** Route prefix of the device pages, e.g. '/user/assets'. Null = no links. */
  readonly basePath = input<string | null>(null);

  protected readonly items = computed<ActionView[]>(() => {
    const base = this.basePath();
    const id = this.asset().id;
    return this.actions().map((action) => {
      const def = ACTIONS[action];
      const link = base !== null && def.path !== null ? [base, id, ...def.path] : null;
      return { action, ...def, link };
    });
  });
}
