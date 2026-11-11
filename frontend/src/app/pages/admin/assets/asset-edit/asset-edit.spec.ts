import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { AssetEdit } from './asset-edit';

function configure(id: string | null) {
  return TestBed.configureTestingModule({
    imports: [AssetEdit],
    providers: [
      provideRouter([]),
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } } },
    ],
  }).compileComponents();
}

describe('AssetEdit', () => {
  it('shows the add heading and the admin form for a new device', async () => {
    await configure(null);
    const fixture = TestBed.createComponent(AssetEdit);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Dodaj urządzenie');
    expect(text).toContain('Administracja');
  });

  it('loads the device when editing', async () => {
    await configure('4');
    const fixture = TestBed.createComponent(AssetEdit);
    TestBed.inject(HttpTestingController).expectOne('/api/v1/admin/assets/4').flush({
      id: 4,
      name: 'Dell',
      category: { id: 2, name: 'Monitor' },
      serialNumber: 'S',
      inventoryNumber: 'M-1',
      location: null,
      address: null,
      owner: null,
      inspectionIntervalMonths: 12,
      nextInspectionDate: null,
      description: null,
      isActive: true,
      approvalStatus: 'Approved',
    });
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Edycja urządzenia');
  });
});
