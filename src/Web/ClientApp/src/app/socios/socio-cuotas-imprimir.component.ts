import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { SociosClient, CuotasImprimirVm } from '../web-api-client';
import { ToastService } from '../shared/toast/toast.service';
import { FormErrorsService } from '../shared/forms/form-errors.service';

@Component({
  standalone: false,
  selector: 'app-socio-cuotas-imprimir',
  template: `
    <app-cuotas-imprimir [vm]="vm()" [year]="year" />
    <div class="volver">
      <button type="button" class="secondary" (click)="goToSocios()">{{ 'comun.volver' | translate }}</button>
    </div>
  `
})
export class SocioCuotasImprimirComponent implements OnInit {
  vm = signal<CuotasImprimirVm | null>(null);
  year = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sociosClient: SociosClient,
    private toast: ToastService,
    private formErrors: FormErrorsService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const socioId = Number(params.get('id'));
      this.year = Number(params.get('year'));
      const regenerar = (params.get('regenerar') ?? '').toLowerCase() === 'true';
      this.sociosClient.getSocioCuotasImprimir(socioId, this.year, regenerar).subscribe({
        next: result => this.vm.set(result),
        error: error => this.toast.show(this.formErrors.firstMessage(error, this.translate.instant('cuotas.errorCargar')), 'danger')
      });
    });
  }

  goToSocios(): void {
    window.scrollTo(0, 0);
    this.router.navigate(['/socios']);
  }
}
