import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { AssetHistory } from './asset-history';

describe('AssetHistory', () => {
  it('shows the change log newest first with readable field names', async () => {
    await TestBed.configureTestingModule({
      imports: [AssetHistory],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '3' }) } } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetHistory);
    const http = TestBed.inject(HttpTestingController);
    const user = { id: 'u1', email: 'a@b.pl', fullName: 'Anna Admin' };

    http.expectOne('/api/v1/assets/3').flush({
      id: 3,
      name: 'HP LaserJet',
      inventoryNumber: 'PRN-001',
      category: { id: 3, name: 'Drukarka' },
    });
    http.expectOne('/api/v1/assets/3/history').flush([
      { id: 1, field: 'Created', oldValue: null, newValue: 'PRN-001', changedBy: user, changedAt: '2026-11-10T10:00:00Z' },
      { id: 2, field: 'Owner', oldValue: null, newValue: 'user@test.pl', changedBy: user, changedAt: '2026-11-11T10:00:00Z' },
    ]);
    await fixture.whenStable();

    const rows = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr'));
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Właściciel'); // newest first
    expect(rows[1].textContent).toContain('Dodano urządzenie');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('PRN-001 · HP LaserJet');
  });
});
