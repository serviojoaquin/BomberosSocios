import { Component, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SuscripcionesClient, SuscripcionDto } from '../web-api-client';
import { DataTableColumn } from '../shared/data-table/data-table.component';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-suscripciones',
  templateUrl: './suscripciones.component.html'
})
export class SuscripcionesComponent implements OnInit {
  suscripciones = signal<SuscripcionDto[] | null>(null);
  columns: DataTableColumn[] = [
    { data: 'id', title: 'suscripciones.id' },
    { data: 'payer_id', title: 'suscripciones.numeroUsuario', className: 'dt-body-right' },
    { data: 'payer_first_name', title: 'suscripciones.nombre' },
    { data: 'payer_last_name', title: 'suscripciones.apellido' },
    { data: 'payment_method_id', title: 'suscripciones.metodoDePago' },
    { data: 'status', title: 'suscripciones.estado' }
  ];

  constructor(
    private client: SuscripcionesClient,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.client.getSuscripciones().subscribe({
      next: result => this.suscripciones.set(result.suscripciones),
      error: error => {
        this.suscripciones.set([]);
        this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('suscripciones.errorCargar')), 'danger');
      }
    });
  }
}
