import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { TicketCategory } from '../models';

/** GET /api/v1/ticket-categories (any logged-in user). */
@Injectable({ providedIn: 'root' })
export class TicketCategoriesService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<TicketCategory[]> {
    return this.http.get<TicketCategory[]>('/api/v1/ticket-categories');
  }
}
