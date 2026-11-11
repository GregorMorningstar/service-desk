import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, concatMap, from, of, toArray } from 'rxjs';
import { AssetAvailability, AssetDto, AssetHistoryEntry, AssetPhoto, KnownAddress, RequestAsset } from '../models';

/** /api/v1/assets. Admins get every device, regular users only their own. */
@Injectable({ providedIn: 'root' })
export class AssetsService {
  private readonly http = inject(HttpClient);

  get(id: number): Observable<AssetDto> {
    return this.http.get<AssetDto>(`/api/v1/assets/${id}`);
  }

  /** Change log of a device, oldest first. */
  getHistory(id: number): Observable<AssetHistoryEntry[]> {
    return this.http.get<AssetHistoryEntry[]>(`/api/v1/assets/${id}/history`);
  }

  getAll(categoryId?: number | null): Observable<AssetDto[]> {
    let params = new HttpParams();
    if (categoryId != null) params = params.set('categoryId', categoryId);
    return this.http.get<AssetDto[]>('/api/v1/assets', { params });
  }

  /** Whether the serial / inventory number is already used by another device (exceptId = the one being edited). */
  checkAvailability(query: {
    serialNumber?: string;
    inventoryNumber?: string;
    exceptId?: number;
  }): Observable<AssetAvailability> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) params = params.set(key, String(value));
    }
    return this.http.get<AssetAvailability>('/api/v1/assets/availability', { params });
  }

  /** Addresses linked to a user (own + their devices) to pick from. Only staff may pass userId. */
  getKnownAddresses(userId?: string | null): Observable<KnownAddress[]> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId);
    return this.http.get<KnownAddress[]>('/api/v1/assets/addresses', { params });
  }

  getPhotos(id: number): Observable<AssetPhoto[]> {
    return this.http.get<AssetPhoto[]>(`/api/v1/assets/${id}/photos`);
  }

  uploadPhoto(id: number, file: File): Observable<AssetPhoto> {
    const body = new FormData();
    body.append('file', file);
    return this.http.post<AssetPhoto>(`/api/v1/assets/${id}/photos`, body);
  }

  /** Uploads the files one after another, so the backend's per-device limit is checked in order. */
  uploadPhotos(id: number, files: File[]): Observable<AssetPhoto[]> {
    if (files.length === 0) return of([]);
    return from(files).pipe(
      concatMap((f) => this.uploadPhoto(id, f)),
      toArray(),
    );
  }

  /** The image needs the bearer token, so it is fetched as a blob instead of being used as <img src>. */
  getPhotoBlob(id: number, photoId: number): Observable<Blob> {
    return this.http.get(`/api/v1/assets/${id}/photos/${photoId}`, { responseType: 'blob' });
  }

  deletePhoto(id: number, photoId: number): Observable<void> {
    return this.http.delete<void>(`/api/v1/assets/${id}/photos/${photoId}`);
  }

  /** A user proposes a device; it waits for an admin's approval. */
  request(data: RequestAsset): Observable<AssetDto> {
    return this.http.post<AssetDto>('/api/v1/assets', data);
  }
}
