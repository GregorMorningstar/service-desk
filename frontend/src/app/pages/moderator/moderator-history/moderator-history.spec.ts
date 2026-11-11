import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ModeratorHistory } from './moderator-history';

const item = (id: number) => ({
  id,
  number: 'INC-00000' + id,
  title: 'Naprawa ' + id,
  status: 'Resolved',
  priority: 'Low',
  category: { id: 1, name: 'Sprzęt' },
  author: { id: 'u', email: 'u@x.pl', fullName: 'U' },
  assignee: { id: 'm', email: 'm@x.pl', fullName: 'M' },
  asset: { id: 1, name: 'Dell', inventoryNumber: 'LAP-001' },
  createdAt: '2026-11-10T08:00:00Z',
});

describe('ModeratorHistory', () => {
  async function setup() {
    await TestBed.configureTestingModule({
      imports: [ModeratorHistory],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(ModeratorHistory);
    return { fixture, http: TestBed.inject(HttpTestingController), el: fixture.nativeElement as HTMLElement };
  }

  it('loads only my tickets, page by page', async () => {
    const { fixture, http, el } = await setup();

    const first = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(first.request.params.get('assignedToMe')).toBe('true');
    expect(first.request.params.get('page')).toBe('1');
    first.flush({ items: [item(1)], page: 1, pageSize: 10, totalCount: 12, totalPages: 2 });
    await fixture.whenStable();
    expect(el.textContent).toContain('INC-000001');

    (Array.from(el.querySelectorAll('button')).find((b) => b.textContent?.includes('Następna')) as HTMLButtonElement).click();
    const second = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(second.request.params.get('page')).toBe('2');
    second.flush({ items: [item(2)], page: 2, pageSize: 10, totalCount: 12, totalPages: 2 });
  });

  it('filters by status and goes back to the first page', async () => {
    const { fixture, http, el } = await setup();
    http.expectOne((r) => r.url === '/api/v1/tickets').flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
    await fixture.whenStable();

    const select = el.querySelector('#status') as HTMLSelectElement;
    select.value = 'Resolved';
    select.dispatchEvent(new Event('change'));

    const req = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(req.request.params.get('status')).toBe('Resolved');
    expect(req.request.params.get('page')).toBe('1');
    req.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
  });
});
