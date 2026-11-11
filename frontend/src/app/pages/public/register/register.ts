import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { ProblemDetails } from '../../../models';

/** Polish messages for ASP.NET Identity error codes */
const IDENTITY_ERRORS: Record<string, string> = {
  DuplicateUserName: 'Konto z tym adresem email już istnieje.',
  DuplicateEmail: 'Konto z tym adresem email już istnieje.',
  InvalidEmail: 'Nieprawidłowy adres email.',
  InvalidUserName: 'Nieprawidłowy adres email.',
  PasswordTooShort: 'Hasło musi mieć co najmniej 8 znaków.',
  PasswordRequiresDigit: 'Hasło musi zawierać cyfrę.',
  PasswordRequiresUpper: 'Hasło musi zawierać wielką literę.',
  PasswordRequiresLower: 'Hasło musi zawierać małą literę.',
  PasswordRequiresNonAlphanumeric: 'Hasło musi zawierać znak specjalny (np. !@#).',
  PasswordRequiresUniqueChars: 'Hasło zawiera za mało różnych znaków.',
};

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const { password, confirmPassword } = group.value as Record<string, string>;
  return password === confirmPassword ? null : { passwordsMismatch: true };
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  host: { class: 'container' },
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = inject(NonNullableFormBuilder).group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/),
        ],
      ],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordsMatch },
  );

  protected readonly loading = signal(false);
  protected readonly errors = signal<string[]>([]);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errors.set([]);

    const { confirmPassword: _, ...data } = this.form.getRawValue();
    this.auth.register(data).subscribe({
      next: () => this.router.navigate(['/login'], { queryParams: { registered: 1 } }),
      error: (e: HttpErrorResponse) => {
        this.loading.set(false);
        this.errors.set(this.toMessages(e));
      },
    });
  }

  private toMessages(e: HttpErrorResponse): string[] {
    const problem = e.error as ProblemDetails | null;
    if (e.status === 400 && problem?.errors) {
      return Object.entries(problem.errors).flatMap(([code, msgs]) =>
        IDENTITY_ERRORS[code] ? [IDENTITY_ERRORS[code]] : msgs,
      );
    }
    return ['Nie udało się utworzyć konta. Spróbuj ponownie.'];
  }
}
