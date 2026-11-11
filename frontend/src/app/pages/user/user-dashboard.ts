import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ROLE_LABELS } from '../../constants/roles';

@Component({
  selector: 'app-user-dashboard',
  host: { class: 'container' },
  template: `
    @if (auth.user(); as user) {
      <header class="page-header">
        <h1>Witaj, {{ user.firstName || user.email }}!</h1>
        <p class="muted">To jest Twój panel użytkownika.</p>
      </header>

      <section class="card">
        <h2>Twoje konto</h2>
        <div class="table-wrap">
          <table>
            <tbody>
              <tr>
                <th>Email</th>
                <td>{{ user.email }}</td>
              </tr>
              <tr>
                <th>Imię i nazwisko</th>
                <td>{{ user.firstName }} {{ user.lastName }}</td>
              </tr>
              <tr>
                <th>Rola</th>
                <td>
                  @for (role of user.roles; track role) {
                    <span class="badge badge-{{ role.toLowerCase() }}">{{ roleLabels[role] }}</span>
                  }
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    }
  `,
})
export class UserDashboard {
  protected readonly auth = inject(AuthService);
  protected readonly roleLabels = ROLE_LABELS;
}
