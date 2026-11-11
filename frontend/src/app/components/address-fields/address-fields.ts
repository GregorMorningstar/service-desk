import { Component, input } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Address } from '../../models';

/** Typed group of the address controls, shared by every form that has an address. */
export function createAddressGroup(fb: NonNullableFormBuilder) {
  return fb.group({
    street: ['', [Validators.required, Validators.maxLength(100)]],
    buildingNumber: ['', [Validators.required, Validators.maxLength(20)]],
    apartmentNumber: ['', Validators.maxLength(20)],
    postalCode: ['', [Validators.required, Validators.maxLength(12)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    country: ['Polska', [Validators.required, Validators.maxLength(100)]],
  });
}

export type AddressGroup = ReturnType<typeof createAddressGroup>;

export function addressFromGroup(group: AddressGroup): Address {
  const v = group.getRawValue();
  return { ...v, apartmentNumber: v.apartmentNumber.trim() || null };
}

export function patchAddress(group: AddressGroup, address: Address | null): void {
  if (address) group.setValue({ ...address, apartmentNumber: address.apartmentNumber ?? '' });
}

/** Address inputs for a group made by createAddressGroup. */
@Component({
  selector: 'app-address-fields',
  imports: [ReactiveFormsModule],
  styleUrl: './address-fields.css',
  templateUrl: './address-fields.html',
})
export class AddressFields {
  readonly group = input.required<AddressGroup>();
  /** Keeps element ids unique when the component is used twice on a page. */
  readonly idPrefix = input('addr');

  protected invalid(name: keyof AddressGroup['controls']): boolean {
    const c = this.group().controls[name];
    return c.touched && c.invalid;
  }
}
