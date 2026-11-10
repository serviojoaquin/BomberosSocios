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
      Entities/Category.cs        entidad de ejemplo
    Application/     Casos de uso. Un folder por feature: Commands/ y Queries/.
      Categories/    CRUD de ejemplo (Create/Update/Delete + GetCategories)
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
| viewer@localhost | Viewer1! | Viewer | View categories |

Docs de la API: http://localhost:5279/scalar

---

## 4. Sistema de permisos

- `Permissions` es un enum `[Flags]` en Domain. Cada bit es un permiso: `ViewCategories=1, CreateCategories=2, EditCategories=4, DeleteCategories=8, ManageSecurity=16`. `All = ~None`.
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

Copiar el patrón de `Category`. Orden:

1. `Domain/Entities/X.cs` heredando `BaseAuditableEntity`.
2. `Infrastructure/Data/Configurations/XConfiguration.cs` (longitudes, precisión decimal).
3. `DbSet<X>` en `IApplicationDbContext` y `ApplicationDbContext`.
4. `Application/Xs/Commands/CreateX/CreateX.cs` + `CreateXCommandValidator.cs`, idem Update y Delete. `Application/Xs/Queries/GetXs/GetXs.cs` + `XDto.cs` (con `Mapping : Profile` privado) + `XsVm.cs`.
5. `Web/Endpoints/Xs.cs` implementando `IEndpointGroup`. El nombre de la clase define la ruta `/api/Xs`; el nombre del método define el `operationId` y por lo tanto el nombre del método en el cliente TS.
6. `dotnet build src/Web` regenera `wwwroot/openapi/v1.json`; luego `npm run generate-api` en ClientApp regenera `web-api-client.ts`.
7. Componente Angular en `ClientApp/src/app/xs/` (NgModule, `standalone: false`), declararlo en `app.module.ts`, agregar ruta con `canActivate: [AuthGuard]` y link en `nav-menu.component.html` condicionado por permiso. Las grillas se hacen con `<app-data-table [columns] [data] [actions] exportTitle (action)>`: `columns` con `data/title/render/className`, `actions` con `name/label/icon/visible` filtradas por permiso antes de pasarlas, y `options` opcional con sintaxis nativa de DataTables para casos especiales. Nunca instanciar DataTables fuera de ese componente.
8. Seed opcional en `TrySeedAsync`.
9. Generar la migración: `dotnet ef migrations add Add<X> --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations` (requiere `dotnet tool install -g dotnet-ef`). Revisar el archivo generado antes de seguir. Las migraciones ya aplicadas en algún ambiente no se editan: se agrega una nueva.

Convenciones: records para commands/queries, `Guard.Against.NotFound`, `ProjectTo<Dto>`, errores de negocio desde `IdentityService` se devuelven como `Result` y se convierten en 400 con `result.ThrowIfFailed()`.

---

## 6. Convenciones de código

- Todo el código, nombres y textos de UI en inglés.
- Sin comentarios en el código salvo que agreguen algo que el código no dice.
- Un command/query por archivo junto con su handler. Validators en archivo aparte.
- Angular: signals para estado, `<dialog>` nativo para modales, clases de Pico (`icon-btn`, `secondary`, `danger`), íconos Lucide registrados en `app.module.ts` (`LucideAngularModule.pick({...})`).
- Grillas: siempre `<app-data-table>`. Los estilos de DataTables se adaptan a Pico en `styles.scss` mapeando las variables `--dt-*` y `--dtb-*` a tokens `--pico-*`; no sobreescribir reglas de DataTables con `!important`.
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
- **Registro de usuarios** (`/register`) crea usuarios sin roles. Un administrador se los asigna desde Users.
- **Git**: la solución todavía no tiene repositorio. Crearlo antes de seguir (`git init`, `.gitignore` ya existe).
- `Category` tiene `Description` y `Amount`. Ampliar según el dominio real.
- **Regla de esquema**: todo cambio de base va en una migración incremental nueva. La base publicada NO se borra ni se regenera `InitialCreate`, salvo pedido explícito del dueño del proyecto.

---

## 9. Instrucción para el asistente

Cuando trabajes sobre esta solución:

1. Leé este documento completo antes de tocar código.
2. Respetá la estructura de capas y el patrón de `Categories` para cualquier feature nueva.
3. Todo cambio en el backend que afecte la API implica rebuild + `npm run generate-api`.
4. Todo cambio de permisos implica tocar `Permissions.cs` y `permissions.ts` a la vez.
5. Compilá y probá con usuario administrador y con usuario sin permisos antes de reportar algo como terminado.
6. No hagas `git commit`, `git push` ni `azd up` sin que te lo pidan explícitamente.
