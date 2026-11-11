import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ModeratorLayout } from './moderator-layout';

describe('ModeratorLayout', () => {
  it('shows the panel, acceptance, tickets and devices links', async () => {
    await TestBed.configureTestingModule({
      imports: [ModeratorLayout],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(ModeratorLayout);
    await fixture.whenStable();

    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('a.nav-item'));
    expect(links.map((l) => l.getAttribute('aria-label'))).toEqual(['Panel', 'Do akceptacji', 'Zgłoszenia', 'Sprzęt']);
  });
});
