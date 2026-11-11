import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TicketListItem } from '../../models';
import { ModeratorDashboard } from './moderator-dashboard';

const ticket = (id: number, status: TicketListItem['status']): TicketListItem => ({
  id,
  number: 'INC-00000' + id,
  title: 'Awaria ' + id,
  status,
  priority: 'High',
  category: { id: 1, name: 'Sprzęt' },
  author: { id: 'u', email: 'u@x.pl', fullName: 'Paweł' },
  assignee: { id: 'm', email: 'm@x.pl', fullName: 'Mod' },
  createdAt: '2026-11-10T08:00:00Z',
});

describe('ModeratorDashboard', () => {
  it('asks only for tickets assigned to me and counts them', async () => {
    await TestBed.configureTestingModule({
      imports: [ModeratorDashboard],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(ModeratorDashboard);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/v1/dashboard/overview').flush({
      upcomingServices: [
        { assetId: 1, name: 'Dell', inventoryNumber: 'LAP-001', barcode: null, nextInspectionDate: '2026-11-20', daysLeft: 9, owner: null },
      ],
      openTickets: [ticket(1, 'New')],
    });
    const req = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(req.request.params.get('assignedToMe')).toBe('true');
    req.flush({ items: [ticket(1, 'New'), ticket(2, 'Resolved'), ticket(3, 'InProgress')], page: 1, pageSize: 100, totalCount: 3, totalPages: 1 });
    await fixture.whenStable();

    const values = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('.stat .value')).map((e) => e.textContent);
    expect(values).toEqual(['2', '1', '3']);

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('Najbliższe serwisy');
    expect(text).toContain('LAP-001');
    expect(text).toContain('Zgłoszenia do obsługi');
  });
});
