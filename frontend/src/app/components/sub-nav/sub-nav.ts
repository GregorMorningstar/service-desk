import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';

export interface SubNavItem {
  link: string;
  label: string;
  icon: IconDefinition;
  /** Highlight only on the exact url (the list page, not its children). */
  exact?: boolean;
}

/** Toolbar under the topbar: title, optional projected content (e.g. search) and icon links. */
@Component({
  selector: 'app-sub-nav',
  imports: [FontAwesomeModule, RouterLink, RouterLinkActive],
  styleUrl: './sub-nav.css',
  templateUrl: './sub-nav.html',
})
export class SubNav {
  readonly title = input.required<string>();
  readonly items = input.required<SubNavItem[]>();
}
