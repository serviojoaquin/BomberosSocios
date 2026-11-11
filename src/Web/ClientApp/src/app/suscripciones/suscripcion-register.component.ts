import { Component, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SuscripcionesClient, RegisterSuscripcionVm, CategoriaSuscripcionDto, CreateSuscripcionCommand } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-suscripcion-register',
  templateUrl: './suscripcion-register.component.html'
})
export class SuscripcionRegisterComponent implements OnInit {
  vm = signal<RegisterSuscripcionVm | null>(null);

  constructor(
    private client: SuscripcionesClient,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.client.register().subscribe({
      next: result => this.vm.set(result),
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('suscripciones.errorCargarCategorias')), 'danger')
    });
  }

  crearSuscripcion(categoria: CategoriaSuscripcionDto): void {
    this.client.create({ categoriaId: categoria.id, importe: categoria.importe } as CreateSuscripcionCommand).subscribe({
      next: () => this.toast.show(this.translate.instant('suscripciones.creada'), 'success'),
      error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('suscripciones.errorCrear')), 'danger')
    });
  }

  formatImporte(value: number): string {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(value ?? 0);
  }
}
