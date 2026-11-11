# BaseApp – Contexto para el asistente de IA

**Autor:** Javier Gardella
**Fecha:** 2026-11-09

Este documento es el contexto que necesita un asistente de IA (Claude Code, Copilot, Cursor, etc.) para seguir trabajando sobre esta solución exactamente como se trabajó hasta ahora. Pegalo al inicio de la sesión junto con la carpeta de la solución.

---

## 1. Qué es

Plantilla base para construir aplicaciones web completas (API + frontend + base de datos + deploy en Azure). Nació del template oficial de Jason Taylor ([jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture), `dotnet new ca-sln`) y se le agregó un sistema de permisos granulares y un CRUD de ejemplo.

Stack:

- .NET 10, ASP.NET Core Minimal APIs, EF Core 10, SQL Server / Azure SQL.
- Clean Architecture: `Domain` → `Application` → `Infrastructure` → `Web`.
- MediatR (commands/queries), FluentValidation, AutoMapper, Ardalis.GuardClauses.
- ASP.NET Core Identity con cookies (Identity API endpoints). Sin IdentityServer, sin JWT.
- Angular 21 (NgModule, signals, `@if/@for`), Pico CSS, Lucide icons. Cliente TypeScript generado con NSwag desde el OpenAPI.
- .NET Aspire para orquestación y deploy; Azure Developer CLI (`azd`) publica en Azure Container Apps + Azure SQL.

---

## 2. Estructura

```
BaseApp/
  src/
    Domain/          Entidades, enums, constantes. Sin dependencias.
      Enums/Permissions.cs        enum [Flags] con los permisos del sistema
      Constants/Roles.cs          nombres de roles seedeados
      Entities/SampleItem.cs        entidad de ejemplo
    Application/     Casos de uso. Un folder por feature: Commands/ y Queries/.
      SampleItems/    CRUD de ejemplo (Create/Update/Delete + GetSampleItems)
      Roles/         CRUD de roles con permisos
      Users/         GetCurrentUser, GetUsers, UpdateUserRoles
      Common/Security/AuthorizeAttribute.cs      [Authorize(Permissions = ...)]
      Common/Behaviours/AuthorizationBehaviour.cs chequea roles/permisos antes del handler
    Infrastructure/  EF Core, Identity, seed.
      Identity/ApplicationRole.cs                 IdentityRole + Permissions + IsSystem
      Identity/ApplicationUserClaimsPrincipalFactory.cs  emite el claim "permissions"
      Identity/IdentityService.cs                 roles y usuarios (IIdentityService)
      Data/ApplicationDbContextInitialiser.cs     Database.Migrate() + seed de roles/usuarios/datos
      Data/Migrations/                            migraciones de EF Core (incrementales, nunca se editan las aplicadas)
    Web/             Endpoints Minimal API + Angular.
      Endpoints/*.cs                              un IEndpointGroup por recurso → /api/{Clase}
      Infrastructure/PermissionEndpointExtensions.cs  .RequirePermission(Permissions.X)
      ClientApp/                                  Angular
        src/app/permissions.ts                    espejo TS del enum Permissions
        src/api-authorization/auth.service.ts     usuario actual + hasPermission$()
        src/app/web-api-client.ts                 GENERADO, no editar a mano
        src/app/shared/data-table/              componente <app-data-table> (DataTables 3 encapsulado)
        src/app/shared/toast/                   avisos flotantes (ToastService + <app-toasts>)
        src/app/shared/year-selector/           diálogo compartido para elegir un año
        src/app/access-control/                 matriz permiso × rol (Security)
        src/app/shared/forms/                   Reactive Forms: <app-field-error>, directiva [appInvalid], FormErrorsService (mapea los 400 del servidor a cada control)
        src/app/branding.ts                       nombre y logo de la app (sidebar y título de pestaña)
        src/app/modules.ts                        registro de módulos: título, ruta, ícono, sección, permiso → arma el sidebar
        src/app/layout/                           sidebar y header del shell
    AppHost/         Aspire: define recursos (SQL, Web) y el target Azure Container Apps.
    ServiceDefaults/ Telemetría, health checks.
  tests/
    Domain.UnitTests, Application.UnitTests        corren sin dependencias
    Application.FunctionalTests, Web.AcceptanceTests  necesitan Docker (SQL en contenedor)
  Directory.Packages.props   versiones centralizadas de NuGet (CentralPackageTransitivePinningEnabled)
  azure.yaml                 config de azd
  .azure/                    environments de azd (específicos de cada cuenta Azure; ver §7)
```

---

## 3. Cómo correr en local

Requisitos: .NET 10 SDK, Node 20+, SQL Server LocalDB (viene con Visual Studio) o Docker Desktop.

**Opción A, sin Docker (la usada hasta ahora):**

```powershell
cd BaseApp
dotnet build BaseApp.slnx
dotnet run --project src/Web --launch-profile http          # API en http://localhost:5279
```

En otra terminal:

```powershell
cd BaseApp/src/Web/ClientApp
npm install
$env:services__webapi__http__0 = "http://localhost:5279"
npx ng serve --port 4200                                     # UI en http://localhost:4200
```

**Opción B, con Docker:** `dotnet run --project src/AppHost` levanta SQL en contenedor, API y Angular juntos con el dashboard de Aspire.

Al arrancar, la app aplica las migraciones pendientes (`Database.MigrateAsync()`) y corre el seed, que es idempotente. La base nunca se borra automáticamente.

Usuarios seedeados:

| Usuario | Password | Rol | Permisos |
|---|---|---|---|
| administrator@localhost | Administrator1! | Administrator | Todos |
| viewer@localhost | Viewer1! | Viewer | View sampleItems |

Docs de la API: http://localhost:5279/scalar

---

## 4. Sistema de permisos

- `Permissions` es un enum `[Flags]` en Domain. Cada bit es un permiso: `ViewSampleItems=1, CreateSampleItems=2, EditSampleItems=4, DeleteSampleItems=8, ManageSecurity=16`. `All = ~None`.
- Cada rol (`ApplicationRole`) guarda un entero con la suma de sus permisos. Un usuario tiene el OR de los permisos de todos sus roles.
- Al loguearse, `ApplicationUserClaimsPrincipalFactory` pone ese entero en el claim `permissions` de la cookie. **Los cambios de permisos o roles aplican en el próximo login** del usuario afectado. Decisión tomada a propósito.
- Backend, dos capas:
  - Endpoint: `groupBuilder.MapPost(CreateX).RequirePermission(Permissions.CreateX)`.
  - Command/Query: `[Authorize(Permissions = Permissions.CreateX)]` sobre el record, validado por `AuthorizationBehaviour`.
- Frontend: `AuthService.hasPermission$(Permissions.X)` devuelve un `Observable<boolean>`. Se usa con `@if (can$ | async)` para ocultar botones y links del menú. El enum TS en `permissions.ts` debe mantenerse igual al de C#.
- Rol `Administrator` es de sistema (`IsSystem = true`): no se edita, renombra ni borra. Cualquier otro rol, incluido `Viewer`, se puede editar y borrar si no tiene usuarios asignados.
- Pantallas de administración (requieren `ManageSecurity`): **Roles** (alta/edición con checkboxes de permisos) y **Users** (asignación de roles).

**Para agregar un permiso nuevo:**

1. Agregar el valor en `Domain/Enums/Permissions.cs` con `[Description("...")]` (siguiente potencia de 2).
2. Agregar el mismo valor en `ClientApp/src/app/permissions.ts`.
3. Usarlo en el command (`[Authorize(Permissions = ...)]`) y en el endpoint (`.RequirePermission(...)`).
4. Si corresponde, dárselo a algún rol en el seed (`ApplicationDbContextInitialiser.TrySeedAsync`).

---

## 5. Cómo agregar una entidad con CRUD

Copiar el patrón de `SampleItem`. Orden:

1. `Domain/Entities/X.cs` heredando `BaseAuditableEntity`.
2. `Infrastructure/Data/Configurations/XConfiguration.cs` (longitudes, precisión decimal).
3. `DbSet<X>` en `IApplicationDbContext` y `ApplicationDbContext`.
4. `Application/Xs/Commands/CreateX/CreateX.cs` + `CreateXCommandValidator.cs`, idem Update y Delete. `Application/Xs/Queries/GetXs/GetXs.cs` + `XDto.cs` (con `Mapping : Profile` privado) + `XsVm.cs`.
5. `Web/Endpoints/Xs.cs` implementando `IEndpointGroup`. El nombre de la clase define la ruta `/api/Xs`; el nombre del método define el `operationId` y por lo tanto el nombre del método en el cliente TS.
6. `dotnet build src/Web` regenera `wwwroot/openapi/v1.json`; luego `npm run generate-api` en ClientApp regenera `web-api-client.ts`.
7. Componente Angular en `ClientApp/src/app/xs/` (NgModule, `standalone: false`), declararlo en `app.module.ts`, agregar ruta con `canActivate: [AuthGuard]` y una entrada en `modules.ts` (título, ruta, ícono Lucide registrado en `app.module.ts`, sección y permiso); el sidebar se arma solo. Las grillas se hacen con `<app-data-table [columns] [data] [actions] exportTitle (action)>`: `columns` con `data/title/render/className`, `actions` con `name/label/icon/visible/disabled` filtradas por permiso antes de pasarlas (una acción con `menu` abre un desplegable por fila con sus ítems, fijos o calculados por fila), `footer` para un pie de tabla que llena `footerCallback`, y `options` opcional con sintaxis nativa de DataTables para casos especiales. Los textos de la grilla se cambian proveyendo el token `DATA_TABLE_LANGUAGE` en `app.module.ts`. Nunca instanciar DataTables fuera de ese componente. Para avisos usar `ToastService` (`show(texto, kind)`), para elegir un año el `<app-year-selector>` compartido, y para confirmaciones simples `confirm()`.
8. Seed opcional en `TrySeedAsync`.
9. Generar la migración: `dotnet ef migrations add Add<X> --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations` (requiere `dotnet tool install -g dotnet-ef`). Revisar el archivo generado antes de seguir. Las migraciones ya aplicadas en algún ambiente no se editan: se agrega una nueva.

Convenciones: records para commands/queries, `Guard.Against.NotFound`, `ProjectTo<Dto>`, errores de negocio desde `IdentityService` se devuelven como `Result` y se convierten en 400 con `result.ThrowIfFailed()`.

---

## 6. Convenciones de código

- Todo el código, nombres y textos de UI en inglés.
- Sin comentarios en el código salvo que agreguen algo que el código no dice.
- Un command/query por archivo junto con su handler. Validators en archivo aparte.
- Angular: signals para estado, `<dialog>` nativo para modales, clases de Pico (`icon-btn`, `secondary`, `danger`), íconos Lucide registrados en `app.module.ts` (`LucideAngularModule.pick({...})`).
- Formularios: siempre Reactive Forms (`FormBuilder` + `Validators`), un control por campo con `[appInvalid]` y `<app-field-error>` debajo; `form.markAllAsTouched()` antes de enviar; los 400 del servidor se aplican con `FormErrorsService.apply(error, form)` y los que no tienen campo se muestran como `generalErrors`. Los textos por defecto se traducen proveyendo `FIELD_ERROR_MESSAGES` en `app.module.ts`.
- Grillas: siempre `<app-data-table>`. Los estilos de DataTables se adaptan a Pico en `styles.scss` mapeando las variables `--dt-*` y `--dtb-*` a tokens `--pico-*`; no sobreescribir reglas de DataTables con `!important`.
- Idiomas: la UI se traduce con ngx-translate. Los textos viven en `ClientApp/src/assets/i18n/en.json` y `es.json` (misma estructura de claves en ambos); en las plantillas se usa `{{ 'clave' | translate }}` y en código `TranslateService.instant('clave')`. Los títulos de columnas y las etiquetas de acciones de `<app-data-table>` se pasan como claves y el componente las traduce y se reconstruye solo al cambiar el idioma (DataTables fija el idioma al crearse). Los títulos y secciones de `modules.ts` también son claves. El idioma se elige desde el menú del avatar (`LanguageService`, guardado en localStorage) y para agregar uno nuevo alcanza con sumar el JSON y la entrada en `LANGUAGES`. Toda pantalla nueva agrega sus claves a los dos JSON; nunca texto fijo en plantillas.
- `TreatWarningsAsErrors` está activo: cualquier warning rompe el build. Las vulnerabilidades de paquetes transitivos se resuelven fijando la versión en `Directory.Packages.props`.
- Antes de dar algo por hecho: `dotnet build BaseApp.slnx`, `dotnet test tests/Domain.UnitTests`, `dotnet test tests/Application.UnitTests`, `npx ng build` en ClientApp, y probar los endpoints con el usuario correcto (admin y uno sin permisos).

---

## 7. Deploy en Azure

Lo que hay hoy publicado pertenece a la suscripción de Javier y se va a apagar. **Cada persona publica en su propia suscripción.**

Requisitos: cuenta de Azure con una suscripción activa, Azure CLI (`az`), Azure Developer CLI (`winget install Microsoft.Azd`).

```powershell
az login                      # o az login --use-device-code
azd auth login                # login separado, también acepta --use-device-code
cd BaseApp
Remove-Item -Recurse -Force .azure        # borrar environments ajenos si existen
azd init --from-code -e <nombre-env> -l brazilsouth -s <subscription-id>
azd up
```

`azd up` crea en un resource group `rg-<nombre-env>`: Container Apps Environment, Container Registry, Azure SQL Server + base serverless, Log Analytics y managed identities. La app accede a la base con identidad de Entra, sin password. Al final imprime la URL pública `https://webapi.<hash>.<region>.azurecontainerapps.io`.

Para redeployar después de un cambio: `azd up`. Para borrar todo: `azd down`.

**Problemas conocidos y cómo se resolvieron:**

| Síntoma | Causa | Solución |
|---|---|---|
| `ProvisioningDisabled` al crear Azure SQL | Suscripciones nuevas/free trial tienen SQL bloqueado en eastus, eastus2, westus2 | Usar `brazilsouth` o `centralus`. Probar con `az sql server create` en un RG temporal si hay dudas |
| `MaxNumberOfGlobalEnvironmentsInSubExceeded` | Free trial permite 1 Container Apps Environment | Borrar el RG anterior y esperar a que desaparezca (tarda varios minutos) |
| `Failed to provision component 'aspire-dashboard'` | El dashboard de Aspire no se pudo instalar en el environment | Ya está desactivado en `AppHost/Program.cs` con `.WithDashboard(false)` |
| Login failed (18456) contra SQL al arrancar | La app corría como Development y hacía `EnsureDeleted` contra `master` sin permisos | Ya corregido: al publicar se manda `ASPNETCORE_ENVIRONMENT=Production` y ya no existe `EnsureDeleted`; el esquema va por migraciones |
| `LocalDB is not supported on this platform` al publicar | El generador de OpenAPI ejecuta `Program.cs` en el build Linux | Ya corregido: la inicialización de la base se saltea cuando el proceso es `GetDocument.Insider` |
| La base publicada no tiene una columna/tabla nueva | Faltó generar la migración | `dotnet ef migrations add <Nombre> --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations`, commitear y `azd up`; la app la aplica al arrancar |
| Primera request lenta | Container Apps escala a cero | Normal; o configurar `minReplicas: 1` (cuesta dinero) |
| Se borró un recurso a mano (p. ej. la base) y `azd up` no lo recrea | azd compara con el estado guardado y no ve cambios en el Bicep | `azd provision --no-state` fuerza el redeploy completo; después reiniciar la revisión (`az containerapp revision restart`) para que la app aplique migraciones |

---

## 8. Pendientes y decisiones abiertas

- **Tests funcionales y de aceptación** no se corrieron (necesitan Docker). Los unitarios pasan.
- **MediatR 14** es de Lucky Penny Software: gratis para desarrollo y empresas con facturación menor a 5M USD, muestra un warning de licencia en consola. Alternativa si molesta: bajar a MediatR 12.x o reemplazar por un dispatcher propio.
- **Usuarios**: la pantalla Users (permiso `ManageSecurity`) permite crear usuarios con password inicial y roles, cambiar roles, resetear password y habilitar/deshabilitar (lockout de Identity; un usuario deshabilitado no puede loguearse y su sesión se corta en la siguiente request). El `/register` público sigue existiendo y crea usuarios sin roles.
- **Git**: la solución todavía no tiene repositorio. Crearlo antes de seguir (`git init`, `.gitignore` ya existe).
- `SampleItem` tiene `Description` y `Amount`. Ampliar según el dominio real.
- **Regla de esquema**: todo cambio de base va en una migración incremental nueva. La base publicada NO se borra ni se regenera `InitialCreate`, salvo pedido explícito del dueño del proyecto.

---

## 9. Instrucción para el asistente

Cuando trabajes sobre esta solución:

1. Leé este documento completo antes de tocar código.
2. Respetá la estructura de capas y el patrón de `SampleItems` para cualquier feature nueva.
3. Todo cambio en el backend que afecte la API implica rebuild + `npm run generate-api`.
4. Todo cambio de permisos implica tocar `Permissions.cs` y `permissions.ts` a la vez.
5. Compilá y probá con usuario administrador y con usuario sin permisos antes de reportar algo como terminado.
6. No hagas `git commit`, `git push` ni `azd up` sin que te lo pidan explícitamente.

## 10. BomberosSocios (esta app)

**Autor:** Javier Gardella

BomberosSocios nace de BaseApp (remoto `upstream` → `serviojoaquin/BaseApp`) y es la migración 1:1 de la app vieja `SociosApp2` (AKS). Regla de oro: copiar el comportamiento de la vieja, no inventar pantallas ni cambiar usabilidad; está en producción.

- **Dominio** (`src/Domain/Entities`): Categoria (12 importes), Cobrador (+UsuarioId), Socio (Estado Activo/Pasivo), Cuota (Ano, Mes, Importe, Estado, CobradorId), Reporte (PDF de cuotas por socio y año).
- **Permisos propios** en `Permissions.cs` / `permissions.ts`: Ver/Crear/Editar/Eliminar de Categorías, Cobradores y Socios; VerCuotas, PagarCuotas, AdministrarCuotas, VerEstadisticas, PagosMercadoPago, VerMercadoPago, AdministrarMercadoPago. Rol `Cobrador` (semilla) ve solo sus socios (`ICobradorActual`).
- **Pantallas**: Socios y Cobradores con menú "Cuotas" por fila (ver, generar por año con regeneración, bonificar año pasado, descargar reporte); cuotas del socio (pagar N, bonificar, QR, marcar impaga); cobradas/impagas por cobrador; impresión de cuotas (`socios/:id/imprimircuotas/:año/:regenerar`); Estadísticas (cobranza por cobrador, `/dashboard`); Cajas, Suscripciones y Control de acceso.
- **Reportes PDF**: se generan en el servidor al crear cuotas (`ReportService`, QuestPDF, logo embebido) y se unen con PDFsharp para la descarga por cobrador.
- **Mercado Pago**: proyecto `src/MercadoPagoExtension` copiado de la vieja; credenciales en la sección `MercadoPago` de `appsettings.json` (vacías en el repo, van en user secrets o en el entorno de Azure). Sin credenciales, Cajas lista vacío.
- **Idiomas**: los textos de Bomberos están en `ClientApp/src/assets/i18n/bomberos/es.json` y `en.json`, aparte de los de BaseApp; el loader carga ambos. Las claves de BaseApp (`common.*`, `dataTable.*`, `validation.*`) se reutilizan. Las tarjetas impresas quedan en castellano a propósito.
- **Base local**: `BomberosSociosDb` en LocalDB; usuarios semilla `administrator@localhost / Administrator1!` y `viewer@localhost / Viewer1!`.
- **Deploy**: igual que BaseApp (`azd env new bomberos`, `azd env set AZURE_LOCATION brazilsouth`, `azd up`) en la suscripción propia. Pendiente: pasar los datos reales del SQL del AKS con script (el esquema cambió: estados como enum, usuarios Identity nuevos, reportes).
- **Traer cambios de BaseApp**: `git fetch upstream && git merge upstream/master`; resolver conflictos quedándose con BaseApp en lo compartido (grilla, layout, i18n base) y con Bomberos en pantallas y estilos propios.
