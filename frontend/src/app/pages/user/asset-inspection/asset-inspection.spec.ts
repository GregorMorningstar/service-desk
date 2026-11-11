import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssetInspection } from './asset-inspection';

describe('AssetInspection', () => {
  let component: AssetInspection;
  let fixture: ComponentFixture<AssetInspection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetInspection],
    }).compileComponents();

    fixture = TestBed.createComponent(AssetInspection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
