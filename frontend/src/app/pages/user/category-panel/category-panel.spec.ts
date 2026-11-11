import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CategoryPanel } from './category-panel';

describe('CategoryPanel', () => {
  it('shows every category as a plain tile without links', async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryPanel],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(CategoryPanel);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/v1/asset-categories').flush([
      {
        id: 1,
        name: 'Laptop',
        description: null,
        assetCount: 2,
        activeTickets: { low: 0, medium: 0, high: 0, critical: 0 },
      },
    ]);
    await fixture.whenStable();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('Laptop');
    expect(el.querySelector('a')).toBeNull();
  });
});
