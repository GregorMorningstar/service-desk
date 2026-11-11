import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetAdminComponent } from './asset-admin-component';

describe('AssetAdminComponent', () => {
  it('shows the list and add links in the submenu', async () => {
    await TestBed.configureTestingModule({
      imports: [AssetAdminComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetAdminComponent);
    await fixture.whenStable();

    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('a.nav-item'));
    expect(links.map((l) => l.getAttribute('aria-label'))).toEqual(['Lista urządzeń', 'Dodaj urządzenie']);
  });
});
