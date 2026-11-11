import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { SubNav } from './sub-nav';

describe('SubNav', () => {
  it('renders the title and one link per item', async () => {
    await TestBed.configureTestingModule({
      imports: [SubNav],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(SubNav);
    fixture.componentRef.setInput('title', 'Sprzęt');
    fixture.componentRef.setInput('items', [{ link: '/admin/assets/new', label: 'Dodaj', icon: faPlus }]);
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.nav-title')?.textContent).toBe('Sprzęt');
    expect(el.querySelectorAll('a.nav-item').length).toBe(1);
  });
});
