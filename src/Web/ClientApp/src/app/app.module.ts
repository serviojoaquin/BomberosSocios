import { APP_ID, NgModule, inject, provideAppInitializer } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LucideAngularModule, Sun, Moon, Laptop, Plus, Settings, MoreHorizontal, Pencil, Trash2, House, Hash, CloudSun, ListChecks, Tags, Box, Wallet, Contact, Receipt, Shield, Users, Layers, Menu, PanelLeftClose, PanelLeftOpen, LockKeyhole, QrCode, ChartColumn, User, Check } from 'lucide-angular';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { TranslatePipe, provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { LanguageService, DEFAULT_LANGUAGE } from './i18n/language.service';

import { AppComponent } from './app.component';
import { SidebarComponent } from './layout/sidebar.component';
import { HeaderComponent } from './layout/header.component';
import { HomeComponent } from './home/home.component';
import { CategoriasComponent } from './categorias/categorias.component';
import { CobradoresComponent } from './cobradores/cobradores.component';
import { CobradorCuotasCobradasComponent } from './cobradores/cobrador-cuotas-cobradas.component';
import { CobradorCuotasImpagasComponent } from './cobradores/cobrador-cuotas-impagas.component';
import { CobradorCuotasImprimirComponent } from './cobradores/cobrador-cuotas-imprimir.component';
import { SociosComponent } from './socios/socios.component';
import { SocioCuotasComponent } from './socios/socio-cuotas.component';
import { SocioCuotasImprimirComponent } from './socios/socio-cuotas-imprimir.component';
import { CuotasImprimirComponent } from './cuotas/cuotas-imprimir.component';
import { CajasComponent } from './cajas/cajas.component';
import { SuscripcionesComponent } from './suscripciones/suscripciones.component';
import { SuscripcionRegisterComponent } from './suscripciones/suscripcion-register.component';
import { AccessControlComponent } from './access-control/access-control.component';
import { EstadisticasComponent } from './estadisticas/estadisticas.component';
import { RolesComponent } from './roles/roles.component';
import { UsersComponent } from './users/users.component';
import { DataTableComponent } from './shared/data-table/data-table.component';
import { YearSelectorComponent } from './shared/year-selector/year-selector.component';
import { ToastComponent } from './shared/toast/toast.component';
import { FieldErrorComponent } from './shared/forms/field-error.component';
import { InvalidDirective } from './shared/forms/invalid.directive';
import { ThemeToggleComponent } from './theme-toggle/theme-toggle.component';
import { API_BASE_URL } from './web-api-client';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { LoginComponent } from 'src/api-authorization/login/login.component';
import { RegisterComponent } from 'src/api-authorization/register/register.component';
import { AuthGuard } from 'src/api-authorization/auth.guard';
import { AuthService } from 'src/api-authorization/auth.service';

export function getApiBaseUrl(): string {
  const url = document.getElementsByTagName('base')[0].href;
  return url.endsWith('/') ? url.slice(0, -1) : url;
}

@NgModule({
    declarations: [
        AppComponent,
        SidebarComponent,
        HeaderComponent,
        HomeComponent,
        CategoriasComponent,
        CobradoresComponent,
        CobradorCuotasCobradasComponent,
        CobradorCuotasImpagasComponent,
        CobradorCuotasImprimirComponent,
        SociosComponent,
        SocioCuotasComponent,
        SocioCuotasImprimirComponent,
        CuotasImprimirComponent,
        CajasComponent,
        SuscripcionesComponent,
        SuscripcionRegisterComponent,
        AccessControlComponent,
        EstadisticasComponent,
        RolesComponent,
        UsersComponent,
        DataTableComponent,
        YearSelectorComponent,
        ToastComponent,
        FieldErrorComponent,
        InvalidDirective,
        ThemeToggleComponent,
        LoginComponent,
        RegisterComponent
    ],
    bootstrap: [AppComponent],
    imports: [
        BrowserModule,
        FormsModule,
        ReactiveFormsModule,
        LucideAngularModule.pick({ Sun, Moon, Laptop, Plus, Settings, MoreHorizontal, Pencil, Trash2, House, Hash, CloudSun, ListChecks, Tags, Box, Wallet, Contact, Receipt, Shield, Users, Layers, Menu, PanelLeftClose, PanelLeftOpen, LockKeyhole, QrCode, ChartColumn, User, Check }),
        TranslatePipe,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'categorias', component: CategoriasComponent, canActivate: [AuthGuard] },
            { path: 'cobradores', component: CobradoresComponent, canActivate: [AuthGuard] },
            { path: 'cobradores/:id/cuotasimpagas/:year', component: CobradorCuotasImpagasComponent, canActivate: [AuthGuard] },
            { path: 'cobradores/:id/cuotascobradas/:year', component: CobradorCuotasCobradasComponent, canActivate: [AuthGuard] },
            { path: 'cobradores/:id/cuotascobradas/:year/:fechaDeCobroFrom/:fechaDeCobroTo', component: CobradorCuotasCobradasComponent, canActivate: [AuthGuard] },
            { path: 'cobradores/:id/imprimircuotas/:year/:regenerar', component: CobradorCuotasImprimirComponent, canActivate: [AuthGuard] },
            { path: 'socios', component: SociosComponent, canActivate: [AuthGuard] },
            { path: 'socios/:id/cuotas', component: SocioCuotasComponent, canActivate: [AuthGuard] },
            { path: 'socios/:id/imprimircuotas/:year/:regenerar', component: SocioCuotasImprimirComponent, canActivate: [AuthGuard] },
            { path: 'cajas', component: CajasComponent, canActivate: [AuthGuard] },
            { path: 'suscripciones', component: SuscripcionesComponent, canActivate: [AuthGuard] },
            { path: 'suscripciones/registrar', component: SuscripcionRegisterComponent, canActivate: [AuthGuard] },
            { path: 'control-de-acceso', component: AccessControlComponent, canActivate: [AuthGuard] },
            { path: 'dashboard', component: EstadisticasComponent, canActivate: [AuthGuard] },
            { path: 'roles', component: RolesComponent, canActivate: [AuthGuard] },
            { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent }
        ])
    ],
    providers: [
        { provide: APP_ID, useValue: 'ng-cli-universal' },
        provideTranslateService({
            loader: provideTranslateHttpLoader({ resources: [{ prefix: './assets/i18n/', suffix: '.json' }, { prefix: './assets/i18n/bomberos/', suffix: '.json' }] }),
            fallbackLang: DEFAULT_LANGUAGE,
            lang: DEFAULT_LANGUAGE
        }),
        { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
        { provide: API_BASE_URL, useFactory: getApiBaseUrl, deps: [] },
        provideAppInitializer(() => inject(LanguageService).init()),
        provideAppInitializer(() => inject(AuthService).initialize()),
        provideHttpClient(withInterceptorsFromDi())
    ]
})
export class AppModule { }
