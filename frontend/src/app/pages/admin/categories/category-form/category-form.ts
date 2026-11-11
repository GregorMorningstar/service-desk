import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AssetCategoriesService } from '../../../../services/asset-categories.service';
import { requestErrorMessage } from '../../../../services/request-error';

/** Add (/new) or edit (/:id/edit) a device category. Admin only. */
@Component({
  selector: 'app-category-form',
  imports: [ReactiveFormsModule, RouterLink],
  styleUrl: './category-form.css',
  templateUrl: './category-form.html',
})
export class CategoryForm {
  private readonly api = inject(AssetCategoriesService);
  private readonly router = inject(Router);

  /** Set when editing. */
  protected readonly id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id')) || null;

  protected readonly form = inject(NonNullableFormBuilder).group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    description: ['', Validators.maxLength(255)],
  });

  protected readonly loading = signal(this.id !== null);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    if (this.id === null) return;

    this.api.get(this.id).subscribe({
      next: (c) => {
        this.form.setValue({ name: c.name, description: c.description ?? '' });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Nie znaleziono kategorii.');
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { name, description } = this.form.getRawValue();
    const data = { name: name.trim(), description: description.trim() || null };

    this.saving.set(true);
    this.error.set(null);

    const request = this.id === null ? this.api.create(data) : this.api.update(this.id, data);
    request.subscribe({
      next: () => this.router.navigate(['/admin/categories']),
      error: (e) => {
        this.saving.set(false);
        this.error.set(requestErrorMessage(e, 'Nie udało się zapisać kategorii.'));
      },
    });
  }
}
