import { TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

describe('Pagination', () => {
  async function setup(page: number, totalPages: number) {
    await TestBed.configureTestingModule({ imports: [Pagination] }).compileComponents();
    const fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    await fixture.whenStable();
    return fixture;
  }

  it('is hidden for a single page', async () => {
    const fixture = await setup(1, 1);
    expect((fixture.nativeElement as HTMLElement).querySelector('nav')).toBeNull();
  });

  it('emits the next and previous page and disables the edges', async () => {
    const fixture = await setup(2, 3);
    const pages: number[] = [];
    fixture.componentInstance.pageChange.subscribe((p) => pages.push(p));
    const [prev, next] = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'));

    prev.click();
    next.click();
    expect(pages).toEqual([1, 3]);
  });

  it('disables previous on the first page', async () => {
    const fixture = await setup(1, 3);
    const prev = (fixture.nativeElement as HTMLElement).querySelector('button') as HTMLButtonElement;
    expect(prev.disabled).toBe(true);
  });
});
