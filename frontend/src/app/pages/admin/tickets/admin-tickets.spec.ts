import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PagedResult, TicketListItem, UserRef } from '../../../models';
import { AdminTickets } from './admin-tickets';

const tech: UserRef = { id: 't1', email: 'tech@demo.local', fullName: 'Tomasz Technik' };

const ticket: TicketListItem = {
  id: 1,
  number: 'INC-000001',
  title: 'Brak połączenia z Wi-Fi',
  status: 'New',
  priority: 'High',
  category: { id: 3, name: 'Sieć' },
  author: { id: 'u1', email: 'user@demo.local', fullName: 'Paweł Pracownik' },
  assignee: null,
  createdAt: '2026-11-10T08:00:00Z',
};

const page = (items: TicketListItem[]): PagedResult<TicketListItem> => ({
  items,
  page: 1,
  pageSize: 20,
  totalCount: items.length,
  totalPages: 1,
});

describe('AdminTickets', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminTickets],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function createLoaded(items: TicketListItem[]) {
    const fixture = TestBed.createComponent(AdminTickets);
    http.expectOne((r) => r.url === '/api/v1/tickets').flush(page(items));
    http.expectOne('/api/v1/users/technicians').flush([tech]);
    http.expectOne('/api/v1/ticket-categories').flush([{ id: 3, name: 'Sieć' }]);
    await fixture.whenStable();
    return fixture;
  }

  it('shows tickets from the API', async () => {
    const fixture = await createLoaded([ticket]);
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');

    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('INC-000001');
    expect(rows[0].textContent).toContain('Sieć');
  });

  it('shows an empty state', async () => {
    const fixture = await createLoaded([]);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Brak zgłoszeń');
  });

  it('sends the selected technician', async () => {
    const fixture = await createLoaded([ticket]);
    const select = (fixture.nativeElement as HTMLElement).querySelector('tbody select') as HTMLSelectElement;

    select.value = 't1';
    select.dispatchEvent(new Event('change'));

    const req = http.expectOne('/api/v1/tickets/1/assignee');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ assigneeId: 't1' });
    req.flush(null);

    await fixture.whenStable();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('przypisano do Tomasz Technik');
  });

  it('passes filters as query params', async () => {
    const fixture = await createLoaded([]);
    const status = (fixture.nativeElement as HTMLElement).querySelector('#f-status') as HTMLSelectElement;

    status.value = 'InProgress';
    status.dispatchEvent(new Event('change'));
    (fixture.nativeElement as HTMLElement).querySelector('form')!.dispatchEvent(new Event('submit'));

    const req = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(req.request.params.get('status')).toBe('InProgress');
    expect(req.request.params.get('page')).toBe('1');
    req.flush(page([]));
  });
});
