import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
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
    { data: 'id', title: 'socios.numero', className: 'dt-body-right' },
    { data: 'nombre', title: 'socios.nombre' },
    { data: 'numeroDocumento', title: 'socios.documento', className: 'dt-body-right' },
    { data: 'categoria', title: 'socios.categoria' },
    { data: 'cobrador', title: 'socios.cobrador' },
    { data: 'domicilio', title: 'socios.domicilio' },
    { data: 'domicilioDeCobro', title: 'socios.domicilioDeCobro' },
    { data: 'estado', title: 'socios.estado', render: (_v, type, row: SocioDto) => type === 'display' ? this.translate.instant(row.pasivo ? 'comun.pasivo' : 'comun.activo') : (row.pasivo ? 'pasivo' : 'activo') }
  ];
  actions: DataTableAction[] = [
    {
      name: 'cuotas',
      label: 'cuotas.menu',
      icon: 'tasks',
      visible: (row: SocioDto) => row.cuotasCount > 0 || (!row.pasivo && this.puedeAdministrarCuotas),
      menu: (row: SocioDto) => [
        { name: 'ver', label: 'cuotas.ver', visible: () => row.cuotasCount > 0 },
        { name: 'd1', label: '', divider: true, visible: () => row.cuotasCount > 0 },
        { name: 'generar', label: 'cuotas.generar', visible: () => !row.pasivo && this.puedeAdministrarCuotas },
        { name: 'bonificar', label: 'cuotas.bonificarAnoPasado', visible: () => row.cuotasCount > 0 && this.puedeAdministrarCuotas },
        { name: 'd2', label: '', divider: true, visible: () => row.cuotasCount > 0 && this.puedeAdministrarCuotas },
        { name: 'descargar', label: 'cuotas.descargarReporte', visible: () => !row.pasivo && row.cuotasCount > 0 }
      ]
    },
    this.puedeEditar
      ? { name: 'edit', label: 'comun.editar', icon: 'pencil' }
      : { name: 'ver-socio', label: 'comun.ver', icon: 'eye' }
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
    private toast: ToastService,
    private translate: TranslateService
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
      error: () => this.toast.show(this.translate.instant('socios.errorCargar'), 'danger')
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
      checkText: this.translate.instant('cuotas.forzarRegeneracion')
    });
  }

  generateCuotas(socio: SocioDto, year: number, forceRecreate: boolean): void {
    this.cuotasClient.generate({ socioId: socio.id, year, forceRecreate } as GenerarCuotasCommand).subscribe({
      next: count => {
        if (count === 0) {
          this.toast.show(this.translate.instant('cuotas.sinCuotasQueGenerar'));
        } else {
          this.toast.show(this.translate.instant('cuotas.generadas', { cantidad: count }), 'success');
          this.cargar();
        }
      },
      error: () => this.toast.show(this.translate.instant('cuotas.errorGenerar'), 'danger', 10000)
    });
  }

  confirmSetOldCuotasAsBonificadas(socio: SocioDto): void {
    if (!confirm(this.translate.instant('socios.confirmarBonificar', { nombre: socio.nombre }))) {
      return;
    }
    this.cuotasClient.setOldCuotasAsBonificada({ socioId: socio.id } as BonificarCuotasViejasCommand).subscribe({
      next: result => {
        if (result.cuotasModifiedCount === 0) {
          this.toast.show(this.translate.instant('cuotas.sinCuotasParaBonificar'));
        } else {
          this.toast.show(this.translate.instant('cuotas.bonificadas', { cantidad: result.cuotasModifiedCount }), 'success');
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorBonificar')), 'danger')
    });
  }

  openYearSelectorForDownload(socio: SocioDto): void {
    this.pendingYearAction = result => {
      this.reportesClient.getReporteCuotasSocio(socio.id, result.year).subscribe({
        next: file => {
          if (!file.data || file.data.size === 0) {
            this.toast.show(this.translate.instant('cuotas.reporteNoGenerado'), 'warning', 3000);
          } else {
            saveBlob(file.data, file.fileName ?? `CuotasSocio_${socio.id}_Ano_${result.year}.pdf`);
          }
        },
        error: () => this.toast.show(this.translate.instant('cuotas.errorDescargarReporte'), 'danger')
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
        this.toast.show(this.translate.instant('socios.errorActualizar'), 'danger');
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
