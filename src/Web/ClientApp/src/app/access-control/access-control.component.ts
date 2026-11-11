import { Component, OnInit, signal } from '@angular/core';
import { RolesClient, RoleDto, PermissionDto, UpdateRoleCommand } from '../web-api-client';
import { addFlag, deleteFlag, hasFlag } from '../shared/flag-enum';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  standalone: false,
  selector: 'app-access-control',
  templateUrl: './access-control.component.html'
})
export class AccessControlComponent implements OnInit {
  roles = signal<RoleDto[]>([]);
  permissions = signal<PermissionDto[]>([]);
  cargado = signal(false);

  constructor(
    private client: RolesClient,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.client.getRoles().subscribe({
      next: result => {
        this.roles.set(result.roles);
        this.permissions.set(result.availablePermissions);
        this.cargado.set(true);
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('accessControl.loadError')), 'danger')
    });
  }

  roleHasPermission(role: RoleDto, permission: PermissionDto): boolean {
    return hasFlag(role.permissions, permission.id);
  }

  esAdministrador(role: RoleDto): boolean {
    return role.name === 'Administrator';
  }

  setPermissionToRole(role: RoleDto, permission: PermissionDto, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const anterior = role.permissions;
    role.permissions = checked ? addFlag(role.permissions, permission.id) : deleteFlag(role.permissions, permission.id);
    this.client.updateRole(role.id, { id: role.id, name: role.name, permissions: role.permissions } as UpdateRoleCommand).subscribe({
      error: error => {
        role.permissions = anterior;
        (event.target as HTMLInputElement).checked = !checked;
        this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('accessControl.updateError')), 'danger');
      }
    });
  }
}
