import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AssetCategoryDto, CategoryTicket, SaveAssetCategory } from '../models';

/** /api/v1/asset-categories (read: any user, write: admin). */
@Injectable({ providedIn: 'root' })
export class AssetCategoriesService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/v1/asset-categories';

  /** Empty search returns every category. */
  getAll(search?: string): Observable<AssetCategoryDto[]> {
    let params = new HttpParams();
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<AssetCategoryDto[]>(this.url, { params });
  }

  get(id: number): Observable<AssetCategoryDto> {
    return this.http.get<AssetCategoryDto>(`${this.url}/${id}`);
  }

  /** Tickets of the devices in the category (staff only). */
  getTickets(id: number): Observable<CategoryTicket[]> {
    return this.http.get<CategoryTicket[]>(`${this.url}/${id}/tickets`);
  }

  create(data: SaveAssetCategory): Observable<AssetCategoryDto> {
    return this.http.post<AssetCategoryDto>(this.url, data);
  }

  update(id: number, data: SaveAssetCategory): Observable<AssetCategoryDto> {
    return this.http.put<AssetCategoryDto>(`${this.url}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
