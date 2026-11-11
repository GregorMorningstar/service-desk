import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AssetDto } from '../../../models';
import { ModeratorAssets } from './moderator-assets';

const asset = (id: number, name: string, city: string): AssetDto => ({
  id,
  name,
  category: { id: 1, name: 'Laptop' },
  serialNumber: 'S' + id,
  inventoryNumber: 'LAP-00' + id,
  location: null,
  address: { street: 'Długa', buildingNumber: '1', apartmentNumber: null, postalCode: '00-001', city, country: 'Polska' },
  owner: null,
  inspectionIntervalMonths: 12,
  nextInspectionDate: null,
  description: null,
  isActive: true,
  approvalStatus: 'Approved',
  barcode: '100100000000' + id,
  photoCount: 0,
});

describe('ModeratorAssets', () => {
  it('lists devices and filters them by text', async () => {
    await TestBed.configureTestingModule({
      imports: [ModeratorAssets],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(ModeratorAssets);
    TestBed.inject(HttpTestingController)
      .expectOne((r) => r.url === '/api/v1/assets')
      .flush([asset(1, 'Dell', 'Gdańsk'), asset(2, 'HP', 'Kraków')]);
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelectorAll('tbody tr').length).toBe(2);

    const input = el.querySelector('input.filter') as HTMLInputElement;
    input.value = 'krak';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(el.querySelectorAll('tbody tr').length).toBe(1);
    expect(el.textContent).toContain('HP');
  });
});
