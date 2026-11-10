import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { UsersClient, UserDto, UpdateUserRolesCommand } from '../web-api-client';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

@Component({
  standalone: false,
  selector: 'app-users',
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;

  users = signal<UserDto[] | null>(null);
  availableRoles = signal<string[]>([]);
  columns: DataTableColumn[] = [
    { data: 'userName', title: 'User' },
    { data: 'roles', title: 'Roles', render: (value: string[]) => this.escape(value?.length ? value.join(', ') : 'None') }
  ];
  actions: DataTableAction[] = [
    { name: 'edit', label: 'Edit roles', icon: 'pencil' }
  ];
  editor: { id?: string, userName?: string, roles: string[] } = { roles: [] };
  editorError = signal('');

  constructor(private client: UsersClient) {}

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
    if (event.action === 'edit') {
      this.showEditDialog(event.row as UserDto);
    }
  }

  showEditDialog(user: UserDto): void {
    this.editor = { id: user.id, userName: user.userName, roles: [...user.roles] };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.editor = { roles: [] };
    this.editorError.set('');
  }

  hasRole(role: string): boolean {
    return this.editor.roles.includes(role);
  }

  toggleRole(role: string, checked: boolean): void {
    this.editor.roles = checked
      ? [...this.editor.roles, role]
      : this.editor.roles.filter(r => r !== role);
  }

  save(): void {
    const command = { id: this.editor.id, roles: this.editor.roles } as UpdateUserRolesCommand;
    this.client.updateUserRoles(command.id, command).subscribe({
      next: () => {
        this.users.update(us => us!.map(u => u.id === command.id
          ? { ...u, roles: [...command.roles].sort() } as UserDto
          : u));
        this.closeEditDialog();
      },
      error: error => {
        try {
          const errors = JSON.parse(error.response).errors;
          const first = errors && Object.values(errors)[0] as string[];
          this.editorError.set(first?.[0] ?? 'Unable to update the user.');
        } catch {
          this.editorError.set('Unable to update the user.');
        }
      }
    });
  }

  private escape(value: unknown): string {
    return String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
}
