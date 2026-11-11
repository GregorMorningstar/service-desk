import { TestBed } from '@angular/core/testing';
import { AssetDto } from '../../models';
import { AssetStatus } from './asset-status';

const base = { isActive: true, approvalStatus: 'Approved' } as AssetDto;

async function render(asset: Partial<AssetDto>): Promise<string> {
  await TestBed.configureTestingModule({ imports: [AssetStatus] }).compileComponents();
  const fixture = TestBed.createComponent(AssetStatus);
  fixture.componentRef.setInput('asset', { ...base, ...asset });
  await fixture.whenStable();
  return (fixture.nativeElement as HTMLElement).textContent!.trim();
}

describe('AssetStatus', () => {
  it('shows pending approval', async () => {
    expect(await render({ approvalStatus: 'Pending', isActive: false })).toBe('Oczekuje na zatwierdzenie');
  });

  it('shows active and switched-off devices', async () => {
    expect(await render({})).toBe('Aktywne');
    TestBed.resetTestingModule();
    expect(await render({ isActive: false })).toBe('Wyłączone');
  });
});
