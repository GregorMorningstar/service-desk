import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssetsHistory } from './assets-history';

describe('AssetsHistory', () => {
  let component: AssetsHistory;
  let fixture: ComponentFixture<AssetsHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetsHistory],
    }).compileComponents();

    fixture = TestBed.createComponent(AssetsHistory);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
