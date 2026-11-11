import { Component, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CuotasClient, CuotasSocioVm, CuotaDto, CobrarCuotasCommand, BonificarCuotasCommand, MarcarCuotaImpagaCommand, ChangeStatusCuotaResponse, CajasClient, OrdenesPresencialesClient, CrearOrdenCommand } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { ToastService } from '../shared/toast/toast.service';

const ESTADO_IMPAGA = 1;

@Component({
  standalone: false,
  selector: 'app-socio-cuotas',
  templateUrl: './socio-cuotas.component.html'
})
export class SocioCuotasComponent implements OnInit {
  @ViewChild('qrDialog') qrDialogRef: ElementRef<HTMLDialogElement>;

  vm = signal<CuotasSocioVm | null>(null);
  years = signal<number[]>([]);
  year = signal<number | null>(null);
  allCuotasByYear = signal<CuotaDto[]>([]);
  cuotasImpagas = signal<CuotaDto[]>([]);
  cantCuotasToPaid = signal(1);
  qrImageUrl = signal<string | null>(null);
  puedePagar = this.authService.hasPermission(Permissions.PagarCuotas);
  puedePagarMercadoPago = this.authService.hasPermission(Permissions.PagosMercadoPago);
  tableOptions = {
    paging: false,
    searching: false,
    info: false,
    ordering: false,
    layout: { topStart: null, topEnd: null, bottomStart: null, bottomEnd: null }
  };
  columns: DataTableColumn[] = [
    { data: 'ano', title: 'cuotas.ano' },
    { data: 'mes', title: 'cuotas.mes', render: (_v, type, row: CuotaDto) => type === 'display' ? this.translate.instant(`meses.l${row.mes}`) : String(row.mes) },
    { data: 'importe', title: 'cuotas.importe', render: (v: number, type) => type === 'display' ? this.formatImporte(v) : String(v) },
    { data: 'estado', title: 'cuotas.estado', render: (v: number, type) => type === 'display' ? `<span class="badge-estado estado-${v}">${this.nombreEstado(v)}</span>` : this.nombreEstado(v) },
    { data: 'fechaDeCobro', title: 'cuotas.fechaDePago', render: (_v, type, row: CuotaDto) => type === 'display' ? this.formatFecha(row.fechaDeCobro ?? row.fechaBonificada) : String(row.fechaDeCobro ?? row.fechaBonificada ?? '') }
  ];
  actions: DataTableAction[] = this.puedePagar
    ? [{ name: 'impaga', label: 'cuotas.marcarImpaga', icon: 'circle-check', visible: (row: CuotaDto) => row.estado !== ESTADO_IMPAGA }]
    : [];

  private socioId = 0;

  constructor(
    private client: CuotasClient,
    private cajasClient: CajasClient,
    private ordenesClient: OrdenesPresencialesClient,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private formErrors: FormErrorsService,
    private toast: ToastService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.socioId = Number(params.get('id'));
      this.cargar();
    });
    if (this.puedePagarMercadoPago) {
      this.cajasClient.getCurrentCaja().subscribe({
        next: result => this.qrImageUrl.set(result.caja?.qr?.image ?? null),
        error: () => this.qrImageUrl.set(null)
      });
    }
  }

  cargar(): void {
    this.client.getCuotasSocio(this.socioId).subscribe({
      next: result => {
        this.vm.set(result);
        const years = this.generateArrayOfYears(result.cuotas);
        this.years.set(years);
        this.setYear(this.year() && years.includes(this.year()!) ? this.year()! : years[0] ?? null);
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorCargar')), 'danger')
    });
  }

  onYearChange(value: string): void {
    this.setYear(Number(value));
  }

  onAction(event: DataTableActionEvent): void {
    if (event.action !== 'impaga') {
      return;
    }
    const cuota = event.row as CuotaDto;
    this.client.setCuotaAsImpaga({ cuotaId: cuota.id } as MarcarCuotaImpagaCommand).subscribe({
      next: result => this.applyChange([cuota.id], result),
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorModificar')), 'danger')
    });
  }

  pagarCuotas(): void {
    const ids = this.getCuotasToSet();
    this.client.setCuotasAsCobrada({ cuotasIds: ids } as CobrarCuotasCommand).subscribe({
      next: result => this.applyChange(ids, result),
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorCobrar')), 'danger')
    });
  }

  bonificarCuotas(): void {
    const ids = this.getCuotasToSet();
    this.client.setCuotasAsBonificada({ cuotasIds: ids } as BonificarCuotasCommand).subscribe({
      next: result => this.applyChange(ids, result),
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorBonificarCuotas')), 'danger')
    });
  }

  generarCodigoQr(): void {
    const ids = this.getCuotasToSet();
    this.ordenesClient.crear({ cuotasIds: ids } as CrearOrdenCommand).subscribe({
      next: () => {
        if (this.qrImageUrl()) {
          this.qrDialogRef.nativeElement.showModal();
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorOrden')), 'danger')
    });
  }

  closeQr(): void {
    this.qrDialogRef.nativeElement.close();
  }

  goToSocios(): void {
    window.scrollTo(0, 0);
    this.router.navigate(['/socios']);
  }

  formatImporte(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(value ?? 0);
  }

  nombreEstado(estado: number): string {
    const key = `cuotas.estados.${estado}`;
    const value = this.translate.instant(key);
    return value === key ? String(estado) : value;
  }

  private setYear(year: number | null): void {
    this.year.set(year);
    const cuotas = (this.vm()?.cuotas ?? []).filter(c => c.ano === year);
    this.allCuotasByYear.set(cuotas);
    this.refreshImpagas();
  }

  private refreshImpagas(): void {
    const impagas = this.allCuotasByYear().filter(c => c.estado === ESTADO_IMPAGA);
    this.cuotasImpagas.set(impagas);
    this.cantCuotasToPaid.set(Math.min(Math.max(1, this.cantCuotasToPaid()), Math.max(1, impagas.length)));
  }

  private getCuotasToSet(): number[] {
    return [...this.cuotasImpagas()]
      .sort((a, b) => a.ano === b.ano ? a.mes - b.mes : a.ano - b.ano)
      .slice(0, this.cantCuotasToPaid())
      .map(c => c.id);
  }

  private applyChange(ids: number[], result: ChangeStatusCuotaResponse): void {
    const vm = this.vm();
    if (!vm) {
      return;
    }
    const cuotas = vm.cuotas.map(c => ids.includes(c.id)
      ? Object.assign(c, { estado: result.cuotaEstadoId, estadoNombre: this.nombreEstado(result.cuotaEstadoId), fechaDeCobro: result.fechaDeCobro, fechaBonificada: result.fechaBonificada })
      : c);
    this.vm.set({ ...vm, cuotas } as CuotasSocioVm);
    this.setYear(this.year());
  }

  private generateArrayOfYears(cuotas: CuotaDto[]): number[] {
    if (!cuotas.length) {
      return [];
    }
    const min = Math.min(...cuotas.map(c => c.ano));
    const max = Math.max(...cuotas.map(c => c.ano));
    const years: number[] = [];
    for (let y = max; y >= min; y--) {
      years.push(y);
    }
    return years;
  }

  private formatFecha(value: Date | string | null | undefined): string {
    if (!value) {
      return '';
    }
    return new Date(value).toLocaleString('es-AR', { dateStyle: 'medium', timeStyle: 'short' });
  }
}
