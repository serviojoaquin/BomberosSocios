import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { UsersClient, UserDto, UpdateUserRolesCommand, CreateUserCommand, ResetUserPasswordCommand, SetUserEnabledCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { TranslateService } from '@ngx-translate/core';

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
    { data: 'userName', title: 'users.user', render: (value: string, type, row: UserDto) => type === 'display' ? this.escape(value) + (row.isEnabled ? '' : ' <small>' + this.translate.instant('users.disabled') + '</small>') : value },
    { data: 'roles', title: 'users.roles', render: (value: string[], type) => type === 'display' ? this.escape(value?.length ? value.join(', ') : this.translate.instant('common.none')) : (value ?? []).join(', ') }
  ];
  actions: DataTableAction[] = [
    { name: 'roles', label: 'users.editRoles', icon: 'pencil' },
    { name: 'password', label: 'users.resetPassword', icon: 'key' },
    { name: 'disable', label: 'users.disable', icon: 'ban', visible: (row: UserDto) => row.isEnabled && row.id !== this.currentUserId },
    { name: 'enable', label: 'users.enable', icon: 'circle-check', visible: (row: UserDto) => !row.isEnabled }
  ];
  selected = signal<UserDto | null>(null);
  rolesEditor: string[] = [];
  newUserRoles: string[] = [];
  newUserForm = this.fb.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  passwordForm = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6)]]
  });
  generalErrors = signal<string[]>([]);
  newUserOpen = signal(false);
  passwordOpen = signal(false);

  private currentUserId: string | null = null;

  constructor(
    private client: UsersClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {
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
    this.generalErrors.set([]);
    switch (event.action) {
      case 'roles':
        this.rolesEditor = [...user.roles];
        this.rolesDialogRef.nativeElement.showModal();
        break;
      case 'password':
        this.passwordForm.reset({ newPassword: '' });
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
    this.newUserForm.reset({ email: '', password: '' });
    this.newUserRoles = [];
    this.generalErrors.set([]);
    this.newUserOpen.set(true);
    this.newUserDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('email')?.focus(), 50);
  }

  closeDialogs(): void {
    for (const ref of [this.rolesDialogRef, this.newUserDialogRef, this.passwordDialogRef, this.enabledDialogRef]) {
      ref.nativeElement.close();
    }
    this.selected.set(null);
    this.generalErrors.set([]);
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
      error: error => this.generalErrors.set([this.formErrors.firstMessage(error, this.translate.instant('users.updateError'))])
    });
  }

  createUser(): void {
    this.newUserForm.markAllAsTouched();
    if (this.newUserForm.invalid) {
      return;
    }
    const command = { email: this.newUserForm.value.email, password: this.newUserForm.value.password, roles: this.newUserRoles } as CreateUserCommand;
    this.client.createUser(command).subscribe({
      next: id => {
        const created = { id, userName: command.email, email: command.email, isEnabled: true, roles: [...command.roles].sort() } as UserDto;
        this.users.update(us => [...(us ?? []), created]);
        this.closeDialogs();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.newUserForm).general)
    });
  }

  resetPassword(): void {
    this.passwordForm.markAllAsTouched();
    if (this.passwordForm.invalid) {
      return;
    }
    const command = { id: this.selected()!.id, newPassword: this.passwordForm.value.newPassword } as ResetUserPasswordCommand;
    this.client.resetUserPassword(command.id, command).subscribe({
      next: () => this.closeDialogs(),
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.passwordForm).general)
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
      error: error => this.generalErrors.set([this.formErrors.firstMessage(error, this.translate.instant('users.updateError'))])
    });
  }

  private patch(id: string, changes: Partial<UserDto>): void {
    this.users.update(us => us!.map(u => u.id === id ? { ...u, ...changes } as UserDto : u));
  }

  private escape(value: unknown): string {
    return String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
}
