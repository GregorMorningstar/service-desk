import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TicketListItem } from '../../../models';
import { ModeratorAccept } from './moderator-accept';

const base = {
  title: 'Awaria',
  priority: 'High',
  category: { id: 1, name: 'Sprzęt' },
  author: { id: 'u', email: 'u@x.pl', fullName: 'U' },
  createdAt: '2026-11-10T08:00:00Z',
} as const;

const mine: TicketListItem = {
  ...base,
  id: 1,
  number: 'INC-000001',
  status: 'New',
  assignee: { id: 'me', email: 'm@x.pl', fullName: 'Mod' },
  asset: { id: 5, name: 'Dell', inventoryNumber: 'LAP-001' },
};
const done: TicketListItem = { ...mine, id: 3, number: 'INC-000003', status: 'Resolved' };

describe('ModeratorAccept', () => {
  async function setup() {
    await TestBed.configureTestingModule({
      imports: [ModeratorAccept],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ModeratorAccept);
    const http = TestBed.inject(HttpTestingController);
    const req = http.expectOne((r) => r.url === '/api/v1/tickets');
    expect(req.request.params.get('assignedToMe')).toBe('true');
    req.flush({ items: [mine, done], page: 1, pageSize: 100, totalCount: 2, totalPages: 1 });
    await fixture.whenStable();
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  }

  it('lists only unfinished tickets, asking the backend for my own', async () => {
    const { el } = await setup();
    const rows = el.querySelectorAll('tbody tr');

    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('INC-000001');
    expect(rows[0].textContent).toContain('LAP-001');
  });

  it('asks for the changes and sends them with the next service date', async () => {
    const { fixture, http, el } = await setup();

    (el.querySelector('tbody button') as HTMLButtonElement).click();
    await fixture.whenStable();
    expect(el.querySelector('#next-date')).not.toBeNull();

    const text = el.querySelector('#resolution') as HTMLTextAreaElement;
    text.value = 'Wymieniono zasilacz';
    text.dispatchEvent(new Event('input'));
    const date = el.querySelector('#next-date') as HTMLInputElement;
    date.value = '2027-06-01';
    date.dispatchEvent(new Event('input'));

    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    const req = http.expectOne('/api/v1/tickets/1/accept');
    expect(req.request.body).toEqual({ resolution: 'Wymieniono zasilacz', nextInspectionDate: '2027-06-01' });
    req.flush(null);
    await fixture.whenStable();

    expect(el.textContent).toContain('zlecenie zaakceptowane');
    expect(el.querySelectorAll('tbody tr').length).toBe(0);
  });

  it('does not send an empty description', async () => {
    const { fixture, http, el } = await setup();

    (el.querySelector('tbody button') as HTMLButtonElement).click();
    await fixture.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    http.expectNone('/api/v1/tickets/1/accept');
  });
});
