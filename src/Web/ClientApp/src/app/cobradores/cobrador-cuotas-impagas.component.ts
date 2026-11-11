import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CobradoresClient, CobradorCuotasImpagasVm } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';
import { formatPivotImporte, pivotColumns, pivotPdfCustomize } from './cobrador-cuotas-pivot';

@Component({
  standalone: false,
  selector: 'app-cobrador-cuotas-impagas',
  templateUrl: './cobrador-cuotas-impagas.component.html'
})
export class CobradorCuotasImpagasComponent implements OnInit {
  vm = signal<CobradorCuotasImpagasVm | null>(null);
  year = 0;
  columns = pivotColumns();
  tableOptions: Record<string, any> = this.buildOptions();

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
    const base = this.translate.instant('cobradores.cobradorNumero', { id: vm.cobrador.id, nombre: `${vm.cobrador.apellido}, ${vm.cobrador.nombre}` });
    return `${base} - ${this.translate.instant('cobradores.cuotasImpagas')} - ${this.year}`;
  }

  get filename(): string {
    const vm = this.vm();
    return vm ? `Cobrador_${vm.cobrador.id}_CuotasImpagas_${this.year}` : '';
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const cobradorId = Number(params.get('id'));
      this.year = Number(params.get('year'));
      this.cobradoresClient.getCuotasImpagas(cobradorId, this.year).subscribe({
        next: result => this.vm.set(result),
        error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorCargar')), 'danger')
      });
    });
  }

  goToCobradores(): void {
    window.scrollTo(0, 0);
    this.router.navigate(['/cobradores']);
  }

  private buildOptions(): Record<string, any> {
    const self = this;
    return {
      searching: false,
      paging: false,
      info: false,
      layout: { topStart: 'buttons', topEnd: null, bottomStart: null, bottomEnd: null },
      buttons: [
        { extend: 'excelHtml5', text: 'Excel', title: () => self.subtitle, filename: () => self.filename, exportOptions: { columns: ':visible' } },
        { extend: 'pdfHtml5', text: 'PDF', title: () => self.subtitle, filename: () => self.filename, footer: true, orientation: 'landscape', exportOptions: { columns: ':visible' }, customize: pivotPdfCustomize('center') }
      ],
      footerCallback: function (this: any) {
        const api = this.api();
        const total = api.column(14, { page: 'current' }).data().reduce((a: number, b: number) => (Number(a) || 0) + (Number(b) || 0), 0);
        api.column(13).footer().innerHTML = `<div style="text-align:right">${self.translate.instant('comun.total')}</div>`;
        api.column(14).footer().innerHTML = `<div style="text-align:right">${formatPivotImporte(total)}</div>`;
      }
    };
  }
}
