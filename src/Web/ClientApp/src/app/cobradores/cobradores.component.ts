import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CobradoresClient, CobradorDto, CreateCobradorCommand, UpdateCobradorCommand, CrearUsuarioCobradorCommand, CuotasClient, ReportesClient, GenerarCuotasCommand, BonificarCuotasViejasCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn, DataTableMenuItem } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { YearSelectorComponent, YearSelectorResult } from '../shared/year-selector/year-selector.component';
import { ToastService } from '../shared/toast/toast.service';
import { saveBlob } from '../shared/download';

@Component({
  standalone: false,
  selector: 'app-cobradores',
  templateUrl: './cobradores.component.html'
})
export class CobradoresComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('deleteDialog') deleteDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('usuarioDialog') usuarioDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild(YearSelectorComponent) yearSelector: YearSelectorComponent;

  cobradores = signal<CobradorDto[] | null>(null);
  initialYear = new Date().getFullYear();
  puedeCrear = this.authService.hasPermission(Permissions.CrearCobradores);
  puedeEditar = this.authService.hasPermission(Permissions.EditarCobradores);
  puedeAdministrarCuotas = this.authService.hasPermission(Permissions.AdministrarCuotas);
  soloLectura = signal(false);
  columns: DataTableColumn[] = [
    { data: 'id', title: 'Id', className: 'dt-body-right' },
    { data: 'nombre', title: 'Nombre' },
    { data: 'apellido', title: 'Apellido' },
    { data: 'telefono', title: 'Teléfono' },
    { data: 'celular', title: 'Celular' },
    { data: 'email', title: 'Email' },
    { data: 'sociosCount', title: 'Socios', className: 'dt-body-right' },
    { data: 'tieneUsuario', title: 'Usuario', render: (value: boolean) => value ? 'Si' : 'No' }
  ];
  actions: DataTableAction[] = [
    {
      name: 'cuotas',
      label: 'Cuotas',
      icon: 'tasks',
      visible: (row: CobradorDto) => row.sociosCount > 0,
      menu: (row: CobradorDto) => this.menuCuotas(row)
    },
    ...(this.authService.hasPermission(Permissions.ManageSecurity) ? [{ name: 'usuario', label: 'Crear Usuario', icon: 'user-plus', visible: (row: CobradorDto) => !!row.email && !row.tieneUsuario } as DataTableAction] : []),
    this.puedeEditar
      ? { name: 'edit', label: 'Editar', icon: 'pencil' }
      : { name: 'ver', label: 'Ver', icon: 'eye' },
    ...(this.authService.hasPermission(Permissions.EliminarCobradores) ? [{ name: 'delete', label: 'Eliminar', icon: 'trash', visible: (row: CobradorDto) => row.cuotasCobradasCount === 0, disabled: (row: CobradorDto) => row.sociosCount > 0 } as DataTableAction] : [])
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

  private pendingYearAction: ((result: YearSelectorResult) => void) | null = null;

  constructor(
    private client: CobradoresClient,
    private cuotasClient: CuotasClient,
    private reportesClient: ReportesClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private formErrors: FormErrorsService,
    private router: Router,
    private toast: ToastService
  ) {}

  get isEditing(): boolean {
    return (this.form.value.id ?? 0) > 0;
  }

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.client.getCobradores().subscribe({
      next: result => {
        this.initialYear = result.initialYear;
        this.cobradores.set(result.cobradores);
      },
      error: () => this.toast.show('Error al cargar los cobradores.', 'danger')
    });
  }

  onAction(event: DataTableActionEvent): void {
    const cobrador = event.row as CobradorDto;
    const action = event.action;
    if (action.startsWith('generar-')) {
      this.openYearSelectorForGenerate(cobrador, Number(action.substring('generar-'.length)));
      return;
    }
    switch (action) {
      case 'cobradas':
        this.openYearSelectorForCobradas(cobrador);
        break;
      case 'impagas':
        this.openYearSelectorForImpagas(cobrador);
        break;
      case 'bonificar':
        this.confirmSetOldCuotasAsBonificadas(cobrador);
        break;
      case 'descargar':
        this.openYearSelectorForDownload(cobrador);
        break;
      case 'edit':
        this.showEditDialog(cobrador, false);
        break;
      case 'ver':
        this.showEditDialog(cobrador, true);
        break;
      case 'delete':
        this.confirmDelete(cobrador);
        break;
      case 'usuario':
        this.showUsuarioDialog(cobrador);
        break;
    }
  }

  onYearSelected(result: YearSelectorResult): void {
    const action = this.pendingYearAction;
    this.pendingYearAction = null;
    action?.(result);
  }

  private menuCuotas(row: CobradorDto): DataTableMenuItem[] {
    const items: DataTableMenuItem[] = [
      { name: 'cobradas', label: 'Cobradas' },
      { name: 'impagas', label: 'Impagas' },
      { name: 'd1', label: '', divider: true }
    ];
    if (this.puedeAdministrarCuotas) {
      for (let i = 1; i <= Math.max(1, row.cantidadBotones); i++) {
        items.push({ name: `generar-${i}`, label: row.cantidadBotones > 1 ? `Generar cuotas - Parte ${i}` : 'Generar cuotas' });
      }
      items.push({ name: 'bonificar', label: 'Bonificar año pasado' });
      items.push({ name: 'd2', label: '', divider: true });
    }
    items.push({ name: 'descargar', label: 'Descargar reporte cuotas' });
    return items;
  }

  openYearSelectorForCobradas(cobrador: CobradorDto): void {
    this.pendingYearAction = result => {
      if (result.startSelectedDate && result.endSelectedDate) {
        this.router.navigate(['/cobradores', cobrador.id, 'cuotascobradas', result.year, this.isoDate(result.startSelectedDate), this.isoDate(result.endSelectedDate)]);
      } else {
        this.router.navigate(['/cobradores', cobrador.id, 'cuotascobradas', result.year]);
      }
    };
    this.yearSelector.open({ initialYear: this.initialYear - 1, showDateRangePicker: true });
  }

  openYearSelectorForImpagas(cobrador: CobradorDto): void {
    this.pendingYearAction = result => this.router.navigate(['/cobradores', cobrador.id, 'cuotasimpagas', result.year]);
    this.yearSelector.open({ initialYear: this.initialYear - 1 });
  }

  openYearSelectorForGenerate(cobrador: CobradorDto, parte: number): void {
    this.pendingYearAction = result => this.generateCuotas(cobrador, result.year, result.checkValue, parte);
    this.yearSelector.open({ initialYear: this.initialYear, showCheck: true, checkText: 'Forzar regeneración' });
  }

  generateCuotas(cobrador: CobradorDto, year: number, forceRecreate: boolean, parte: number): void {
    this.cuotasClient.generate({ cobradorId: cobrador.id, year, forceRecreate, parte } as GenerarCuotasCommand).subscribe({
      next: count => {
        if (count === 0) {
          this.toast.show('No hay mas cuotas que generar.');
        } else {
          this.toast.show(`Se han generado ${count} cuotas.`, 'success');
        }
      },
      error: () => this.toast.show('Se produjo un error al generar las cuotas.', 'danger', 10000)
    });
  }

  confirmSetOldCuotasAsBonificadas(cobrador: CobradorDto): void {
    if (!confirm(`¿Está seguro que desea bonificar todas las cuotas de los socios del cobrador ${cobrador.apellido}, ${cobrador.nombre}?`)) {
      return;
    }
    this.cuotasClient.setOldCuotasAsBonificada({ cobradorId: cobrador.id } as BonificarCuotasViejasCommand).subscribe({
      next: result => {
        if (result.cuotasModifiedCount === 0) {
          this.toast.show('No hay mas cuotas para bonificar.');
        } else {
          this.toast.show(`Se han bonificado ${result.cuotasModifiedCount} cuotas.`, 'success');
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, 'Se produjo un error al bonificar las cuotas.'), 'danger')
    });
  }

  openYearSelectorForDownload(cobrador: CobradorDto): void {
    this.pendingYearAction = result => {
      this.reportesClient.getReporteCuotasSocioByCobrador(cobrador.id, result.year).subscribe({
        next: file => {
          if (!file.data || file.data.size === 0) {
            this.toast.show('Reporte no generado. Por favor, genérelo.', 'warning', 3000);
          } else {
            saveBlob(file.data, file.fileName ?? `CuotasSocioByCobrador_${cobrador.id}_Ano_${result.year}.pdf`);
          }
        },
        error: () => this.toast.show('Error desconocido al descargar el reporte de cuotas.', 'danger', 10000)
      });
    };
    this.yearSelector.open({ initialYear: this.initialYear });
  }

  showNewDialog(): void {
    this.openEditor({ id: 0, apellido: '', nombre: '', email: '', comision: null, telefono: '', celular: '' }, false);
  }

  showEditDialog(cobrador: CobradorDto, soloLectura: boolean): void {
    this.openEditor({
      id: cobrador.id,
      apellido: cobrador.apellido,
      nombre: cobrador.nombre,
      email: cobrador.email ?? '',
      comision: cobrador.comision ?? null,
      telefono: cobrador.telefono ?? '',
      celular: cobrador.celular ?? ''
    }, soloLectura);
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
    const cobrador = this.selected()!;
    const command = { id: cobrador.id, password: this.usuarioForm.value.password } as CrearUsuarioCobradorCommand;
    this.client.crearUsuarioCobrador(command.id, command).subscribe({
      next: () => {
        this.cobradores.update(cs => cs!.map(c => c.id === command.id ? { ...c, tieneUsuario: true } as CobradorDto : c));
        this.toast.show(`Se ha creado con éxito el usuario '${cobrador.email}' para el cobrador ${cobrador.id}.`, 'success');
        this.closeUsuarioDialog();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.usuarioForm).general)
    });
  }

  save(): void {
    if (this.soloLectura()) {
      return;
    }
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
        this.toast.show(this.formErrors.firstMessage(error, 'No se pudo eliminar el cobrador.'), 'danger');
      }
    });
  }

  private openEditor(value: { id: number, apellido: string, nombre: string, email: string, comision: number | null, telefono: string, celular: string }, soloLectura: boolean): void {
    this.soloLectura.set(soloLectura);
    this.form.reset(value);
    if (soloLectura) {
      this.form.disable();
    } else {
      this.form.enable();
    }
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
      next: () => {
        this.closeEditDialog();
        this.cargar();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private update(): void {
    const command = { id: this.form.value.id, ...this.payload() } as UpdateCobradorCommand;
    this.client.updateCobrador(command.id, command).subscribe({
      next: () => {
        this.closeEditDialog();
        this.cargar();
      },
      error: error => this.generalErrors.set(this.formErrors.apply(error, this.form).general)
    });
  }

  private isoDate(value: Date): string {
    const mm = String(value.getMonth() + 1).padStart(2, '0');
    const dd = String(value.getDate()).padStart(2, '0');
    return `${value.getFullYear()}-${mm}-${dd}`;
  }
}
