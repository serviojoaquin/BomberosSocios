import { Component, ElementRef, OnDestroy, OnInit, ViewChild, signal } from '@angular/core';
import DataTable from 'datatables.net-dt';
import { DashboardClient, CobranzaCuotaSociosDto } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-estadisticas',
  templateUrl: './estadisticas.component.html'
})
export class EstadisticasComponent implements OnInit, OnDestroy {
  private tablaRef: ElementRef<HTMLTableElement> | undefined;

  @ViewChild('tabla') set tabla(ref: ElementRef<HTMLTableElement> | undefined) {
    this.tablaRef = ref;
    if (ref && this.pendingInit) {
      this.pendingInit = false;
      queueMicrotask(() => this.initTable());
    }
  }

  years = this.generateArrayOfYears();
  year = new Date().getFullYear();
  buckets = [...Array(13).keys()];
  filas = signal<CobranzaCuotaSociosDto[]>([]);
  totalRow = signal<CobranzaCuotaSociosDto | null>(null);
  cargado = signal(false);
  invertir = signal(false);

  private table: any = null;
  private pendingInit = false;

  constructor(
    private client: DashboardClient,
    private toast: ToastService,
    private formErrors: FormErrorsService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  ngOnDestroy(): void {
    this.destroyTable();
  }

  onYearChange(value: string): void {
    this.year = Number(value);
    this.cargar();
  }

  invertirColumnas(): void {
    this.destroyTable();
    this.invertir.update(v => !v);
    this.pendingInit = true;
  }

  cargar(): void {
    this.destroyTable();
    this.cargado.set(false);
    this.client.getCobranzaCuotaSocios(this.year).subscribe({
      next: result => {
        const list = [...result.cobranzaCuotaSociosList];
        this.totalRow.set(list.length ? list.pop()! : null);
        this.filas.set(list);
        this.cargado.set(true);
        this.pendingInit = list.length > 0;
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, 'No se pudieron cargar las estadísticas.'), 'danger')
    });
  }

  porcI(fila: CobranzaCuotaSociosDto, i: number): string {
    const value = fila.porcI[i];
    return value >= 0 ? this.percent(value) : '-';
  }

  percent(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'percent', maximumFractionDigits: 0 }).format(value ?? 0);
  }

  get subtitle(): string {
    return `Cobranza Cuota Socios ${this.year}`;
  }

  get filename(): string {
    return `CobranzaCuotaSocios_${this.year}`;
  }

  private initTable(): void {
    if (!this.tablaRef || this.table) {
      return;
    }
    const self = this;
    const invertido = this.invertir();
    this.table = new DataTable(this.tablaRef!.nativeElement, {
      paging: false,
      info: false,
      searching: !invertido,
      ordering: !invertido,
      orderClasses: false,
      language: { search: '', searchPlaceholder: 'Buscar…' },
      layout: { topStart: invertido ? null : 'search', topEnd: 'buttons', bottomStart: null, bottomEnd: null },
      buttons: [
        { extend: 'excelHtml5', text: 'Excel', footer: true, title: () => self.subtitle, filename: () => self.filename, exportOptions: { columns: ':visible' } },
        {
          extend: 'pdfHtml5', text: 'PDF', footer: true, orientation: 'landscape', title: () => self.subtitle, filename: () => self.filename,
          exportOptions: { columns: ':visible' },
          customize: (doc: any) => {
            doc.styles.title.alignment = 'left';
            doc.defaultStyle.fontSize = 8;
            doc.styles.tableHeader.fontSize = 8;
            doc.styles.tableFooter.fontSize = 8;
            const body = doc.content[1].table.body;
            if (!invertido) {
              doc.pageMargins = [10, 30, 10, 30];
              const widths: string[] = [];
              for (let index = 0; index < body[0].length; index++) {
                if (index === 0) {
                  widths.push('*');
                } else if (index === body[0].length - 1) {
                  widths.push('4%');
                } else {
                  widths.push('3.2%');
                  body[0][index].text = (Math.ceil(index / 2) - 1) + body[0][index].text;
                }
              }
              doc.content[1].table.widths = widths;
            }
            for (let i = 1; i < body.length; i++) {
              for (let j = 1; j < body[0].length; j++) {
                body[i][j].alignment = 'right';
              }
            }
          }
        }
      ]
    } as any);
  }

  private destroyTable(): void {
    this.table?.destroy();
    this.table = null;
  }

  private generateArrayOfYears(): number[] {
    const max = new Date().getFullYear();
    const years: number[] = [];
    for (let y = max; y >= 2021; y--) {
      years.push(y);
    }
    return years;
  }
}
