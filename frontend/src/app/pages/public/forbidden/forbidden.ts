import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-forbidden',
  imports: [RouterLink],
  host: { class: 'container' },
  template: `
    <section class="card auth-card">
      <h1>Brak dostępu</h1>
      <p class="muted">Nie masz uprawnień do tej strony.</p>
      <a class="btn" [routerLink]="auth.homeUrl()">Wróć do swojego panelu</a>
    </section>
  `,
})
export class Forbidden {
  protected readonly auth = inject(AuthService);
}
