import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TicketListItem } from '../../models';
import { OpenTickets } from './open-tickets';

const ticket = (id: number, priority: TicketListItem['priority']): TicketListItem => ({
  id,
  number: 'INC-00000' + id,
  title: 'Awaria ' + id,
  status: 'New',
  priority,
  category: { id: 1, name: 'Sprzęt' },
  author: { id: 'u', email: 'u@x.pl', fullName: 'U' },
  assignee: null,
  createdAt: '2026-11-10T08:00:00Z',
});

describe('OpenTickets', () => {
  it('shows the priority level and an unassigned marker', async () => {
    await TestBed.configureTestingModule({ imports: [OpenTickets], providers: [provideRouter([])] }).compileComponents();
    const fixture = TestBed.createComponent(OpenTickets);
    fixture.componentRef.setInput('tickets', [ticket(1, 'Critical')]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('INC-000001');
    expect(text).toContain('Krytyczny');
    expect(text).toContain('brak');
  });

  it('says when nothing is open', async () => {
    await TestBed.configureTestingModule({ imports: [OpenTickets], providers: [provideRouter([])] }).compileComponents();
    const fixture = TestBed.createComponent(OpenTickets);
    fixture.componentRef.setInput('tickets', []);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Brak otwartych zgłoszeń');
  });
});
