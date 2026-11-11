import { Component, input } from '@angular/core';

/** Error / success message box; renders nothing for an empty message. */
@Component({
  selector: 'app-alert',
  template: `
    @if (message()) {
      <p class="alert" [class.alert-error]="kind() === 'error'" [class.alert-success]="kind() === 'success'"
         [attr.role]="kind() === 'error' ? 'alert' : 'status'">{{ message() }}</p>
    }
  `,
})
export class Alert {
  readonly message = input<string | null | undefined>(null);
  readonly kind = input<'error' | 'success'>('error');
}
