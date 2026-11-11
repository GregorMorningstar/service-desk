import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AssetCategoryDto } from '../../models';

type Severity = 'high' | 'medium' | 'low';

interface Bubble {
  severity: Severity;
  count: number;
  label: string;
}

/**
 * Tile of one device category. What it offers depends on the inputs, so the same
 * tile can be used for the admin, a moderator or a user (who may get no links at all).
 */
@Component({
  selector: 'app-category-tile',
  imports: [RouterLink],
  styleUrl: './category-tile.css',
  templateUrl: './category-tile.html',
})
export class CategoryTile {
  readonly category = input.required<AssetCategoryDto>();

  /** Route prefix of the details page (the id is appended). Null = tile is not clickable. */
  readonly detailsLink = input<string | null>(null);
  /** Page the device count links to, e.g. '/admin/assets'. Null = plain, non-clickable count. */
  readonly assetsLink = input<string | null>(null);

  protected readonly initial = computed(() => this.category().name.trim().charAt(0).toUpperCase());

  /** One bubble per priority that has open tickets; critical is shown together with high. */
  protected readonly bubbles = computed<Bubble[]>(() => {
    const t = this.category().activeTickets;
    const all: Bubble[] = [
      { severity: 'high', count: t.high + t.critical, label: 'wysoki priorytet' },
      { severity: 'medium', count: t.medium, label: 'średni priorytet' },
      { severity: 'low', count: t.low, label: 'niski priorytet' },
    ];
    return all.filter((b) => b.count > 0);
  });
}
