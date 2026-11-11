import { Component, computed, input } from '@angular/core';
import { AssetDto } from '../../models';

/** Badge with the approval state of a device, or "Wyłączone" for an inactive approved one. */
@Component({
  selector: 'app-asset-status',
  styleUrl: './asset-status.css',
  templateUrl: './asset-status.html',
})
export class AssetStatus {
  readonly asset = input.required<AssetDto>();

  protected readonly view = computed(() => {
    const a = this.asset();
    if (a.approvalStatus === 'Pending') return { label: 'Oczekuje na zatwierdzenie', tone: 'warn' };
    if (a.approvalStatus === 'Rejected') return { label: 'Odrzucone', tone: 'bad' };
    return a.isActive ? { label: 'Aktywne', tone: 'ok' } : { label: 'Wyłączone', tone: 'off' };
  });
}
