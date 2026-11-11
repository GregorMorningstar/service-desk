import { signal } from '@angular/core';
import { createPager } from './pager';

describe('createPager', () => {
  it('slices the list and clamps the page', () => {
    const list = signal([1, 2, 3, 4, 5]);
    const pager = createPager(() => list(), 2);

    expect(pager.totalPages()).toBe(3);
    expect(pager.items()).toEqual([1, 2]);

    pager.go(3);
    expect(pager.items()).toEqual([5]);

    pager.go(99);
    expect(pager.page()).toBe(3);
    pager.go(-4);
    expect(pager.page()).toBe(1);
  });

  it('moves back when the list shrinks below the current page', () => {
    const list = signal([1, 2, 3, 4, 5]);
    const pager = createPager(() => list(), 2);
    pager.go(3);

    list.set([1, 2]);
    expect(pager.page()).toBe(1);
    expect(pager.items()).toEqual([1, 2]);
  });

  it('always has at least one page', () => {
    const pager = createPager(() => [], 10);
    expect(pager.totalPages()).toBe(1);
    expect(pager.items()).toEqual([]);
  });
});
