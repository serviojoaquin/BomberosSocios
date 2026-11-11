import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { RolesClient, RoleDto, PermissionDto, CreateRoleCommand, UpdateRoleCommand } from '../web-api-client';
import { hasPermission } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { TranslateService } from '@ngx-translate/core';

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
    { data: 'name', title: 'roles.name', render: (value, type, row: RoleDto) => type === 'display' ? this.escape(value) + (row.isSystem ? ' <small>' + this.translate.instant('roles.system') + '</small>' : '') : value },
    { data: 'permissions', title: 'roles.permissions', render: (_value, type, row: RoleDto) => type === 'display' ? this.escape(this.permissionNames(row)) : this.permissionNames(row) }
  ];
  actions: DataTableAction[] = [
    { name: 'edit', label: 'common.edit', icon: 'pencil', visible: (row: RoleDto) => !this.isAdministrator(row) },
    { name: 'delete', label: 'common.delete', icon: 'trash', visible: (row: RoleDto) => !row.isSystem }
  ];
  form = this.fb.group({
    id: [''],
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isSystem: [false],
    permissions: [0]
  });
  generalErrors = signal<string[]>([]);
  selected = signal<RoleDto | null>(null);

  constructor(
    private client: RolesClient,
    private fb: FormBuilder,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  get isEditing(): boolean {
    return !!this.form.value.id;
  }

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

  has(permission: number): boolean {
    return hasPermission(this.form.value.permissions ?? 0, permission);
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
    this.openEditor({ id: '', name: '', isSystem: false, permissions: 0 });
  }

  showEditDialog(role: RoleDto): void {
    this.openEditor({ id: role.id, name: role.name, isSystem: role.isSystem, permissions: role.permissions });
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.form.reset({ id: '', name: '', isSystem: false, permissions: 0 });
    this.form.get('name')!.enable();
    this.generalErrors.set([]);
  }

  togglePermission(permission: number, checked: boolean): void {
    const current = this.form.value.permissions ?? 0;
    this.form.patchValue({ permissions: checked ? current | permission : current & ~permission });
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }
    if (this.isEditing) {
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
        alert(this.formErrors.firstMessage(error, this.translate.instant('roles.deleteError')));
      }
    });
  }

  private openEditor(value: { id: string, name: string, isSystem: boolean, permissions: number }): void {
    this.form.reset(value);
    if (value.isSystem) {
      this.form.get('name')!.disable();
    } else {
      this.form.get('name')!.enable();
    }
    this.generalErrors.set([]);
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('name')?.focus(), 50);
  }

  private create(): void {
    const value = this.form.getRawValue();
    const command = { name: value.name, permissions: value.permissions } as CreateRoleCommand;
    this.client.createRole(command).subscribe({
      next: id => {
        const created = { id, name: command.name, permissions: command.permissions, isSystem: false } as RoleDto;
        this.roles.update(rs => [...(rs ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const value = this.form.getRawValue();
    const command = { id: value.id, name: value.name, permissions: value.permissions } as UpdateRoleCommand;
    this.client.updateRole(command.id, command).subscribe({
      next: () => {
        this.roles.update(rs => rs!.map(r => r.id === command.id
          ? { ...r, name: command.name, permissions: command.permissions } as RoleDto
          : r));
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private escape(value: unknown): string {
    return String(value ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
}
