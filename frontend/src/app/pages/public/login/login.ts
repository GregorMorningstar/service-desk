import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  host: { class: 'container' },
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly justRegistered =
    inject(ActivatedRoute).snapshot.queryParamMap.has('registered');

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => this.router.navigateByUrl(this.auth.homeUrl()),
      error: (e: HttpErrorResponse) => {
        this.loading.set(false);
        if (e.status === 401) {
          // API says whether it's wrong credentials or a locked account
          this.error.set(e.error?.detail ?? 'Nieprawidłowy email lub hasło.');
        } else if (e.status === 429) {
          this.error.set('Za dużo prób logowania. Odczekaj minutę.');
        } else {
          this.error.set('Nie udało się połączyć z serwerem. Spróbuj ponownie.');
        }
      },
    });
  }
}
