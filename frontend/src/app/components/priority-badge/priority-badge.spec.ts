import { TestBed } from '@angular/core/testing';
import { TicketPriority } from '../../models';
import { PriorityBadge } from './priority-badge';

describe('PriorityBadge', () => {
  async function render(priority: TicketPriority) {
    await TestBed.configureTestingModule({ imports: [PriorityBadge] }).compileComponents();
    const fixture = TestBed.createComponent(PriorityBadge);
    fixture.componentRef.setInput('priority', priority);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows the label and one lit bar per level', async () => {
    const el = await render('High');
    expect(el.textContent).toContain('Wysoki');
    expect(el.querySelectorAll('i.on').length).toBe(3);
    expect(el.querySelector('.prio.high')).not.toBeNull();
  });

  it('lights all bars for a critical ticket', async () => {
    const el = await render('Critical');
    expect(el.querySelectorAll('i.on').length).toBe(4);
  });
});
