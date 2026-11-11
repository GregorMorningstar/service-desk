import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AssetCategoryDto } from '../../models';
import { CategoryTile } from './category-tile';

const NO_TICKETS = { low: 0, medium: 0, high: 0, critical: 0 };
const CATEGORY: AssetCategoryDto = {
  id: 3,
  name: 'Laptop',
  description: 'Przenośne',
  assetCount: 2,
  activeTickets: NO_TICKETS,
  barcode: '3001000000003',
};

describe('CategoryTile', () => {
  let fixture: ComponentFixture<CategoryTile>;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryTile],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryTile);
    fixture.componentRef.setInput('category', CATEGORY);
    el = fixture.nativeElement;
  });

  it('has no links by default', async () => {
    await fixture.whenStable();
    expect(el.textContent).toContain('Laptop');
    expect(el.querySelector('a')).toBeNull();
  });

  it('opens the details and links the device count when allowed', async () => {
    fixture.componentRef.setInput('detailsLink', '/admin/categories');
    fixture.componentRef.setInput('assetsLink', '/admin/assets');
    await fixture.whenStable();

    expect(el.querySelector('a.name-link')?.getAttribute('href')).toBe('/admin/categories/3');
    expect(el.querySelector('a.count')?.getAttribute('href')).toBe('/admin/assets?categoryId=3');
  });

  it('does not link an empty category to the device list', async () => {
    fixture.componentRef.setInput('category', { ...CATEGORY, assetCount: 0 });
    fixture.componentRef.setInput('assetsLink', '/admin/assets');
    await fixture.whenStable();

    expect(el.querySelector('a.count')).toBeNull();
    expect(el.querySelector('.count.empty')).not.toBeNull();
  });

  it('shows no bubbles without open tickets', async () => {
    await fixture.whenStable();
    expect(el.querySelector('.bubble')).toBeNull();
  });

  it('shows a coloured bubble per priority, critical counted with high', async () => {
    fixture.componentRef.setInput('category', {
      ...CATEGORY,
      activeTickets: { low: 1, medium: 2, high: 3, critical: 1 },
    });
    await fixture.whenStable();

    const bubbles = Array.from(el.querySelectorAll('.bubble')).map((b) => [
      b.classList.contains('high') ? 'high' : b.classList.contains('medium') ? 'medium' : 'low',
      b.textContent?.trim(),
    ]);
    expect(bubbles).toEqual([
      ['high', '4'],
      ['medium', '2'],
      ['low', '1'],
    ]);
  });
});
