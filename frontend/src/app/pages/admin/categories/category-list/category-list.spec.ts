import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { CategoryList } from './category-list';

describe('CategoryList', () => {
  let component: CategoryList;
  let fixture: ComponentFixture<CategoryList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryList],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('asks the backend for ?search= and pages 9 tiles at a time', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'cats', component: CategoryList }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    const http = TestBed.inject(HttpTestingController);
    const harness = await RouterTestingHarness.create('/cats?search=Lap');

    const many = Array.from({ length: 10 }, (_, i) => ({
      id: i + 1,
      name: `K${i + 1}`,
      description: null,
      assetCount: 0,
      activeTickets: { low: 0, medium: 0, high: 0, critical: 0 },
    }));
    http.expectOne((r) => r.urlWithParams === '/api/v1/asset-categories?search=Lap').flush(many);
    harness.detectChanges();

    const el = harness.routeNativeElement as HTMLElement;
    expect(el.querySelectorAll('.tile').length).toBe(9);
    expect(el.querySelectorAll('.page').length).toBe(4); // prev, 1, 2, next
  });
});
