import { TestBed } from '@angular/core/testing';
import { NonNullableFormBuilder } from '@angular/forms';
import { AddressFields, addressFromGroup, createAddressGroup, patchAddress } from './address-fields';

describe('AddressFields', () => {
  it('renders six inputs bound to the group', async () => {
    await TestBed.configureTestingModule({ imports: [AddressFields] }).compileComponents();
    const group = createAddressGroup(TestBed.inject(NonNullableFormBuilder));

    const fixture = TestBed.createComponent(AddressFields);
    fixture.componentRef.setInput('group', group);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(6);
  });

  it('round-trips an address and turns an empty apartment into null', () => {
    const group = createAddressGroup(TestBed.inject(NonNullableFormBuilder));
    patchAddress(group, {
      street: 'Długa',
      buildingNumber: '1',
      apartmentNumber: null,
      postalCode: '00-001',
      city: 'Gdańsk',
      country: 'Polska',
    });

    expect(group.controls.apartmentNumber.value).toBe('');
    expect(addressFromGroup(group).apartmentNumber).toBeNull();
  });
});
