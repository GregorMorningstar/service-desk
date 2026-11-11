import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AssetPhotos } from './asset-photos';

describe('AssetPhotos', () => {
  async function setup(editable: boolean) {
    await TestBed.configureTestingModule({
      imports: [AssetPhotos],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetPhotos);
    fixture.componentRef.setInput('assetId', 5);
    fixture.componentRef.setInput('editable', editable);
    const http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  }

  it('says when a device has no photos', async () => {
    const { fixture, http, el } = await setup(false);

    http.expectOne('/api/v1/assets/5/photos').flush([]);
    await fixture.whenStable();

    expect(el.textContent).toContain('Brak zdjęć');
  });

  it('downloads each photo and offers delete only when editable', async () => {
    const { fixture, http, el } = await setup(true);

    http.expectOne('/api/v1/assets/5/photos').flush([
      { id: 9, originalName: 'a.png', contentType: 'image/png', size: 1, uploadedAt: '2026-11-11T10:00:00Z' },
    ]);
    http.expectOne('/api/v1/assets/5/photos/9').flush(new Blob(['x'], { type: 'image/png' }));
    await fixture.whenStable();

    expect(el.querySelectorAll('img').length).toBe(1);
    expect(el.querySelector('button.remove')).not.toBeNull();
  });
});
