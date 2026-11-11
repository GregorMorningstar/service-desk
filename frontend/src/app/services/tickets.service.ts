import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  PagedResult,
  TicketListItem,
  TicketPriority,
  TicketStatus,
} from '../models';

export interface TicketListQuery {
  status?: TicketStatus;
  priority?: TicketPriority;
  categoryId?: number;
  /** Only tickets assigned to the caller; the backend enforces the id. */
  assignedToMe?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class TicketsService {
  private readonly http = inject(HttpClient);

  getList(query: TicketListQuery = {}): Observable<PagedResult<TicketListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') params = params.set(key, value);
    }
    return this.http.get<PagedResult<TicketListItem>>('/api/v1/tickets', { params });
  }

  /** The assigned technician (or an admin) finishes the work: describes the changes, optionally sets the next service date. */
  accept(ticketId: number, data: { resolution: string; nextInspectionDate: string | null }): Observable<void> {
    return this.http.post<void>(`/api/v1/tickets/${ticketId}/accept`, data);
  }

  /** Admin only. Pass null to unassign. */
  assign(ticketId: number, assigneeId: string | null): Observable<void> {
    return this.http.patch<void>(`/api/v1/tickets/${ticketId}/assignee`, { assigneeId });
  }
}
