import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { UsersClient, UserDto, UpdateUserRolesCommand, CreateUserCommand, ResetUserPasswordCommand, SetUserEnabledCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

@Component({
  standalone: false,
  selector: 'app-users',
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  @ViewChild('rolesDialog') rolesDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('newUserDialog') newUserDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('passwordDialog') passwordDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('enabledDialog') enabledDialogRef: ElementRef<HTMLDialogElement>;

  users = signal<UserDto[] | null>(null);
  availableRoles = signal<string[]>([]);
  columns: DataTableColumn[] = [
    { data: 'userName', title: 'User', render: (value: string, type, row: UserDto) => type === 'display' ? this.escape(value) + (row.isEnabled ? '' : ' <small>(disabled)</small>') : value },
    { data: 'roles', title: 'Roles', render: (value: string[], type) => type === 'display' ? this.escape(value?.length ? value.join(', ') : 'None') : (value ?? []).join(', ') }
  ];
  actions: DataTableAction[] = [
    { name: 'roles', label: 'Edit roles', icon: 'pencil' },
    { name: 'password', label: 'Reset password', icon: 'key' },
    { name: 'disable', label: 'Disable', icon: 'ban', visible: (row: UserDto) => row.isEnabled && row.id !== this.currentUserId },
    { name: 'enable', label: 'Enable', icon: 'circle-check', visible: (row: UserDto) => !row.isEnabled }
  ];
  selected = signal<UserDto | null>(null);
  rolesEditor: string[] = [];
  newUser: { email?: string, password?: string, roles: string[] } = { roles: [] };
  newPassword = '';
  error = signal('');
  newUserOpen = signal(false);
  passwordOpen = signal(false);

  private currentUserId: string | null = null;

  constructor(private client: UsersClient, private authService: AuthService) {
    this.authService.currentUser$.subscribe(user => this.currentUserId = user?.id ?? null);
  }

  ngOnInit(): void {
    this.client.getUsers().subscribe({
      next: result => {
        this.availableRoles.set(result.availableRoles);
        this.users.set(result.users);
      },
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const user = event.row as UserDto;
    this.selected.set(user);
    this.error.set('');
    switch (event.action) {
      case 'roles':
        this.rolesEditor = [...user.roles];
        this.rolesDialogRef.nativeElement.showModal();
        break;
      case 'password':
        this.newPassword = '';
        this.passwordOpen.set(true);
        this.passwordDialogRef.nativeElement.showModal();
        setTimeout(() => document.getElementById('newPassword')?.focus(), 50);
        break;
      case 'disable':
      case 'enable':
        this.enabledDialogRef.nativeElement.showModal();
        break;
    }
  }

  showNewUserDialog(): void {
    this.newUser = { roles: [] };
    this.error.set('');
    this.newUserOpen.set(true);
    this.newUserDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('newUserEmail')?.focus(), 50);
  }

  closeDialogs(): void {
    for (const ref of [this.rolesDialogRef, this.newUserDialogRef, this.passwordDialogRef, this.enabledDialogRef]) {
      ref.nativeElement.close();
    }
    this.selected.set(null);
    this.error.set('');
    this.newUserOpen.set(false);
    this.passwordOpen.set(false);
  }

  hasRole(roles: string[], role: string): boolean {
    return roles.includes(role);
  }

  toggleRole(roles: string[], role: string, checked: boolean): string[] {
    return checked ? [...roles, role] : roles.filter(r => r !== role);
  }

  saveRoles(): void {
    const command = { id: this.selected()!.id, roles: this.rolesEditor } as UpdateUserRolesCommand;
    this.client.updateUserRoles(command.id, command).subscribe({
      next: () => {
        this.patch(command.id, { roles: [...command.roles].sort() });
        this.closeDialogs();
      },
      error: error => this.error.set(this.firstError(error, 'Unable to update the user.'))
    });
  }

  createUser(): void {
    const command = { email: this.newUser.email, password: this.newUser.password, roles: this.newUser.roles } as CreateUserCommand;
    this.client.createUser(command).subscribe({
      next: id => {
        const created = { id, userName: command.email, email: command.email, isEnabled: true, roles: [...command.roles].sort() } as UserDto;
        this.users.update(us => [...(us ?? []), created]);
        this.closeDialogs();
      },
      error: error => this.error.set(this.firstError(error, 'Unable to create the user.'))
    });
  }

  resetPassword(): void {
    const command = { id: this.selected()!.id, newPassword: this.newPassword } as ResetUserPasswordCommand;
    this.client.resetUserPassword(command.id, command).subscribe({
      next: () => this.closeDialogs(),
      error: error => this.error.set(this.firstError(error, 'Unable to reset the password.'))
    });
  }

  setEnabled(): void {
    const user = this.selected()!;
    const command = { id: user.id, enabled: !user.isEnabled } as SetUserEnabledCommand;
    this.client.setUserEnabled(command.id, command).subscribe({
      next: () => {
        this.patch(command.id, { isEnabled: command.enabled });
        this.closeDialogs();
      },
      error: error => this.error.set(this.firstError(error, 'Unable to update the user.'))
    });
  }

  private patch(id: string, changes: Partial<UserDto>): void {
    this.users.update(us => us!.map(u => u.id === id ? { ...u, ...changes } as UserDto : u));
  }

  private escape(value: unknown): string {
    return String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }

  private firstError(error: any, fallback: string): string {
    try {
      const errors = JSON.parse(error.response).errors;
      const first = errors && Object.values(errors)[0] as string[];
      return first?.[0] ?? fallback;
    } catch {
      return fallback;
    }
  }
}
