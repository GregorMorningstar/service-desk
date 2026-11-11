import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetDto } from '../../models';
import { AssetActions } from './asset-actions';

const ASSET: AssetDto = {
  id: 7,
  name: 'Dell Latitude 5540',
  category: { id: 1, name: 'Laptop' },
  serialNumber: 'DL5540',
  inventoryNumber: 'LAP-001',
  location: null,
  address: null,
  owner: null,
  inspectionIntervalMonths: 12,
  nextInspectionDate: null,
  description: null,
  isActive: true,
  approvalStatus: 'Approved',
  barcode: '1001000000007',
  photoCount: 0,
};

describe('AssetActions', () => {
  let fixture: ComponentFixture<AssetActions>;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetActions],
      providers: [provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(AssetActions);
    fixture.componentRef.setInput('asset', ASSET);
    el = fixture.nativeElement;
  });

  it('shows all four actions, labelled with the device name', async () => {
    await fixture.whenStable();

    const items = Array.from(el.querySelectorAll<HTMLElement>('.icon-btn'));
    expect(items.map((b) => b.dataset['tooltip'])).toEqual([
      'Szczegóły',
      'Historia',
      'Przegląd',
      'Zgłoś awarię',
    ]);
    expect(items[0].getAttribute('aria-label')).toBe('Szczegóły: Dell Latitude 5540');
    expect(items[3].classList).toContain('danger');
  });

  it('has no links without a basePath', async () => {
    await fixture.whenStable();
    expect(el.querySelector('a')).toBeNull();
  });

  it('links the actions that have a page when a basePath is given', async () => {
    fixture.componentRef.setInput('basePath', '/user/assets');
    await fixture.whenStable();

    const hrefs = Array.from(el.querySelectorAll('a')).map((a) => a.getAttribute('href'));
    expect(hrefs).toEqual([
      '/user/assets/7',
      '/user/assets/7/history',
      '/user/assets/7/inspection',
      '/user/assets/7/failure',
    ]);
    expect(el.querySelector('button')).toBeNull();
  });

  it('shows only the requested actions', async () => {
    fixture.componentRef.setInput('actions', ['details', 'history']);
    await fixture.whenStable();

    expect(el.querySelectorAll('.icon-btn').length).toBe(2);
  });
});
