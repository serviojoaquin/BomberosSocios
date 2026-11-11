import { Component, OnInit, signal } from '@angular/core';
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
    { data: 'id', title: 'Id' },
    { data: 'payer_id', title: 'N° Usuario', className: 'dt-body-right' },
    { data: 'payer_first_name', title: 'Nombre' },
    { data: 'payer_last_name', title: 'Apellido' },
    { data: 'payment_method_id', title: 'Método de Pago' },
    { data: 'status', title: 'Estado' }
  ];

  constructor(
    private client: SuscripcionesClient,
    private toast: ToastService,
    private formErrors: FormErrorsService
  ) {}

  ngOnInit(): void {
    this.client.getSuscripciones().subscribe({
      next: result => this.suscripciones.set(result.suscripciones),
      error: error => {
        this.suscripciones.set([]);
        this.toast.show(this.formErrors.firstMessage(error, 'No se pudieron cargar las suscripciones.'), 'danger');
      }
    });
  }
}
