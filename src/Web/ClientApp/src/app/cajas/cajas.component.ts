import { Component, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
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
    { data: 'id', title: 'cajas.id', className: 'dt-body-right' },
    { data: 'name', title: 'cajas.nombre' },
    { data: 'fixed_amount', title: 'cajas.montoFijo', render: (v: boolean) => this.translate.instant(v ? 'comun.si' : 'comun.no') },
    { data: 'status', title: 'cajas.estado' },
    { data: 'store_id', title: 'cajas.storeId' },
    { data: 'external_id', title: 'cajas.idExterno' },
    { data: 'external_store_id', title: 'cajas.idExternoSucursal' }
  ];
  actions: DataTableAction[] = this.puedeAdministrar
    ? [{ name: 'delete', label: 'comun.eliminar', icon: 'trash' }]
    : [];

  constructor(
    private client: CajasClient,
    private authService: AuthService,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
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
        this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cajas.errorCargar')), 'danger');
      }
    });
  }

  crearCajaPrincipal(): void {
    this.client.createCajaPrincipal().subscribe({
      next: caja => {
        if (caja) {
          this.existeCajaPrincipal.set(true);
          this.cajas.update(list => [...(list ?? []), caja]);
          this.toast.show(this.translate.instant('cajas.creada'), 'success');
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cajas.errorCrear')), 'danger')
    });
  }

  onAction(event: DataTableActionEvent): void {
    const caja = event.row as CajaDto;
    if (event.action !== 'delete' || !confirm(this.translate.instant('cajas.confirmarEliminar'))) {
      return;
    }
    this.client.deleteCaja(caja.id).subscribe({
      next: () => {
        this.cajas.update(list => (list ?? []).filter(c => c.id !== caja.id));
        if (caja.external_id === 'CajaPrincipal') {
          this.existeCajaPrincipal.set(false);
        }
      },
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cajas.errorEliminar')), 'danger')
    });
  }
}
