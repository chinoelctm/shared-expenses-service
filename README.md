# shared-expenses.service

API de gastos compartidos: reparto proporcional al presupuesto de cada persona, cuotas,
arriendo por mes y costos fijos. .NET 8 + MongoDB.

## Ejecutar

Requiere un MongoDB accesible. Configura `ConnectionStrings` en `src/SharedExpenses.Api/appsettings.json`
(o por variables de entorno `ConnectionStrings__MongoUri` / `ConnectionStrings__MongoDb`).

```bash
cd src
dotnet run --project SharedExpenses.Api --launch-profile http   # http://localhost:5080/swagger
dotnet test SharedExpenses.Service.sln
```

Al arrancar, un worker en segundo plano (`MongoIndexInitializer`) sincroniza los índices: crea los que
faltan y elimina los propios (`ix_*`/`ux_*`) que ya no se usan. Si MongoDB no responde, la API arranca
igual y reintenta con espera creciente (5 s, 10 s, … hasta 5 min); mientras tanto, los endpoints que usan
la base responden **503** y `GET /api/v1/health` informa `Degraded`.

La primera vez que se consultan, la configuración y las personas se crean con la sección
`DefaultSettings` de `appsettings.json`.

## Arquitectura

```
src/
├── SharedExpenses.Api             Minimal APIs (Endpoints/), Models/ (requests), GlobalExceptionHandler, Extensions/ (DI)
├── SharedExpenses.Application     Commands/ y Queries/ (MediatR), validadores (FluentValidation), Behaviors/,
│                                  Services/ (configuración, apertura de meses, resumen), DTOs/
├── SharedExpenses.Infrastructure  Mongo: DbContext + índices, repositorios, serializadores, opciones
└── SharedExpenses.Domain          Entidades (Person, Settings, Expense), value objects (YearMonth,
                                   ContributionShares, Installment), enums, excepciones, contratos de repositorio
tests/SharedExpenses.Tests         Dominio + cálculo del resumen (incluye paridad con los totales del front)
```

Dependencias: `Api → Application → Infrastructure → Domain`.

## Modelo en Mongo

| Colección | Contenido |
|---|---|
| `people` | Personas: nombre, aporte (`contribution`) y si está activa |
| `settings` | Documento único: arriendo base y costos fijos |
| `expenses` | Un documento por gasto (o por cuota), con `month` (`yyyy-MM`) indexado |

### Índices

Cada consulta de los repositorios se validó con `explain()`: todas usan índice (sin `COLLSCAN`) y
ninguna ordena en memoria.

| Colección | Índice | Claves | Consultas que resuelve |
|---|---|---|---|
| `people` | `ux_people_name` | `normalizedName` (único, parcial `enabled: true`) | Nombre duplicado; no permite dos activas con el mismo nombre |
| `people` | `ix_people_active_order` | `enabled, createdOn, normalizedName` | Personas activas en orden; listado para el respaldo |
| `categories` | `ux_categories_name` | `normalizedName` (único, parcial `enabled: true`) | Nombre de categoría duplicado entre las activas |
| `categories` | `ix_categories_order` | `createdOn, normalizedName` | Categorías en orden de creación; respaldo |
| `expenses` | `ix_expenses_month_amount` | `month, isRent ↓, isFixed ↓, amount ↓, category, createdOn` | Gastos del mes ya ordenados (arriendo, fijos, variables de mayor a menor monto y por categoría); respaldo; `distinct` de meses |
| `expenses` | `ux_expenses_month_rent` | `month` (único, parcial `isRent: true`) | ¿El mes está abierto?; garantiza un solo arriendo por mes |
| `expenses` | `ix_expenses_plan` | `installment.planId` (sparse) | Borrar un plan de cuotas completo |

`ix_expenses_plan` es *sparse* y no parcial: MongoDB no usa un índice parcial con filtro
`installment: {$exists: true}` para una consulta por `installment.planId`.

## Reglas de negocio

- **Personas y porcentajes**: cada persona registra el dinero con el que puede aportar. Su porcentaje es
  `aporte / suma de aportes de las personas activas`. No hay límite de personas.
- **Reparto**: cada gasto se divide según esos porcentajes con el método del resto mayor (cada persona recibe
  la parte entera y los pesos sobrantes van a las fracciones más grandes), así la suma calza exacto. Los
  totales por persona son la suma del reparto de cada gasto.
- Los porcentajes se calculan al momento: cambiar un aporte, agregar o desactivar a alguien recalcula todos
  los meses. Eliminar una persona la desactiva (no se borra) y no puede haber dos activas con el mismo nombre.
- Si la colección `people` está vacía, se siembran las personas de `DefaultSettings:People`.
- **Quién pagó (`paidBy`)**: opcional. Si se indica, la parte de esa persona queda saldada de inmediato y
  las de los demás quedan pendientes; `PATCH /expenses/{id}/payment` con `paid: true` salda la parte de los
  demás. Sin `paidBy`, `paid` marca el gasto completo. El total pendiente del mes (`payments.pending`) y el
  pendiente por persona suman las partes no saldadas.
- **Apertura de mes**: al consultar o escribir en un mes que no existe, se crea con el arriendo base y los
  costos fijos vigentes. Cambiar la configuración no altera los meses ya abiertos.
- **Arriendo**: uno por mes, se edita con `PUT /months/{month}/rent`; no se elimina (DELETE lo deja en $0)
  y "Arriendo" no se acepta como descripción de otros gastos.
- **Cuotas**: `installments` de 2 a 48. Una cuota por mes desde la fecha indicada (el día se ajusta al último
  del mes si no existe), el remanente va en la última cuota y "pagado" aplica solo a la primera.
- Montos en CLP como enteros (`long`). Fechas de auditoría en UTC.

## Autenticación

[shared-expenses-auth-service](https://github.com/chinoelctm/shared-expenses-auth-service) emite el access token
(JWT RS256, 15 min) y lo deja en la cookie HttpOnly `auth_token`. Esta API lo valida con la **clave pública** de
`JwtSettings:PublicKeyPem` (firma, emisor `SharedExpenses.Auth`, audiencia `SharedExpenses` y vencimiento, con
30 s de tolerancia): puede verificar tokens pero no emitirlos. Todos los endpoints exigen sesión por política por
defecto; solo `GET /api/v1/health` es anónimo (lo usa el health check de Render).

Sin cookie, con un token vencido o con una firma inválida responde **401** (`ProblemDetails`); el front entonces
llama a `/api/v1/auth/refresh` y reintenta. El front llega a esta API por el mismo origen (rewrite `/api/*` de su
static site en Render, `proxy.conf.json` en desarrollo): por eso la cookie del sitio del front llega aquí.

| Parámetro | Valor | Uso |
|---|---|---|
| `JwtSettings:PublicKeyPem` | clave pública del servicio de auth | Verifica la firma RS256 (es pública: va en el repo) |
| `JwtSettings:Issuer` / `Audience` | `SharedExpenses.Auth` / `SharedExpenses` | Deben coincidir con los del servicio de auth |
| `JwtSettings:AccessTokenCookie` | `auth_token` | Cookie de la que se lee el token |

Si se rota la clave del servicio de auth, hay que actualizar `JwtSettings:PublicKeyPem` (o la variable
`JwtSettings__PublicKeyPem` en Render).

## Endpoints (`/api/v1`)

Todos requieren la cookie `auth_token` (401 sin sesión), salvo `/health`.

| Método | Ruta | Respuesta |
|---|---|---|
| GET    | `/people` | Personas activas con aporte y porcentaje, y el total de aportes |
| POST   | `/people` | 201 `{ id }`, body `{ "name": "Emma", "contribution": 150000 }` |
| GET    | `/people/{id}` | Persona con su porcentaje |
| PUT    | `/people/{id}` | 204 (409 si el nombre ya existe) |
| DELETE | `/people/{id}` | 204, la desactiva |
| GET    | `/settings` | Arriendo base y costos fijos |
| PUT    | `/settings` | 204 |
| GET    | `/months` | Meses con gastos (`["2026-10", ...]`) |
| GET    | `/months/{month}/expenses?status=All\|Pending\|Paid` | Gastos del mes; `split` = monto de cada persona |
| GET    | `/months/{month}/summary` | Totales, por persona (total/fijos/variables/pagado/pendiente), pagos (%), categorías |
| PUT    | `/months/{month}/rent` | 204 |
| POST   | `/expenses` | 201 `{ ids, planId, amounts }` |
| GET    | `/expenses/{id}` | Gasto con su reparto |
| PUT    | `/expenses/{id}` | 204 (si es cuota, edita solo esa cuota) |
| PATCH  | `/expenses/{id}/payment` | 204, body `{ "paid": true }` |
| DELETE | `/expenses/{id}?scope=Single\|Plan` | 204 |
| GET    | `/categories` | Categorías activas `{ id, name, color, locked }` (se siembran las por defecto si no hay ninguna) |
| POST   | `/categories` | 201 `{ id }`, body `{ "name": "Mascotas", "color": "#a855f7" }` (409 si el nombre ya existe) |
| PUT    | `/categories/{id}` | 204 |
| DELETE | `/categories/{id}` | 204, la desactiva: los gastos que ya la usan la conservan. `Housing` (arriendo) no se puede eliminar |
| GET    | `/health` | `{ status, database }`: 200 `Healthy` o 503 `Degraded` si MongoDB no responde |
| GET    | `/backup` | Respaldo completo (personas, categorías, configuración, gastos) |
| POST   | `/backup` | 204, reemplaza personas, categorías, configuración y gastos |

Errores: `ProblemDetails` (400 validación/dominio con `errors` por campo, 404, 409, 500).
Enums viajan como texto (`"Supermarket"`), fechas como `"2026-10-05"`.

## Despliegue (gratis): Render + MongoDB Atlas

1. **MongoDB Atlas** (base de datos):
   - Crear un cluster **M0 (Free)**.
   - *Database Access*: crear un usuario con contraseña.
   - *Network Access*: agregar `0.0.0.0/0` (Render no tiene IP fija).
   - *Connect → Drivers*: copiar la cadena `mongodb+srv://usuario:clave@cluster.xxxxx.mongodb.net/...`.
2. **Render** (API): **New → Blueprint** y elegir este repo. El `render.yaml` crea un Web Service gratuito
   con el `Dockerfile`. Render pedirá el valor de `ConnectionStrings__MongoUri`: pegar la cadena de Atlas.
3. Probar `https://shared-expenses-service.onrender.com/api/v1/health` (el resto de los endpoints exige sesión).

Las variables de entorno de Render reemplazan a `appsettings.json` (`ConnectionStrings__MongoUri`,
`ConnectionStrings__MongoDb`, `Cors__AllowedOrigins__0`). Si el front queda en otra URL, cambiar
`Cors__AllowedOrigins__0` en el dashboard de Render.

El plan gratuito de Render apaga la API tras 15 minutos sin tráfico; la primera llamada después tarda
cerca de un minuto. Atlas M0 no tiene respaldos automáticos: usar *Descargar Respaldo JSON* en la app.

## Front

[shared-expenses-front](https://github.com/chinoelctm/shared-expenses-front) consume esta API por su mismo origen (rewrites de
Render / proxy de `ng serve`), así que no depende de CORS; `Cors:AllowedOrigins` queda solo para llamadas directas
desde `http://localhost:4200`.

## Limitaciones conocidas

- `POST /backup` no es transaccional (Mongo standalone no soporta transacciones).
