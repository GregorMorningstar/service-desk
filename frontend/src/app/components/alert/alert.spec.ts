import { TestBed } from '@angular/core/testing';
import { Alert } from './alert';

describe('Alert', () => {
  it('shows an error and hides when the message is empty', async () => {
    await TestBed.configureTestingModule({ imports: [Alert] }).compileComponents();
    const fixture = TestBed.createComponent(Alert);
    const el = fixture.nativeElement as HTMLElement;

    fixture.componentRef.setInput('message', 'Błąd');
    await fixture.whenStable();
    expect(el.querySelector('p.alert-error')?.textContent).toBe('Błąd');
    expect(el.querySelector('[role=alert]')).not.toBeNull();

    fixture.componentRef.setInput('message', null);
    await fixture.whenStable();
    expect(el.querySelector('p')).toBeNull();
  });

  it('uses the success style', async () => {
    await TestBed.configureTestingModule({ imports: [Alert] }).compileComponents();
    const fixture = TestBed.createComponent(Alert);
    fixture.componentRef.setInput('message', 'OK');
    fixture.componentRef.setInput('kind', 'success');
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('p.alert-success')).not.toBeNull();
  });
});
