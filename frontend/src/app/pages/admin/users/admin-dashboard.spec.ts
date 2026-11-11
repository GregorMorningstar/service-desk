import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AdminDashboard } from './admin-dashboard';

const user = (id: string, role: string) => ({ id, email: `${id}@x.pl`, firstName: id, lastName: 'T', roles: [role] });

describe('AdminDashboard', () => {
  it('offers only Moderator and Użytkownik, and no role change for administrators', async () => {
    await TestBed.configureTestingModule({
      imports: [AdminDashboard],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AdminDashboard);
    TestBed.inject(HttpTestingController)
      .expectOne((r) => r.url === '/api/v1/users')
      .flush([user('adm', 'Administrator'), user('mod', 'Moderator'), user('usr', 'User')]);
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    const rows = Array.from(el.querySelectorAll('tbody tr'));
    expect(rows.length).toBe(3);

    // administrator row: no select, no save button
    expect(rows[0].querySelector('select')).toBeNull();
    expect(rows[0].querySelector('button')).toBeNull();

    // the others can only be switched between Moderator and Użytkownik
    for (const row of rows.slice(1)) {
      const options = Array.from(row.querySelectorAll('option')).map((o) => o.textContent?.trim());
      expect(options).toEqual(['Moderator', 'Użytkownik']);
    }
  });
});
