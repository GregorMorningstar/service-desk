import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { ROLES, ROLE_LABELS, Role } from '../../../constants/roles';
import { UserDto } from '../../../models';
import { UsersService } from '../../../services/users.service';
import { Pagination } from '../../../components/pagination/pagination';
import { createPager } from '../../../utils/pager';

@Component({
  selector: 'app-admin-dashboard',
  imports: [Pagination],
  templateUrl: './admin-dashboard.html',
  host: { class: 'container' },
})
export class AdminDashboard {
  private readonly usersApi = inject(UsersService);
  protected readonly auth = inject(AuthService);

  protected readonly roles = ROLES;
  /** Roles an admin can hand out; administrators are fixed. */
  protected readonly assignableRoles: Role[] = ['Moderator', 'User'];
  protected readonly roleLabels = ROLE_LABELS;

  protected readonly users = signal<UserDto[]>([]);
  protected readonly pager = createPager(() => this.users(), 10);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly savingId = signal<string | null>(null);
  /** Roles picked in the selects but not saved yet: userId -> role */
  protected readonly pending = signal<Record<string, Role>>({});

  protected readonly stats = computed(() =>
    ROLES.map((role) => ({
      role,
      label: ROLE_LABELS[role],
      count: this.users().filter((u) => u.roles.includes(role)).length,
    })),
  );

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.usersApi.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Nie udało się pobrać listy użytkowników.');
        this.loading.set(false);
      },
    });
  }

  protected currentRole(user: UserDto): Role {
    return user.roles[0] ?? 'User';
  }

  protected selectedRole(user: UserDto): Role {
    return this.pending()[user.id] ?? this.currentRole(user);
  }

  protected isChanged(user: UserDto): boolean {
    return this.selectedRole(user) !== this.currentRole(user);
  }

  protected isAdmin(user: UserDto): boolean {
    return user.roles.includes('Administrator');
  }

  protected isMe(user: UserDto): boolean {
    return user.id === this.auth.user()?.id;
  }

  protected select(user: UserDto, role: string): void {
    this.pending.update((p) => ({ ...p, [user.id]: role as Role }));
  }

  protected save(user: UserDto): void {
    const role = this.selectedRole(user);
    this.savingId.set(user.id);
    this.error.set(null);
    this.success.set(null);

    this.usersApi.changeRole(user.id, role).subscribe({
      next: (updated) => {
        this.users.update((list) => list.map((u) => (u.id === updated.id ? updated : u)));
        this.pending.update(({ [user.id]: _, ...rest }) => rest);
        this.savingId.set(null);
        this.success.set(`Zmieniono rolę ${updated.email} na „${ROLE_LABELS[role]}”.`);
      },
      error: (e: HttpErrorResponse) => {
        this.savingId.set(null);
        this.error.set(e.error?.detail ?? 'Nie udało się zmienić roli.');
      },
    });
  }
}
