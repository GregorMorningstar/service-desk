import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetFormValue } from '../../../components/asset-form/asset-form';
import { AssetsAddNew } from './assets-add-new';

describe('AssetsAddNew', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetsAddNew],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('shows the form without admin fields', async () => {
    const fixture = TestBed.createComponent(AssetsAddNew);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Dodaj sprzęt');
    expect(text).not.toContain('Administracja');
  });

  it('posts the device to the user endpoint', async () => {
    const fixture = TestBed.createComponent(AssetsAddNew);
    const http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();

    const value = {
      data: { name: 'Dell', categoryId: 2, serialNumber: 'S', inventoryNumber: 'M-1' },
      photos: [],
    } as unknown as AssetFormValue;
    (fixture.componentInstance as unknown as { save(v: AssetFormValue): void }).save(value);

    const req = http.expectOne((r) => r.method === 'POST' && r.url === '/api/v1/assets');
    expect(req.request.body.name).toBe('Dell');
  });
});
