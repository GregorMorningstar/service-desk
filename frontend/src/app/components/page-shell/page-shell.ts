import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Page container: padding, optional back link, title with subtitle and a slot for actions. */
@Component({
  selector: 'app-page',
  imports: [RouterLink],
  styleUrl: './page-shell.css',
  templateUrl: './page-shell.html',
})
export class PageShell {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly backLink = input<string | null>(null);
  readonly backLabel = input('Wróć');
}
