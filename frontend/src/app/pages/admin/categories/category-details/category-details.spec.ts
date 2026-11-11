import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { CategoryDetails } from './category-details';

describe('CategoryDetails', () => {
  let component: CategoryDetails;
  let fixture: ComponentFixture<CategoryDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryDetails],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
