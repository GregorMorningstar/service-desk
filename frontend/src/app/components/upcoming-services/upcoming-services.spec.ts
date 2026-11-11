import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { UpcomingService } from '../../models';
import { UpcomingServices } from './upcoming-services';

const svc = (id: number, daysLeft: number): UpcomingService => ({
  assetId: id,
  name: 'Sprzęt ' + id,
  inventoryNumber: 'LAP-00' + id,
  barcode: null,
  nextInspectionDate: '2026-11-20',
  daysLeft,
  owner: null,
});

describe('UpcomingServices', () => {
  async function render(services: UpcomingService[], linkToEdit = false) {
    await TestBed.configureTestingModule({ imports: [UpcomingServices], providers: [provideRouter([])] }).compileComponents();
    const fixture = TestBed.createComponent(UpcomingServices);
    fixture.componentRef.setInput('services', services);
    fixture.componentRef.setInput('linkToEdit', linkToEdit);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('describes the time left and marks the overdue ones', async () => {
    const el = await render([svc(1, -3), svc(2, 0), svc(3, 1), svc(4, 30)]);
    const text = el.textContent!;

    expect(text).toContain('po terminie o 3 dni');
    expect(text).toContain('dziś');
    expect(text).toContain('za 1 dzień');
    expect(el.querySelector('.when.late')).not.toBeNull();
    expect(el.querySelector('.when.ok')).not.toBeNull();
  });

  it('links to the edit page only when asked to', async () => {
    expect((await render([svc(1, 5)], true)).querySelector('tbody a')).not.toBeNull();
    TestBed.resetTestingModule();
    expect((await render([svc(1, 5)], false)).querySelector('tbody a')).toBeNull();
  });

  it('shows 5 per page', async () => {
    const el = await render(Array.from({ length: 7 }, (_, i) => svc(i + 1, i)));
    expect(el.querySelectorAll('tbody tr').length).toBe(5);
    expect(el.textContent).toContain('Strona 1 z 2');
  });
});
