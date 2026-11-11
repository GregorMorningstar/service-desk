import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetForm, AssetFormValue } from './asset-form';

async function setup(admin: boolean) {
  await TestBed.configureTestingModule({
    imports: [AssetForm],
    providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
  }).compileComponents();

  const fixture = TestBed.createComponent(AssetForm);
  fixture.componentRef.setInput('admin', admin);
  fixture.componentRef.setInput('cancelLink', '/user/assets');
  const http = TestBed.inject(HttpTestingController);
  await fixture.whenStable();
  return { fixture, http, el: fixture.nativeElement as HTMLElement };
}

describe('AssetForm', () => {
  it('hides the admin section for a user and does not load users', async () => {
    const { http, el } = await setup(false);

    http.expectOne((r) => r.url.startsWith('/api/v1/asset-categories')).flush([]);
    http.expectNone('/api/v1/users');
    expect(el.textContent).not.toContain('Administracja');
  });

  it('shows the admin section with the user list', async () => {
    const { http, el } = await setup(true);

    http.expectOne((r) => r.url.startsWith('/api/v1/asset-categories')).flush([]);
    http.expectOne('/api/v1/users').flush([]);
    await Promise.resolve();
    expect(el.textContent).toContain('Administracja');
  });

  it('does not emit an invalid form and emits trimmed values when valid', async () => {
    const { fixture, http, el } = await setup(false);
    http.expectOne((r) => r.url.startsWith('/api/v1/asset-categories')).flush([{ id: 2, name: 'Monitor' }]);

    const saved: AssetFormValue[] = [];
    fixture.componentInstance.save.subscribe((v) => saved.push(v));

    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    expect(saved.length).toBe(0);

    patch(fixture, { name: ' Dell ', categoryId: 2, serialNumber: 'S1', inventoryNumber: 'M-1' });
    await settleAvailability(http, { serialTaken: false, inventoryTaken: false });
    el.querySelector('form')!.dispatchEvent(new Event('submit'));

    expect(saved.length).toBe(1);
    expect(saved[0].data.name).toBe('Dell');
    expect(saved[0].data.address).toBeNull();
    expect(saved[0].photos).toEqual([]);
  });

  it('fills the address fields from a suggested address', async () => {
    const { fixture, http, el } = await setup(false);
    http.expectOne((r) => r.url.startsWith('/api/v1/asset-categories')).flush([]);
    http.expectOne((r) => r.url === '/api/v1/assets/addresses').flush([
      {
        label: 'Adres użytkownika: Długa 1, 00-001 Gdańsk, Polska',
        address: { street: 'Długa', buildingNumber: '1', apartmentNumber: null, postalCode: '00-001', city: 'Gdańsk', country: 'Polska' },
      },
    ]);
    await fixture.whenStable();

    const select = el.querySelector('#a-known') as HTMLSelectElement;
    select.value = '0';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();

    expect((el.querySelector('#asset-street') as HTMLInputElement).value).toBe('Długa');
    expect((el.querySelector('#asset-city') as HTMLInputElement).value).toBe('Gdańsk');
  });

  it('blocks saving and explains when the serial number is already used', async () => {
    const { fixture, http, el } = await setup(false);
    http.expectOne((r) => r.url.startsWith('/api/v1/asset-categories')).flush([{ id: 2, name: 'Monitor' }]);

    const saved: AssetFormValue[] = [];
    fixture.componentInstance.save.subscribe((v) => saved.push(v));

    patch(fixture, { name: 'Dell', categoryId: 2, serialNumber: 'USED', inventoryNumber: 'M-2' });
    await settleAvailability(http, { serialTaken: true, inventoryTaken: false });
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(saved.length).toBe(0);
    expect(el.textContent).toContain('numerze seryjnym już istnieje');
  });
});

function patch(fixture: ComponentFixture<AssetForm>, value: object): void {
  (fixture.componentInstance as unknown as { form: { patchValue(v: object): void } }).form.patchValue(value);
}

/** Lets the debounced duplicate checks fire and answers them. */
async function settleAvailability(http: HttpTestingController, answer: object): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 450));
  http.match((r) => r.url === '/api/v1/assets/availability').forEach((req) => req.flush(answer));
  await new Promise((resolve) => setTimeout(resolve));
}
