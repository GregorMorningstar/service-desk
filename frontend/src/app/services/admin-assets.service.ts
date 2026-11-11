import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AssetDto, AssetFilters, SaveAsset } from '../models';

/** Admin-only device management under /api/v1/admin/assets. */
@Injectable({ providedIn: 'root' })
export class AdminAssetsService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/v1/admin/assets';

  getAll(filters: AssetFilters = {}): Observable<AssetDto[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') params = params.set(key, String(value));
    }
    return this.http.get<AssetDto[]>(this.url, { params });
  }

  get(id: number): Observable<AssetDto> {
    return this.http.get<AssetDto>(`${this.url}/${id}`);
  }

  create(data: SaveAsset): Observable<AssetDto> {
    return this.http.post<AssetDto>(this.url, data);
  }

  update(id: number, data: SaveAsset): Observable<AssetDto> {
    return this.http.put<AssetDto>(`${this.url}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  setActive(id: number, isActive: boolean): Observable<AssetDto> {
    return this.http.patch<AssetDto>(`${this.url}/${id}/active`, { isActive });
  }

  approve(id: number): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.url}/${id}/approve`, {});
  }

  reject(id: number): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.url}/${id}/reject`, {});
  }
}
