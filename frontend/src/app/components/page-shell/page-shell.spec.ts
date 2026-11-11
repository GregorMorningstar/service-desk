import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PageShell } from './page-shell';

@Component({
  imports: [PageShell],
  template: `<app-page title="Sprzęt" subtitle="Lista" backLink="/x" backLabel="Wróć do x">
    <button actions>Dodaj</button>
    <p class="body">treść</p>
  </app-page>`,
})
class Host {}

describe('PageShell', () => {
  it('renders the title, back link, actions and content', async () => {
    await TestBed.configureTestingModule({ imports: [Host], providers: [provideRouter([])] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toBe('Sprzęt');
    expect(el.querySelector('a.back')?.textContent).toContain('Wróć do x');
    expect(el.querySelector('.actions button')?.textContent).toBe('Dodaj');
    expect(el.querySelector('.body')?.textContent).toBe('treść');
  });
});
