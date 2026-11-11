import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { SociosClient, SocioDto, LookupDto, CreateSocioCommand, UpdateSocioCommand, UpdateDatosAfiliacionCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn, DataTableComponent } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-socios',
  templateUrl: './socios.component.html'
})
export class SociosComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('afiliacionDialog') afiliacionDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild(DataTableComponent) table: DataTableComponent;

  socios = signal<SocioDto[] | null>(null);
  categorias = signal<LookupDto[]>([]);
  cobradores = signal<LookupDto[]>([]);
  seleccionados = signal<SocioDto[]>([]);
  puedeCrear = this.authService.hasPermission(Permissions.CrearSocios);
  puedeEditar = this.authService.hasPermission(Permissions.EditarSocios);
  columns: DataTableColumn[] = [
    { data: 'id', title: 'N°', className: 'dt-body-right' },
    { data: 'nombre', title: 'Nombre' },
    { data: 'numeroDocumento', title: 'Documento', className: 'dt-body-right' },
    { data: 'categoria', title: 'Categoría' },
    { data: 'cobrador', title: 'Cobrador' },
    { data: 'domicilio', title: 'Domicilio' },
    { data: 'domicilioDeCobro', title: 'Domicilio de cobro' },
    { data: 'estado', title: 'Estado' }
  ];
  actions: DataTableAction[] = [
    ...(this.puedeEditar ? [{ name: 'edit', label: 'Modificar', icon: 'pencil' } as DataTableAction] : []),
    ...(this.authService.hasPermission(Permissions.EliminarSocios) ? [{ name: 'delete', label: 'Eliminar', icon: 'trash' } as DataTableAction] : [])
  ];
  form = this.fb.group({
    id: [0],
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    numeroDocumento: [null as number | null, [Validators.required, Validators.min(1)]],
    fechaAfiliacion: [this.hoy(), [Validators.required]],
    categoriaId: [null as number | null, [Validators.required]],
    cobradorId: [null as number | null, [Validators.required]],
    domicilio: ['', [Validators.required, Validators.maxLength(300)]],
    domicilioDeCobro: ['', [Validators.maxLength(300)]],
    telefono: ['', [Validators.maxLength(50)]],
    celular: ['', [Validators.maxLength(50)]],
    email: ['', [Validators.email, Validators.maxLength(200)]],
    web: ['', [Validators.maxLength(200)]],
    pasivo: [false]
  });
  afiliacionForm = this.fb.group({
    categoriaId: [null as number | null],
    cobradorId: [null as number | null],
    estado: ['' as '' | 'activo' | 'pasivo']
  });
  generalErrors = signal<string[]>([]);
  selected = signal<SocioDto | null>(null);

  constructor(
    private client: SociosClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService
  ) {}

  get isEditing(): boolean {
    return (this.form.value.id ?? 0) > 0;
  }

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.client.getSocios().subscribe({
      next: result => {
        this.categorias.set(result.categorias);
        this.cobradores.set(result.cobradores);
        this.socios.set(result.socios);
        this.seleccionados.set([]);
      },
      error: error => console.error(error)
    });
  }

  onAction(event: DataTableActionEvent): void {
    const socio = event.row as SocioDto;
    if (event.action === 'edit') {
      this.showEditDialog(socio);
    } else if (event.action === 'delete') {
      this.confirmDelete(socio);
    }
  }

  onSelectionChange(rows: any[]): void {
    this.seleccionados.set(rows as SocioDto[]);
  }

  showNewDialog(): void {
    const cobradores = this.cobradores();
    this.openEditor({
      id: 0,
      nombre: '',
      numeroDocumento: null,
      fechaAfiliacion: this.hoy(),
      categoriaId: null,
      cobradorId: cobradores.length === 1 ? cobradores[0].id : null,
      domicilio: '',
      domicilioDeCobro: '',
      telefono: '',
      celular: '',
      email: '',
      web: '',
      pasivo: false
    });
  }

  showEditDialog(socio: SocioDto): void {
    this.openEditor({
      id: socio.id,
      nombre: socio.nombre,
      numeroDocumento: socio.numeroDocumento,
      fechaAfiliacion: this.fecha(socio.fechaAfiliacion),
      categoriaId: socio.categoriaId,
      cobradorId: socio.cobradorId,
      domicilio: socio.domicilio,
      domicilioDeCobro: socio.domicilioDeCobro ?? '',
      telefono: socio.telefono ?? '',
      celular: socio.celular ?? '',
      email: socio.email ?? '',
      web: socio.web ?? '',
      pasivo: socio.pasivo
    });
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.generalErrors.set([]);
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

  showAfiliacionDialog(): void {
    this.afiliacionForm.reset({ categoriaId: null, cobradorId: null, estado: '' });
    this.generalErrors.set([]);
    this.afiliacionDialogRef.nativeElement.showModal();
  }

  closeAfiliacionDialog(): void {
    this.afiliacionDialogRef.nativeElement.close();
    this.generalErrors.set([]);
  }

  saveAfiliacion(): void {
    const v = this.afiliacionForm.value;
    const command = {
      sociosIds: this.seleccionados().map(s => s.id),
      categoriaId: v.categoriaId ? Number(v.categoriaId) : undefined,
      cobradorId: v.cobradorId ? Number(v.cobradorId) : undefined,
      pasivo: v.estado === '' ? undefined : v.estado === 'pasivo'
    } as UpdateDatosAfiliacionCommand;
    this.client.updateDatosAfiliacion(command).subscribe({
      next: () => {
        this.closeAfiliacionDialog();
        this.cargar();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.afiliacionForm).general)
    });
  }

  confirmDelete(socio: SocioDto): void {
    this.selected.set(socio);
    this.deleteDialogRef.nativeElement.showModal();
  }

  closeDeleteDialog(): void {
    this.deleteDialogRef.nativeElement.close();
    this.selected.set(null);
  }

  deleteConfirmed(): void {
    const id = this.selected()!.id;
    this.client.deleteSocio(id).subscribe({
      next: () => {
        this.socios.update(ss => ss!.filter(s => s.id !== id));
        this.closeDeleteDialog();
      },
      error: error => {
        this.closeDeleteDialog();
        alert(this.formErrors.firstMessage(error, 'No se pudo eliminar el socio.'));
      }
    });
  }

  private openEditor(value: any): void {
    this.form.reset(value);
    this.generalErrors.set([]);
    this.editDialogRef.nativeElement.showModal();
    setTimeout(() => document.getElementById('nombre')?.focus(), 50);
  }

  private payload() {
    const v = this.form.value;
    return {
      nombre: v.nombre,
      numeroDocumento: Number(v.numeroDocumento),
      fechaAfiliacion: v.fechaAfiliacion ? new Date(v.fechaAfiliacion + 'T00:00:00') : undefined,
      categoriaId: Number(v.categoriaId),
      cobradorId: Number(v.cobradorId),
      domicilio: v.domicilio,
      domicilioDeCobro: v.domicilioDeCobro || undefined,
      telefono: v.telefono || undefined,
      celular: v.celular || undefined,
      email: v.email || undefined,
      web: v.web || undefined,
      pasivo: !!v.pasivo
    };
  }

  private create(): void {
    const command = this.payload() as CreateSocioCommand;
    this.client.createSocio(command).subscribe({
      next: () => {
        this.closeEditDialog();
        this.cargar();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const command = { id: this.form.value.id, ...this.payload() } as UpdateSocioCommand;
    this.client.updateSocio(command.id, command).subscribe({
      next: () => {
        this.closeEditDialog();
        this.cargar();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private hoy(): string {
    return this.fecha(new Date());
  }

  private fecha(value: Date | string): string {
    const d = new Date(value);
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${mm}-${dd}`;
  }
}
