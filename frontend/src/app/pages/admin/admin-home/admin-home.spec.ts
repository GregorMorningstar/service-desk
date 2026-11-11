import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AdminHome } from './admin-home';

describe('AdminHome', () => {
  it('shows counters, the nearest services and the urgent tickets', async () => {
    await TestBed.configureTestingModule({
      imports: [AdminHome],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AdminHome);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/v1/dashboard/stats').flush({ total: 8, open: 5, unassignedOpen: 2 });
    http.expectOne('/api/v1/dashboard/overview').flush({
      upcomingServices: [
        { assetId: 1, name: 'Dell', inventoryNumber: 'LAP-001', barcode: null, nextInspectionDate: '2026-11-01', daysLeft: -3, owner: null },
        { assetId: 2, name: 'HP', inventoryNumber: 'PRN-001', barcode: null, nextInspectionDate: '2026-11-20', daysLeft: 9, owner: null },
      ],
      openTickets: [
        {
          id: 1, number: 'INC-000001', title: 'Awaria', status: 'New', priority: 'Critical',
          category: { id: 1, name: 'Sprzęt' }, author: { id: 'u', email: 'u@x.pl', fullName: 'U' },
          assignee: null, createdAt: '2026-11-10T08:00:00Z',
        },
      ],
    });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('Wszystkie zgłoszenia');
    expect(text).toContain('po terminie o 3 dni');
    expect(text).toContain('za 9 dni');
    expect(text).toContain('INC-000001');
    expect(text).toContain('Krytyczny');
  });
});
