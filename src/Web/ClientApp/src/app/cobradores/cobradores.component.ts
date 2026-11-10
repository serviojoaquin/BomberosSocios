import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { CobradoresClient, CobradorDto, CreateCobradorCommand, UpdateCobradorCommand, CrearUsuarioCobradorCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-cobradores',
  templateUrl: './cobradores.component.html'
})
export class CobradoresComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('usuarioDialog') usuarioDialogRef: ElementRef<HTMLDialogElement>;

  cobradores = signal<CobradorDto[] | null>(null);
  puedeCrear = this.authService.hasPermission(Permissions.CrearCobradores);
  columns: DataTableColumn[] = [
    { data: 'apellido', title: 'Apellido' },
    { data: 'nombre', title: 'Nombre' },
    { data: 'telefono', title: 'Teléfono' },
    { data: 'celular', title: 'Celular' },
    { data: 'email', title: 'Email' },
    { data: 'comision', title: 'Comisión', className: 'dt-body-right', render: (value: number | null, type: string) => type === 'display' ? (value === null || value === undefined ? '' : `${value} %`) : (value ?? '') } as DataTableColumn,
    { data: 'tieneUsuario', title: 'Usuario', render: (value: boolean) => value ? 'Sí' : 'No' }
  ];
  actions: DataTableAction[] = [
    ...(this.authService.hasPermission(Permissions.EditarCobradores) ? [{ name: 'edit', label: 'Modificar', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.ManageSecurity) ? [{ name: 'usuario', label: 'Crear usuario', icon: 'key', visible: (row: CobradorDto) => !row.tieneUsuario } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.EliminarCobradores) ? [{ name: 'delete', label: 'Eliminar', icon: 'trash' } as DataTableAction] : [])
  ];
  form = this.fb.group({
    id: [0],
    apellido: ['', [Validators.required, Validators.maxLength(200)]],
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.email, Validators.maxLength(200)]],
    comision: [null as number | null, [Validators.min(0), Validators.max(100)]],
    telefono: ['', [Validators.maxLength(50)]],
    celular: ['', [Validators.maxLength(50)]]
  });
  usuarioForm = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  generalErrors = signal<string[]>([]);
  selected = signal<CobradorDto | null>(null);
  usuarioOpen = signal(false);

  constructor(
    private client: CobradoresClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService
  ) {}

  get isEditing(): boolean {
    return (this.form.value.id ?? 0) > 0;
  }

  ngOnInit(): void {
    this.client.getCobradores().subscribe({
      next: result => this.cobradores.set(result.cobradores),
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const cobrador = event.row as CobradorDto;
    if (event.action === 'edit') {
      this.showEditDialog(cobrador);
    } else if (event.action === 'delete') {
      this.confirmDelete(cobrador);
    } else if (event.action === 'usuario') {
      this.showUsuarioDialog(cobrador);
    }
  }

  showNewDialog(): void {
    this.openEditor({ id: 0, apellido: '', nombre: '', email: '', comision: null, telefono: '', celular: '' });
  }

  showEditDialog(cobrador: CobradorDto): void {
    this.openEditor({
      id: cobrador.id,
      apellido: cobrador.apellido,
      nombre: cobrador.nombre,
      email: cobrador.email ?? '',
      comision: cobrador.comision ?? null,
      telefono: cobrador.telefono ?? '',
      celular: cobrador.celular ?? ''
    });
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.generalErrors.set([]);
  }

  showUsuarioDialog(cobrador: CobradorDto): void {
    this.selected.set(cobrador);
    this.usuarioForm.reset({ password: '' });
    this.generalErrors.set(cobrador.email ? [] : ['El cobrador no tiene email cargado; es necesario para crear el usuario.']);
    this.usuarioOpen.set(true);
    this.usuarioDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('password')?.focus(), 50);
  }

  closeUsuarioDialog(): void {
    this.usuarioDialogRef.nativeElement.close();
    this.usuarioOpen.set(false);
    this.selected.set(null);
    this.generalErrors.set([]);
  }

  crearUsuario(): void {
    if (!this.selected()?.email) {
      return;
    }
    this.usuarioForm.markAllAsTouched();
    if (this.usuarioForm.invalid) {
      return;
    }
    const command = { id: this.selected()!.id, password: this.usuarioForm.value.password } as CrearUsuarioCobradorCommand;
    this.client.crearUsuarioCobrador(command.id, command).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.map(c => c.id === command.id ? { ...c, tieneUsuario: true } as CobradorDto : c));
        this.closeUsuarioDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.usuarioForm).general)
    });
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

  confirmDelete(cobrador: CobradorDto): void {
    this.selected.set(cobrador);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteCobrador(id).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.filter(c => c.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.formErrors.firstMessage(error, 'No se pudo eliminar el cobrador.'));
      }
    });
  }

  private openEditor(value: { id: number, apellido: string, nombre: string, email: string, comision: number | null, telefono: string, celular: string }): void {
    this.form.reset(value);
    this.generalErrors.set([]);
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('apellido')?.focus(), 50);
  }

  private payload() {
    const v = this.form.value;
    return {
      nombre: v.nombre,
      apellido: v.apellido,
      telefono: v.telefono || undefined,
      celular: v.celular || undefined,
      email: v.email || undefined,
      comision: v.comision === null || v.comision === undefined || (v.comision as any) === '' ? undefined : Number(v.comision)
    };
  }

  private create(): void {
    const command = this.payload() as CreateCobradorCommand;
    this.client.createCobrador(command).subscribe({
      next: id => {
        const created = { id, ...command, tieneUsuario: false } as CobradorDto;
        this.cobradores.update(cs => [...(cs ?? []), created]);
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const command = { id: this.form.value.id, ...this.payload() } as UpdateCobradorCommand;
    this.client.updateCobrador(command.id, command).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.map(c => c.id === command.id ? { ...c, ...command } as CobradorDto : c));
        this.closeEditDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }
}
