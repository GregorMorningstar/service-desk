import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetsPanel } from './assets-panel';

describe('AssetsPanel', () => {
  it('lists the devices returned by the API', async () => {
    await TestBed.configureTestingModule({
      imports: [AssetsPanel],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetsPanel);
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/v1/assets').flush([
      {
        id: 1,
        name: 'Dell Latitude 5540',
        category: { id: 1, name: 'Laptop' },
        serialNumber: 'DL5540',
        inventoryNumber: 'LAP-001',
        location: 'Pokój 2.14',
        owner: null,
        inspectionIntervalMonths: 12,
        nextInspectionDate: '2000-01-15',
      },
    ]);
    await fixture.whenStable();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('LAP-001');
    expect(el.textContent).toContain('15.01.2000');
    expect(el.querySelector('.overdue')).not.toBeNull(); // the date is in the past
  });
});
