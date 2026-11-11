import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CobradoresClient, CobradorCuotasCobradasVm } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { formatPivotImporte, pivotColumns, pivotPdfCustomize } from './cobrador-cuotas-pivot';

@Component({
  standalone: false,
  selector: 'app-cobrador-cuotas-cobradas',
  templateUrl: './cobrador-cuotas-cobradas.component.html'
})
export class CobradorCuotasCobradasComponent implements OnInit {
  vm = signal<CobradorCuotasCobradasVm | null>(null);
  year = 0;
  columns = pivotColumns();
  tableOptions: Record<string, any> = {};

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cobradoresClient: CobradoresClient,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  get subtitle(): string {
    const vm = this.vm();
    if (!vm) {
      return '';
    }
    const base = this.translate.instant('cobradores.cobradorNumero', { id: vm.cobrador.id, nombre: `${vm.cobrador.apellido}, ${vm.cobrador.nombre}` }) + ' - ';
    if (!vm.fechaDeCobroFromFormatted) {
      return `${base}${this.translate.instant('cobradores.cuotasCobradas')} - ${this.year}`;
    }
    if (vm.fechaDeCobroFromFormatted !== vm.fechaDeCobroToFormatted) {
      return base + this.translate.instant('cobradores.cuotasCobradasDesde', { desde: vm.fechaDeCobroFromFormatted, hasta: vm.fechaDeCobroToFormatted });
    }
    return base + this.translate.instant('cobradores.cuotasCobradasEl', { fecha: vm.fechaDeCobroFromFormatted });
  }

  get filename(): string {
    const vm = this.vm();
    if (!vm) {
      return '';
    }
    return vm.fechaDeCobroFromFormatted
      ? `Cobrador_${vm.cobrador.id}_CuotasCobradas_${this.year}_${vm.fechaDeCobroFromFormatted}_${vm.fechaDeCobroToFormatted}`
      : `Cobrador_${vm.cobrador.id}_CuotasCobradas_${this.year}`;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const cobradorId = Number(params.get('id'));
      this.year = Number(params.get('year'));
      const fromParam = params.get('fechaDeCobroFrom');
      const toParam = params.get('fechaDeCobroTo');
      const from = fromParam ? new Date(fromParam + 'T00:00:00') : undefined;
      const to = toParam ? new Date(toParam + 'T00:00:00') : undefined;
      this.cobradoresClient.getCuotasCobradas(cobradorId, this.year, from, to).subscribe({
        next: result => {
          this.tableOptions = this.buildOptions(result.cobrador.comision);
          this.vm.set(result);
        },
        error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorCargar')), 'danger')
      });
    });
  }

  goToCobradores(): void {
    window.scrollTo(0, 0);
    this.router.navigate(['/cobradores']);
  }

  private buildOptions(comision: number): Record<string, any> {
    const self = this;
    return {
      searching: false,
      paging: false,
      info: false,
      layout: { topStart: 'buttons', topEnd: null, bottomStart: null, bottomEnd: null },
      buttons: [
        { extend: 'excelHtml5', text: 'Excel', title: () => self.subtitle, filename: () => self.filename, exportOptions: { columns: ':visible' } },
        { extend: 'pdfHtml5', text: 'PDF', title: () => self.subtitle, filename: () => self.filename, footer: true, orientation: 'landscape', exportOptions: { columns: ':visible' }, customize: pivotPdfCustomize('right') }
      ],
      footerCallback: function (this: any) {
        const api = this.api();
        const total = api.column(14, { page: 'current' }).data().reduce((a: number, b: number) => (Number(a) || 0) + (Number(b) || 0), 0);
        const comisionTotal = total * comision / 100;
        const neto = total - comisionTotal;
        api.column(13).footer().innerHTML = `<div style="text-align:right">${self.translate.instant('comun.total')}:</div><div style="text-align:right">${comision}%:</div><div style="text-align:right">${self.translate.instant('cobradores.neto')}:</div>`;
        api.column(14).footer().innerHTML = `<div style="text-align:right">${formatPivotImporte(total)}</div><div style="text-align:right">${formatPivotImporte(comisionTotal)}</div><div style="text-align:right">${formatPivotImporte(neto)}</div>`;
      }
    };
  }
}
