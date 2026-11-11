import { Component, Input, signal } from '@angular/core';
import * as htmlToImage from 'html-to-image';
import pdfMake from 'pdfmake/build/pdfmake';
import { CuotasImprimirVm, ReportesClient, SaveCuotaSocioCommand, SocioImprimirDto } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';

@Component({
  standalone: false,
  selector: 'app-cuotas-imprimir',
  templateUrl: './cuotas-imprimir.component.html'
})
export class CuotasImprimirComponent {
  @Input() vm: CuotasImprimirVm | null = null;
  @Input() year = 0;

  generando = signal(false);

  constructor(private reportesClient: ReportesClient, private toast: ToastService) {}

  formatImporte(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(value ?? 0);
  }

  formatFecha(value: Date | string): string {
    return new Date(value).toLocaleDateString('es-AR');
  }

  async download(): Promise<void> {
    if (!this.vm || this.generando()) {
      return;
    }
    this.generando.set(true);
    const containers = Array.from(document.getElementsByClassName('htmlToPdfContainer')) as HTMLElement[];
    for (const element of containers) {
      const socioId = Number(element.dataset['socioId']);
      try {
        const dataUrl = await htmlToImage.toPng(element, { backgroundColor: '#ffffff' });
        const docDefinition: any = {
          pageSize: 'A4',
          pageMargins: [5, 10, 0, 0],
          info: { title: 'Cuotas' },
          content: [{ image: dataUrl, width: 580, margin: [10, 0] }]
        };
        const base64 = await new Promise<string>(resolve => pdfMake.createPdf(docDefinition).getBase64(resolve));
        await this.guardar(socioId, base64);
      } catch {
        this.toast.show(`Error al generar el reporte del socio N° ${socioId}.`, 'danger', 10000);
      }
    }
    this.generando.set(false);
  }

  private guardar(socioId: number, base64: string): Promise<void> {
    return new Promise(resolve => {
      this.reportesClient.saveCuotaSocio({ socioId, reporte: base64, ano: this.year } as SaveCuotaSocioCommand).subscribe({
        next: () => {
          this.removeSocio(socioId);
          this.toast.show(`Reporte de cuotas para Socio N° ${socioId} y año ${this.year} creado.`, 'success');
          resolve();
        },
        error: () => {
          this.toast.show('Error al guardar el reporte.', 'danger', 10000);
          resolve();
        }
      });
    });
  }

  private removeSocio(socioId: number): void {
    if (this.vm) {
      this.vm.socios = this.vm.socios.filter((s: SocioImprimirDto) => s.id !== socioId);
    }
  }
}
