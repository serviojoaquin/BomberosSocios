import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { RolesClient, RoleDto, PermissionDto, CreateRoleCommand, UpdateRoleCommand } from '../web-api-client';
import { hasPermission } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';

@Component({
  standalone: false,
  selector: 'app-roles',
  templateUrl: './roles.component.html'
})
export class RolesComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;

  roles = signal<RoleDto[] | null>(null);
  availablePermissions = signal<PermissionDto[]>([]);
  columns: DataTableColumn[] = [
    { data: 'name', title: 'Name', render: (value, type, row: RoleDto) => type === 'display' ? this.escape(value) + (row.isSystem ? ' <small>(system)</small>' : '') : value },
    { data: 'permissions', title: 'Permissions', render: (_value, type, row: RoleDto) => type === 'display' ? this.escape(this.permissionNames(row)) : this.permissionNames(row) }
  ];
  actions: DataTableAction[] = [
    { name: 'edit', label: 'Edit', icon: 'pencil', visible: (row: RoleDto) => !this.isAdministrator(row) },
    { name: 'delete', label: 'Delete', icon: 'trash', visible: (row: RoleDto) => !row.isSystem }
  ];
  editor: { id?: string, name?: string, isSystem?: boolean, permissions: number } = { permissions: 0 };
  editorError = signal('');
  selected = signal<RoleDto | null>(null);

  constructor(private client: RolesClient) {}

  ngOnInit(): void {
    this.client.getRoles().subscribe({
      next: result => {
        this.availablePermissions.set(result.availablePermissions);
        this.roles.set(result.roles);
      },
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const role = event.row as RoleDto;
    if (event.action === 'edit') {
      this.showEditDialog(role);
    } else if (event.action === 'delete') {
      this.confirmDelete(role);
    }
  }

  has(granted: number, permission: number): boolean {
    return hasPermission(granted, permission);
  }

  permissionNames(role: RoleDto): string {
    if (role.permissions === -1) {
      return 'All';
    }
    return this.availablePermissions()
      .filter(p => hasPermission(role.permissions, p.id))
      .map(p => p.name)
      .join(', ') || 'None';
  }

  isAdministrator(role: RoleDto): boolean {
    return role.name === 'Administrator';
  }

  showNewDialog(): void {
    this.editor = { permissions: 0 };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('roleName')?.focus(), 50);
  }

  showEditDialog(role: RoleDto): void {
    this.editor = { id: role.id, name: role.name, isSystem: role.isSystem, permissions: role.permissions };
    this.editorError.set('');
    this.editDialogRef.nativeElement.showModal();
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.editor = { permissions: 0 };
    this.editorError.set('');
  }

  togglePermission(permission: number, checked: boolean): void {
    this.editor.permissions = checked
      ? this.editor.permissions | permission
      : this.editor.permissions & ~permission;
  }

  save(): void {
    if (this.editor.id) {
      this.update();
    } else {
      this.create();
    }
  }

  confirmDelete(role: RoleDto): void {
    this.selected.set(role);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteRole(id).subscribe({
      next: () => {
        this.roles.update(rs => rs!.filter(r => r.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.firstError(error, 'Unable to delete the role.'));
      }
    });
  }

  private create(): void {
    const command = { name: this.editor.name, permissions: this.editor.permissions } as CreateRoleCommand;
    this.client.createRole(command).subscribe({
      next: id => {
        const created = { id, name: command.name, permissions: command.permissions, isSystem: false } as RoleDto;
        this.roles.update(rs => [...rs!, created]);
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'Unable to save the role.'))
    });
  }

  private update(): void {
    const command = { id: this.editor.id, name: this.editor.name, permissions: this.editor.permissions } as UpdateRoleCommand;
    this.client.updateRole(command.id, command).subscribe({
      next: () => {
        this.roles.update(rs => rs!.map(r => r.id === command.id
          ? { ...r, name: command.name, permissions: command.permissions } as RoleDto
          : r));
        this.closeEditDialog();
      },
      error: error => this.editorError.set(this.firstError(error, 'Unable to save the role.'))
    });
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
