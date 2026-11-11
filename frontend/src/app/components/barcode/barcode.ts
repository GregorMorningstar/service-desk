import { Component, computed, input } from '@angular/core';

// EAN-13 bit patterns of the digits 0-9 (7 modules each). R is the complement of L.
const L = ['0001101', '0011001', '0010011', '0111101', '0100011', '0110001', '0101111', '0111011', '0110111', '0001011'];
const G = ['0100111', '0110011', '0011011', '0100001', '0011101', '0111001', '0000101', '0010001', '0001001', '0010111'];
const R = L.map((code) => [...code].map((bit) => (bit === '0' ? '1' : '0')).join(''));

// Which of L / G encodes digits 2-7, chosen by the first digit.
const PARITY = ['LLLLLL', 'LLGLGG', 'LLGGLG', 'LLGGGL', 'LGLLGG', 'LGGLLG', 'LGGGLL', 'LGLGLG', 'LGLGGL', 'LGGLGL'];

const QUIET = 9; // empty modules on each side

/** Module string (1 = bar) of a 13-digit EAN code, or null when the value is not 13 digits. */
export function ean13Modules(value: string): string | null {
  if (!/^\d{13}$/.test(value)) return null;

  const digits = [...value].map(Number);
  const parity = PARITY[digits[0]];
  const left = digits.slice(1, 7).map((d, i) => (parity[i] === 'L' ? L[d] : G[d])).join('');
  const right = digits.slice(7).map((d) => R[d]).join('');
  return `101${left}01010${right}101`;
}

/** EAN-13 barcode drawn as an SVG, with the digits underneath. */
@Component({
  selector: 'app-barcode',
  styleUrl: './barcode.css',
  templateUrl: './barcode.html',
})
export class Barcode {
  /** 13 digits. */
  readonly value = input.required<string | null | undefined>();

  protected readonly width = 95 + 2 * QUIET;

  /** Runs of black modules as x / width pairs. */
  protected readonly bars = computed(() => {
    const modules = ean13Modules(this.value() ?? '');
    if (!modules) return [];

    const bars: { x: number; w: number }[] = [];
    for (let i = 0; i < modules.length; i++) {
      if (modules[i] !== '1') continue;
      const last = bars[bars.length - 1];
      if (last && last.x + last.w === i + QUIET) last.w++;
      else bars.push({ x: i + QUIET, w: 1 });
    }
    return bars;
  });

  /** Digits split like on a printed code: 1 + 6 + 6. */
  protected readonly groups = computed(() => {
    const v = this.value() ?? '';
    return { first: v.slice(0, 1), left: v.slice(1, 7), right: v.slice(7) };
  });
}
