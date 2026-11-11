import { TestBed } from '@angular/core/testing';
import { Barcode, ean13Modules } from './barcode';

describe('ean13Modules', () => {
  it('encodes 4006381333931 with the LGLLGG parity of the leading 4', () => {
    const expected =
      '101' +
      ['0001101', '0100111', '0101111', '0111101', '0001001', '0110011'].join('') + // 0 0 6 3 8 1
      '01010' +
      ['1000010', '1000010', '1000010', '1110100', '1000010', '1100110'].join('') + // 3 3 3 9 3 1
      '101';
    expect(ean13Modules('4006381333931')).toBe(expected);
  });

  it('is always 95 modules long and starts and ends with a guard', () => {
    const modules = ean13Modules('1001000000042')!;
    expect(modules.length).toBe(95);
    expect(modules.startsWith('101')).toBe(true);
    expect(modules.endsWith('101')).toBe(true);
    expect(modules.slice(45, 50)).toBe('01010');
  });

  it('rejects values that are not 13 digits', () => {
    expect(ean13Modules('123')).toBeNull();
    expect(ean13Modules('100100000004a')).toBeNull();
  });
});

describe('Barcode', () => {
  it('draws bars for a valid value and nothing for an invalid one', async () => {
    await TestBed.configureTestingModule({ imports: [Barcode] }).compileComponents();

    const fixture = TestBed.createComponent(Barcode);
    fixture.componentRef.setInput('value', '1001000000042');
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelectorAll('svg rect').length).toBeGreaterThan(10);

    fixture.componentRef.setInput('value', null);
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('svg')).toBeNull();
  });
});
