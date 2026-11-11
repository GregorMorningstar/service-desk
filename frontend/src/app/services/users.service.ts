import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Role } from '../constants/roles';
import { UserDto, UserRef } from '../models';

/** Admin-only endpoints under /api/v1/users. */
@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>('/api/v1/users');
  }

  /** People a ticket can be assigned to (moderators and admins). */
  getTechnicians(): Observable<UserRef[]> {
    return this.http.get<UserRef[]>('/api/v1/users/technicians');
  }

  changeRole(userId: string, role: Role): Observable<UserDto> {
    return this.http.patch<UserDto>(`/api/v1/users/${userId}/role`, { role });
  }
}
