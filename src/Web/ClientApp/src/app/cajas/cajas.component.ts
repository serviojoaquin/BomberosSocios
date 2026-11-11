import { Component, OnInit, signal } from '@angular/core';
import { CajasClient, CajaDto } from '../web-api-client';
import { AuthService } from 'src/api-authorization/auth.service';
import { Permissions } from '../permissions';
import { DataTableAction, DataTableActionEvent, DataTableColumn } from '../shared/data-table/data-table.component';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-cajas',
  templateUrl: './cajas.component.html'
})
export class CajasComponent implements OnInit {
  cajas = signal<CajaDto[] | null>(null);
  existeCajaPrincipal = signal(true);
  puedeAdministrar = this.authService.hasPermission(Permissions.AdministrarMercadoPago);
  columns: DataTableColumn[] = [
    { data: 'id', title: 'Id', className: 'dt-body-right' },
    { data: 'name', title: 'Nombre' },
    { data: 'fixed_amount', title: 'Monto Fijo', render: (v: boolean) => v ? 'Si' : 'No' },
    { data: 'status', title: 'Estado' },
    { data: 'store_id', title: 'Store Id' },
    { data: 'external_id', title: 'Id externo' },
    { data: 'external_store_id', title: 'Id externo sucursal' }
  ];
  actions: DataTableAction[] = this.puedeAdministrar
    ? [{ name: 'delete', label: 'Eliminar', icon: 'trash' }]
    : [];

  constructor(
    private client: CajasClient,
    private authService: AuthService,
    private toast: ToastService,
    private formErrors: FormErrorsService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.client.getCajas().subscribe({
      next: result => {
        this.cajas.set(result.cajas);
        this.existeCajaPrincipal.set(result.cajas.some(c => c.external_id === 'CajaPrincipal'));
      },
      error: error => {
        this.cajas.set([]);
        this.toast.show(this.formErrors.firstMessage(error, 'No se pudieron cargar las cajas de Mercado Pago.'), 'danger');
      }
    });
  }

  crearCajaPrincipal(): void {
    this.client.createCajaPrincipal().subscribe({
      next: caja => {
        if (caja) {
          this.existeCajaPrincipal.set(true);
          this.cajas.update(list => [...(list ?? []), caja]);
          this.toast.show('Se ha creado con éxito la caja principal', 'success');
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, 'No se pudo crear la caja principal.'), 'danger')
    });
  }

  onAction(event: DataTableActionEvent): void {
    const caja = event.row as CajaDto;
    if (event.action !== 'delete' || !confirm('Está seguro que desea eliminar la caja?')) {
      return;
    }
    this.client.deleteCaja(caja.id).subscribe({
      next: () => {
        this.cajas.update(list => (list ?? []).filter(c => c.id !== caja.id));
        if (caja.external_id === 'CajaPrincipal') {
          this.existeCajaPrincipal.set(false);
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, 'No se pudo eliminar la caja.'), 'danger')
    });
  }
}
