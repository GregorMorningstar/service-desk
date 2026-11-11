import { computed, signal } from '@angular/core';

/**
 * Client-side paging of a list that is already loaded.
 * `items` is read inside computed signals, so pass a function that reads a signal.
 */
export function createPager<T>(items: () => T[], pageSize = 10) {
  const requested = signal(1);
  const totalPages = computed(() => Math.max(1, Math.ceil(items().length / pageSize)));
  // The list can shrink (filters, deletes), so never point past its end.
  const page = computed(() => Math.min(requested(), totalPages()));

  return {
    page,
    totalPages,
    items: computed(() => items().slice((page() - 1) * pageSize, page() * pageSize)),
    go: (target: number) => requested.set(Math.min(Math.max(1, target), totalPages())),
  };
}
