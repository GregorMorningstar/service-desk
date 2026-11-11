import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssetFailure } from './asset-failure';

describe('AssetFailure', () => {
  let component: AssetFailure;
  let fixture: ComponentFixture<AssetFailure>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetFailure],
    }).compileComponents();

    fixture = TestBed.createComponent(AssetFailure);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
