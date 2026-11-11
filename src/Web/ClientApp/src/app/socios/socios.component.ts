import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SociosClient, SocioDto, LookupDto, CreateSocioCommand, UpdateSocioCommand, UpdateDatosAfiliacionCommand, CuotasClient, ReportesClient, GenerarCuotasCommand, BonificarCuotasViejasCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn, DataTableComponent } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { YearSelectorComponent, YearSelectorResult } from '../shared/year-selector/year-selector.component';
import { ToastService } from '../shared/toast/toast.service';
import { saveBlob } from '../shared/download';

@Component({
  standalone: false,
  selector: 'app-socios',
  templateUrl: './socios.component.html'
})
export class SociosComponent implements OnInit {
  @ViewChild('editDialog') editDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild('afiliacionDialog') afiliacionDialogRef: ElementRef<HTMLDialogElement>;
  @ViewChild(DataTableComponent) table: DataTableComponent;
  @ViewChild(YearSelectorComponent) yearSelector: YearSelectorComponent;

  socios = signal<SocioDto[] | null>(null);
  categorias = signal<LookupDto[]>([]);
  cobradores = signal<LookupDto[]>([]);
  seleccionados = signal<SocioDto[]>([]);
  initialYear = new Date().getFullYear();
  puedeCrear = this.authService.hasPermission(Permissions.CrearSocios);
  puedeEditar = this.authService.hasPermission(Permissions.EditarSocios);
  puedeAdministrarCuotas = this.authService.hasPermission(Permissions.AdministrarCuotas);
  soloLectura = signal(false);
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
    {
      name: 'cuotas',
      label: 'Cuotas',
      icon: 'tasks',
      visible: (row: SocioDto) => row.cuotasCount > 0 || (!row.pasivo && this.puedeAdministrarCuotas),
      menu: (row: SocioDto) => [
        { name: 'ver', label: 'Ver', visible: () => row.cuotasCount > 0 },
        { name: 'd1', label: '', divider: true, visible: () => row.cuotasCount > 0 },
        { name: 'generar', label: 'Generar', visible: () => !row.pasivo && this.puedeAdministrarCuotas },
        { name: 'bonificar', label: 'Bonificar año pasado', visible: () => row.cuotasCount > 0 && this.puedeAdministrarCuotas },
        { name: 'd2', label: '', divider: true, visible: () => row.cuotasCount > 0 && this.puedeAdministrarCuotas },
        { name: 'descargar', label: 'Descargar reporte cuotas', visible: () => !row.pasivo && row.cuotasCount > 0 }
      ]
    },
    this.puedeEditar
      ? { name: 'edit', label: 'Editar', icon: 'pencil' }
      : { name: 'ver-socio', label: 'Ver', icon: 'eye' }
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

  private pendingYearAction: ((result: YearSelectorResult) => void) | null = null;

  constructor(
    private client: SociosClient,
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
    this.client.getSocios().subscribe({
      next: result => {
        this.categorias.set(result.categorias);
        this.cobradores.set(result.cobradores);
        this.initialYear = result.initialYear;
        this.socios.set(result.socios);
        this.seleccionados.set([]);
      },
      error: () => this.toast.show('Error al cargar los socios.', 'danger')
    });
  }

  onAction(event: DataTableActionEvent): void {
    const socio = event.row as SocioDto;
    switch (event.action) {
      case 'ver':
        this.router.navigate(['/socios', socio.id, 'cuotas']);
        break;
      case 'generar':
        this.openYearSelectorForGenerate(socio);
        break;
      case 'bonificar':
        this.confirmSetOldCuotasAsBonificadas(socio);
        break;
      case 'descargar':
        this.openYearSelectorForDownload(socio);
        break;
      case 'edit':
        this.showEditDialog(socio, false);
        break;
      case 'ver-socio':
        this.showEditDialog(socio, true);
        break;
    }
  }

  onYearSelected(result: YearSelectorResult): void {
    const action = this.pendingYearAction;
    this.pendingYearAction = null;
    action?.(result);
  }

  openYearSelectorForGenerate(socio: SocioDto): void {
    this.pendingYearAction = result => this.generateCuotas(socio, result.year, result.checkValue);
    this.yearSelector.open({
      initialYear: this.initialYear,
      maxYears: socio.cuotasCount > 0 ? 2 : 1,
      showCheck: true,
      checkText: 'Forzar regeneración'
    });
  }

  generateCuotas(socio: SocioDto, year: number, forceRecreate: boolean): void {
    this.cuotasClient.generate({ socioId: socio.id, year, forceRecreate } as GenerarCuotasCommand).subscribe({
      next: count => {
        if (count === 0) {
          this.toast.show('No hay mas cuotas que generar.');
        } else {
          this.toast.show(`Se han generado ${count} cuotas.`, 'success');
          this.cargar();
        }
      },
      error: () => this.toast.show('Se produjo un error al generar las cuotas.', 'danger', 10000)
    });
  }

  confirmSetOldCuotasAsBonificadas(socio: SocioDto): void {
    if (!confirm(`¿Está seguro que desea bonificar todas las cuotas del socio ${socio.nombre}?`)) {
      return;
    }
    this.cuotasClient.setOldCuotasAsBonificada({ socioId: socio.id } as BonificarCuotasViejasCommand).subscribe({
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

  openYearSelectorForDownload(socio: SocioDto): void {
    this.pendingYearAction = result => {
      this.reportesClient.getReporteCuotasSocio(socio.id, result.year).subscribe({
        next: file => {
          if (!file.data || file.data.size === 0) {
            this.toast.show('Reporte no generado. Por favor, genérelo.', 'warning', 3000);
          } else {
            saveBlob(file.data, file.fileName ?? `CuotasSocio_${socio.id}_Ano_${result.year}.pdf`);
          }
        },
        error: () => this.toast.show('Error desconocido al descargar el reporte de cuotas socio.', 'danger')
      });
    };
    this.yearSelector.open({ initialYear: this.initialYear });
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
    }, false);
  }

  showEditDialog(socio: SocioDto, soloLectura: boolean): void {
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
    }, soloLectura);
  }

  closeEditDialog(): void {
    this.editDialogRef.nativeElement.close();
    this.generalErrors.set([]);
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
      error: error => {
        this.generalErrors.set(this.formErrors.apply(error, this.afiliacionForm).general);
        this.toast.show('Se produjo un error al actualizar los socios.', 'danger');
      }
    });
  }

  private openEditor(value: any, soloLectura: boolean): void {
    this.soloLectura.set(soloLectura);
    this.form.reset(value);
    if (soloLectura) {
      this.form.disable();
    } else {
      this.form.enable();
    }
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
