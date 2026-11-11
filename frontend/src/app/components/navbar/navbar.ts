import { Component, ElementRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ROLE_LABELS } from '../../constants/roles';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  host: { '(document:click)': 'onDocumentClick($event)' },
})
export class Navbar {
  protected readonly auth = inject(AuthService);
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly mobileOpen = signal(false);
  protected readonly menuOpen = signal(false);

  protected readonly displayName = computed(() => {
    const u = this.auth.user();
    if (!u) return '';
    const full = `${u.firstName} ${u.lastName}`.trim();
    return full || u.email;
  });

  protected readonly initials = computed(() => {
    const u = this.auth.user();
    if (!u) return '';
    const fromName = `${u.firstName.charAt(0)}${u.lastName.charAt(0)}`.trim();
    return (fromName || u.email.charAt(0)).toUpperCase();
  });

  /** Label of the highest role */
  protected readonly roleLabel = computed(() => {
    if (this.auth.hasAnyRole(['Administrator'])) return ROLE_LABELS.Administrator;
    if (this.auth.hasAnyRole(['Moderator'])) return ROLE_LABELS.Moderator;
    return ROLE_LABELS.User;
  });

  constructor() {
    // Close menus after navigating
    inject(Router)
      .events.pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.closeAll());
  }

  protected toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  protected logout(): void {
    this.closeAll();
    this.auth.logout();
  }

  protected closeAll(): void {
    this.menuOpen.set(false);
    this.mobileOpen.set(false);
  }

  protected onDocumentClick(event: MouseEvent): void {
    if (!this.el.nativeElement.contains(event.target as Node)) {
      this.closeAll();
    }
  }
}
