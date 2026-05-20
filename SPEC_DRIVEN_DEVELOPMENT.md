# MyCarApp - Especificación Completa

**Versión**: 1.1  
**Última actualización**: 2026-05-06  
**Estado**: Pre-Producción  

**Changelog**:
- **v1.1** (2026-05-06): Alineación con código real (sin estado `Approved`, snapshots en `WorkOrderService`). Nueva entidad `Mechanic` y flujo de asignación/aceptación/finalización de servicios por mecánicos. Notas obligatorias al finalizar.

---

## 📋 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Actores y Roles](#actores-y-roles)
3. [Entidades de Dominio](#entidades-de-dominio)
4. [Flujos de Negocio](#flujos-de-negocio)
5. [Estados y Máquina de Estados](#estados-y-máquina-de-estados)
6. [API Endpoints](#api-endpoints)
7. [Autenticación y Autorización](#autenticación-y-autorización)
8. [Email y Comunicaciones](#email-y-comunicaciones)
9. [Validaciones](#validaciones)
10. [Seguridad](#seguridad)
11. [Logging](#logging)
12. [Configuración](#configuración)

---

## Visión General

**MyCarApp** es una plataforma de gestión de órdenes de trabajo para talleres automotores. Permite a:
- **Clientes individuales**: Puede ver sus vehiculos, ordenes. Cambios en TIEMPO REAL. Aprobar presupuestos.
- **Flotas**: Un contacto encargado (operario) gestiona vehículos de la flota, ordena trabajos. Puede ver sus vehiculos, ordenes. Cambios en TIEMPO REAL. Aprobar presupuestos.
- **Talleres**: Registrar vehículos, crear órdenes de trabajo. Pueden crear sus propios catálogos de servicios. Diagnosticar, presupuestar, ejecutar y entregar trabajos.

**Arquitectura**: Clean Architecture con CQRS (MediatR)
- **API**: ASP.NET Core 9, JWT Bearer auth
- **Database**: PostgreSQL con EF Core 9
- **ORM**: Entity Framework Core 9
- **Mapeo**: Mapster
- **Validación**: FluentValidation
- **Logging**: Serilog (Console + File)
- **Mapper**: Mapster (automático en responses)

---

## Actores y Roles

### 1. Customer (Cliente Individual)
**Características**:
- Email único
- Documento de identidad (DNI/RUT) único
- Teléfono (único)
- Nombre y apellido
- **NO tiene FleetId**
- **NO puede ser contacto de flota**

**Permisos**:
- Ver y crear sus propios vehículos
- Crear órdenes de trabajo para sus vehículos
- Ver estado de sus órdenes
- Aprobar presupuestos vía link en email (token de 48h)

**Restricciones**:
- No puede ver datos de flotas
- No puede ver vehículos/órdenes ajenos
- No puede crear órdenes para vehículos de flota

---

### 2. Fleet Contact (Encargado de Flota)
**Características**:
- Es un `Customer` con `FleetId != null`
- Una flota solo puede tener UN encargado
- El encargado es empleado de la flota
- Cada encargado tiene su propio email (único en sistema)
- Tiene nombre y apellido propios (no es el nombre de la flota)

**Permisos**:
- Ver su flota (`/api/fleets/mine`)
- Ver todos los vehículos de su flota
- Crear vehículos para su flota
- Crear órdenes de trabajo para vehículos de su flota
- Aprobar presupuestos vía email
- Ver órdenes de su flota
- Especificar empleado que llevó el vehículo (ContactPersonName, ContactPersonPhone)

**Restricciones**:
- No puede crear vehículos individuales (aunque quiera un vehículo personal, necesita cuenta separada)
- No puede ver datos de otras flotas
- No puede ver clientes individuales
- No puede cambiar de flota

### PARTE MAS IMPORTANTE! EL CLIENTE DEBE SENTIRSE COMODO Y CONFIADO!!!


Módulo Cliente - Lo que ya tiene
Mis Órdenes (/my-orders)
Lista de todas las órdenes
Muestra: vehículo, estado, monto, fecha
Separa en "Activas" e "Historial"
Detalle de Orden (/my-orders/[id])
Header: ID orden, vehículo, patente, estado, total, fecha
Servicios: Lista de servicios con precios individual y total
Nota del mecánico: Si el técnico dejó alguna nota
Tu nota: Lo que el cliente informó al traer el vehículo
Historial: Timeline de todos los cambios de estado

!!! Lo que falta implementar
1. Fotos (Antes/Después)
Ya existe el componente PhotosCard para admin
Falta agregarlo en la vista del cliente (/my-orders/[id]/page.tsx)
El cliente debería ver las fotos que el taller suba
2. Cambiar Contraseña
Ya existe el endpoint en backend
Falta crear UI en el módulo cliente:
Página o modal para cambiar contraseña
Campos: contraseña actual, nueva contraseña
Validar que la contraseña actual sea correcta

---

### 3. Admin (Taller)
**Características**:
- Email único
- Rol "Admin" en JWT
- Acceso irrestricto

**Permisos**:
- Ver, crear, actualizar, eliminar clientes
- Ver, crear, actualizar, eliminar flotas
- Ver, crear, actualizar, eliminar vehículos
- Crear, modificar, completar órdenes de trabajo
- Cambiar estado de órdenes manualmente
- Generar presupuestos y aprobarlos
- Crear/editar/desactivar mecánicos
- Asignar mecánicos a servicios de una orden

---

### 4. Mechanic (Mecánico del Taller)

**Características**:
- Email único
- Rol "Mechanic" en JWT
- Tiene `ApplicationUser` propio (login independiente)
- Especialidad opcional (texto libre, ej: "Motor", "Frenos", "Electricidad")
- `IsActive`: false al desactivar (no aparece en asignaciones nuevas)

**Permisos**:
- Ver SOLO los `WorkOrderService` que le fueron asignados (`AssignedMechanicId == self`)
- Aceptar un servicio asignado → estado `Pending` → `Accepted`
- Finalizar un servicio aceptado → estado `Accepted` → `Completed` (con notas obligatorias)
- Ver su perfil

**Restricciones**:
- NO puede ver clientes, flotas, ni vehículos
- NO puede ver `WorkOrder` completas — solo el listado de SUS servicios con info mínima del vehículo
- NO puede cambiar el estado global de la `WorkOrder` (eso es del Admin)
- NO puede agregar/eliminar servicios de una orden
- NO puede modificar el `priceSnapshot` ni la `quantity`
- NO puede reasignarse trabajos a sí mismo (lo hace el Admin)
- NO puede rechazar un trabajo asignado (si hay un problema, lo habla con el Admin)

**Filosofía**:
> El mecánico se comunica **a través del sistema**, no por WhatsApp/voz. Cuando finaliza un servicio, las notas son **obligatorias** y describen qué hizo. Esto cierra el loop de comunicación con la recepción/admin: el admin no necesita preguntar "¿qué le hiciste?" porque está todo escrito.

---

## Entidades de Dominio

### 1. Customer
```
{
  Id: Guid (PK),
  FirstName: string (requerido),
  LastName: string (requerido),
  Email: string (único, requerido),
  DocumentNumber: string (único, requerido),
  Phone: string (único, requerido),
  
  FleetId: Guid? (nullable)
    - Si null: cliente individual
    - Si Guid: contacto de flota (encargado)
  
  ApplicationUserId: Guid? (FK → AspNetUsers)
    - Si null: no tiene cuenta de login
    - Si Guid: tiene credenciales registradas
  
  CreatedAt: DateTime (UTC),
  UpdatedAt: DateTime (UTC),
  IsDeleted: bool (soft delete)
}
```

**Índices**:
- Email (único)
- DocumentNumber (único)
- Phone (único)
- FleetId (para búsqueda rápida de encargado)
- ApplicationUserId (para resolver customer por JWT)

---

### 2. Fleet
```
{
  Id: Guid (PK),
  CompanyName: string (requerido),
  Cuit: string (único, requerido),
  Email: string (nullable)
    - Email de la flota (NO se usa para presupuestos)
    - Deprecated: El presupuesto va al encargado
  
  ContactId: Guid (FK → Customer)
    - ÚNICO: una flota solo tiene un contacto
    - Constraint: Customer.FleetId == FleetId
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  IsDeleted: bool
}
```

**Índices**:
- Cuit (único)
- ContactId (único)

---

### 3. Vehicle
```
{
  Id: Guid (PK),
  LicensePlate: string (único, requerido),
  Brand: string (requerido, ej: "Toyota"),
  Model: string (requerido, ej: "Corolla"),
  Year: int (requerido),
  VIN: string? (opcional),
  
  CustomerId: Guid? (FK → Customer)
    - Si poblado: vehículo de cliente individual
    - Si null: vehículo de flota
  
  FleetId: Guid? (FK → Fleet)
    - Si poblado: vehículo de flota
    - Si null: vehículo de cliente individual
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  IsDeleted: bool
}
```

**XOR Constraint**: `(CustomerId != null) XOR (FleetId != null)`
- Cada vehículo pertenece a EXACTAMENTE uno: cliente individual O flota

**Validación en creación**:
- Si se intenta asignar a cliente que es fleet contact: ❌ BadRequestException
- Si se intenta asignar a flota Y cliente: ❌ BadRequestException
- Si no se asigna a ninguno: ❌ BadRequestException

**Índices**:
- LicensePlate (único)
- CustomerId
- FleetId

---

### 4. WorkOrder
```
{
  Id: Guid (PK),
  VehicleId: Guid (FK → Vehicle),
  
  // Snapshot de propietario al momento de crear la orden
  CustomerIdAtEntry: Guid? (snapshot)
    - Congelado al crear: si cliente individual
    - No cambia aunque el cliente sea eliminado
  
  FleetIdAtEntry: Guid? (snapshot)
    - Congelado al crear: si es flota
    - No cambia aunque la flota sea eliminada
  
  // Información de contacto (solo para flotas)
  ContactPersonName: string? (ej: "Juan Pérez")
    - Empleado que llevó el vehículo
    - Nombre del que contactar sobre la orden
  
  ContactPersonPhone: string? (ej: "+54 9 11 1234-5678")
    - Teléfono del contacto
    - Para referencia rápida
  
  CurrentStatus: WorkOrderStatus (enum 0-7),
  
  // Diagnóstico y trabajo
  DiagnosisNote: string? (ej: "Ruido en motor, revisar correas"),
  EstimatedCost: decimal (ej: 15000.00),
  TotalAmount: decimal (costo final con todos los servicios),
  
  CreatedAt: DateTime (UTC),
  UpdatedAt: DateTime (UTC),
  IsDeleted: bool (soft delete)
}
```

---

### 5. WorkOrderService
```
{
  Id: Guid (PK),
  WorkOrderId: Guid (FK → WorkOrder),
  CatalogServiceId: Guid (FK → CatalogService),
  
  // Snapshots del catálogo al momento de agregar
  NameSnapshot: string,
  DescriptionSnapshot: string,
  PriceSnapshot: decimal (precio en el momento)
  Quantity: int (cantidad, ej: 2 unidades)
  Subtotal: decimal = PriceSnapshot * Quantity (calculado)

  // ── Asignación al mecánico ─────────────────────────────────
  AssignedMechanicId: Guid? (FK → Mechanic, nullable)
    - Null: aún no asignado a ningún mecánico
    - Guid: mecánico asignado por el admin
  
  AssignmentStatus: enum (0-2)
    - 0 = Unassigned    (sin mecánico asignado)
    - 1 = Pending       (asignado pero el mecánico no aceptó aún)
    - 2 = Accepted      (mecánico aceptó, está trabajando)
    - 3 = Completed     (mecánico finalizó el servicio)
  
  AcceptedAt: DateTime? (UTC, cuando el mecánico aceptó)
  CompletedAt: DateTime? (UTC, cuando el mecánico finalizó)
  MechanicNotes: string? (obligatorio al completar, lo que hizo)
  MechanicFindings: string? (opcional, recomendaciones extra detectadas)
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  IsDeleted: bool
}
```

**Reglas de transición de `AssignmentStatus`**:
```
Unassigned → Pending     (admin asigna mecánico)
Pending    → Accepted    (mecánico acepta)
Pending    → Pending     (admin reasigna a otro mecánico, resetea AcceptedAt=null)
Pending    → Unassigned  (admin desasigna)
Accepted   → Completed   (mecánico finaliza con notas)
Accepted   → Pending     (admin reasigna; reset AcceptedAt)
Completed  → ❌          (terminal a nivel servicio; el admin podrá "reabrir" en futuro si hace falta)
```

---

### 5.b Mechanic
```
{
  Id: Guid (PK),
  ApplicationUserId: Guid (FK → AspNetUsers, único)
  FirstName: string (requerido, max 100),
  LastName: string (requerido, max 100),
  Email: string (único, requerido, max 150),
  Phone: string? (opcional, max 30),
  Specialty: string? (opcional, max 200, ej: "Motor", "Electricidad"),
  IsActive: bool (default true),
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  IsDeleted: bool
}
```

**Índices**:
- Email (único)
- ApplicationUserId (único, para resolver mecánico por JWT)
- IsActive (para filtrar activos al asignar)

---

### 6. WorkOrderStatusChange (Auditoría)
```
{
  Id: Guid (PK),
  WorkOrderId: Guid (FK → WorkOrder),
  PreviousStatus: WorkOrderStatus,
  NewStatus: WorkOrderStatus,
  ChangedBy: Guid (FK → ApplicationUser)
  ChangedAt: DateTime (UTC),
  Note: string? (ej: "Cliente aprobó presupuesto via email")
}
```

---

### 7. WorkOrderApprovalToken
```
{
  Id: Guid (PK),
  WorkOrderId: Guid (FK → WorkOrder, unique),
  Token: string (unique, 64 caracteres hex)
    - Generado con: RandomNumberGenerator.GetBytes(32)
    - Convertido a hex lowercase
  
  ExpiresAt: DateTime (UTC, +48 horas)
  IsUsed: bool (true tras usarse)
  UsedAt: DateTime? (timestamp de uso)
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}
```

---

### 8. WorkOrderPhoto
```
{
  Id: Guid (PK),
  WorkOrderId: Guid (FK → WorkOrder),
  PhotoUrl: string (URL a storage, ej: S3)
  Caption: string? (ej: "Motor desmontado"),
  
  CreatedAt: DateTime,
  UpdatedAt: DateTime,
  IsDeleted: bool
}
```

---

## Flujos de Negocio

### Flujo 1: Creacion orden de trabajo. Con dos tipos. Para cliente particular. Para flota.

El admin realiza un nuevo ingreso desde /admin/intake:

Paso 1 - Tipo de cliente:

Cliente particular: Persona física con vehículos propios
Empresa/Flota: Empresa con múltiples vehículos y contacto asignado
Paso 2A - Datos del cliente particular:

Buscar cliente existente (por nombre, apellido, email o documento)
Si no existe → registrar nuevo con:
Nombre, Apellido
Tipo de documento (DNI, Pasaporte, CUIT, CUIL)
Número de documento
Email
Teléfono
Paso 2B - Datos de empresa/flota:

Buscar empresa existente (por nombre o CUIT)
Si no existe → registrar nueva con:
Razón social, CUIT, Teléfono, Email, Dirección
Datos del contacto/conductor:
Nombre, Apellido, Tipo y número de documento, Email, Teléfono
Paso 3 - Datos del vehículo:

Marca, Modelo, Año
Patente (formato nacional ABC123 o Mercosur AB123CD)
Color
Tipo de combustible, Carrocería, Uso
Kilometraje actual, VIN
Titular registral (propietario en el registro):
Puede ser el mismo cliente
O especificar otro titular con sus datos
Motivo de ingreso: Qué trabajo necesita el vehículo
Quién trae el vehículo: Nombre y teléfono de quien lo entrega
Paso 4 - Confirmar:

Resumen de todo → crear orden de trabajo

### Flujo 2: Taller Diagnostica y Genera Presupuesto

```
┌─────────────────────────────────────────────────────────────┐
│ TALLER DIAGNOSTICA Y GENERA PRESUPUESTO                     │
└─────────────────────────────────────────────────────────────┘
1. El vehículo llega al taller
Se registra el ingreso con todos los datos del cliente y vehículo. La orden se crea automáticamente con estado "Recibido".

2. El mecánico inicia el diagnóstico
El admin cambia el estado de la orden a "En diagnóstico" (desde el detalle de la orden).

En este estado, el admin puede agregar servicios del catálogo:

Busca un servicio (ej: "Cambio de aceite", "Frenos", "Diagnóstico電子系")
Elige la cantidad
Agrega a la orden
Cada servicio guarda:

Nombre del servicio
Precio unitario
Cantidad
Subtotal
3. Se genera el presupuesto
Una vez que el mecánico agregó todos los servicios necesarios, el admin cambia el estado a "Esperando aprobación".

Esto automáticamente:

Calcula el total de la orden (suma de todos los servicios)
Genera un token de aprobación único (link)
Envía un email al cliente con el link para aprobar
El email incluye:

Datos del vehículo
Lista de servicios con precios
Total a pagar
Link para aprobar
4. El cliente aprueba
El cliente recibe el email y hace clic en el link de aprobación. Ve:

Vehículo y servicios
Monto total
Aviso legal
Y puede aprobar el presupuesto.

5. Inicio del trabajo
Cuando el admin ve que el cliente aprobó (en el dashboard o detalle de orden), cambia el estado a "En progreso" y el taller comienza a trabajar.


 !!!! (A IMPLEMENTAR EN LA APP) Flujo Propuesto:
Estado	Descripción
Recibido	Vehículo llegó al taller
En diagnóstico	Mecánico revisando
Esperando aprobación	Presupuesto listo, esperando que cliente apruebe
Aprobado	Cliente aprobó pero aún no trajo el veículo
En progreso	Vehículo en taller, trabajo comenzando
Completado	Trabajo terminado
Entregado	Cliente retiró
Para el cliente:
Cuando aprueba el presupuesto, ve: "Presupuesto aprobado. Llevá el vehículo al taller para comenzar el trabajo."
El trabajo no starts hasta que el vehículo llega físicamente
Para el admin:
Puede ver qué vehículos están "aprobados pero no llegaron"
Helps gestionar la cola de trabajo
---

### Flujo 4: Email de Presupuesto y Aprobación

```
┌─────────────────────────────────────────────────────────────┐
│ EMAIL DE PRESUPUESTO Y FLUJO DE APROBACIÓN                  │
└─────────────────────────────────────────────────────────────┘

EMISOR DEL EMAIL:
1. Sistema detecta cambio a status AwaitingApproval
2. Llama: ChangeWorkOrderStatusCommandHandler.TryEnqueueQuoteEmailAsync()

DESTINATARIO - CLIENTE INDIVIDUAL:
- Email → customer.Email (del CustomerIdAtEntry)

DESTINATARIO - FLEET CONTACT (ENCARGADO):
- NO envía a fleet.Email ❌
- Email → contact.Email (del encargado registrado)
- Contact se obtiene: GetByFleetIdAsync(FleetIdAtEntry)

CONTENIDO DEL EMAIL:
- Asunto: "Presupuesto para su vehículo {Brand} {Model} — MyCarApp"
- HTML body con:
  - Saludo: "Hola, {recipientName}!"
  - Descripción: "El diagnóstico de tu {Brand} {Model} está listo"
  - Monto: "El total estimado es de $ {TotalAmount:N0}"
  - Botón "Aprobar presupuesto" con href="{approvalLink}"
    (approvalLink = ApprovalBaseUrl + token, ej: "http://localhost:3000/approve?token=abc123...")
  - Botón "Contactar por WhatsApp" (placeholder: "https://wa.me/WHATSAPP_NUMBER_PLACEHOLDER")
  - Footer: "Este enlace es válido por 48 horas..."

ATTACHMENT:
- PDF generado por IPdfService.GenerateQuotePdf(QuotePdfData)
- Incluye: WorkOrder, Vehicle, Services, TotalAmount
- Nombre: "Presupuesto-{LicensePlate}-{YYYYMMDD}.pdf"

PATRÓN FIRE-AND-FORGET:
- Email se envía FUERA de transacción HTTP
- Usa: _ = SendEmailAsync(...)
- No bloquea respuesta al cliente
- Si falla, se registra en logs con LogError
- CancellationToken = CancellationToken.None (NO el del request)

EMAIL SERVICE CALL:
```csharp
await _emailService.SendAsync(
    to: "cliente@example.com",
    subject: "Presupuesto para su vehículo Toyota Corolla — MyCarApp",
    htmlBody: "... HTML template ...",
    attachment: pdfBytes,
    attachmentName: "Presupuesto-ABC1234-20260503.pdf",
    cancellationToken: CancellationToken.None
);
```
```

---

### Flujo 5: Cliente Aprueba Presupuesto (Token)

```
┌─────────────────────────────────────────────────────────────┐
│ CLIENTE APRUEBA PRESUPUESTO VÍA EMAIL                       │
└─────────────────────────────────────────────────────────────┘

Al diagnosticar el vehiculo o al finalizar el diagnostico, el admin cambia el estado a AwaitingApproval.

Y el admin debe crear el presupuesto con el total estimado y los servicios que se le van a realizar al vehiculo. Y luego enviar el presupuesto al cliente.

!!!! Falta implementar la logica de que el cliente pueda ver el presupuesto y aprobarlo desde un link enviado por correo electronico o Whatssap.

---

### Flujo 6: Taller Ejecuta Trabajo

```
┌─────────────────────────────────────────────────────────────┐
│ TALLER EJECUTA TRABAJO                                      │
└─────────────────────────────────────────────────────────────┘

Al estar el vehiculo ya en el taller el admin cambia el estado a EnProgreso.

Se debe actualizar el vehiculo con todas las fotos que se tomaron al recibirlo.

Y el admin puede ver desde esta pantalla el detalle de los servicios que se le van a realizar al vehiculo.Y si encuentran mas detalles o servicios que realizar se pueden agregar en esta pantalla. Si hay algun imprevisto se puede actualizar el total de la orden.

Cuando se finaliza el trabajo se cambia el estado a Completado. Y se pueden subir fotos posterior a la realizacion del trabajo

> ✅ **Implementado en v1.1**: Participación de mecánicos. Ver Flujo 7.

Por ultimo falta implementar la logica de que el cliente pueda ver el estado de su vehiculo en el taller y pueda ver el detalle de los servicios que se le estan realizando.

---

### Flujo 7: Mecánico ejecuta servicio asignado

```
┌─────────────────────────────────────────────────────────────┐
│ MECÁNICO EJECUTA SERVICIO ASIGNADO                          │
└─────────────────────────────────────────────────────────────┘

PRECONDICIÓN:
- WorkOrder está en estado InProgress (cliente ya aprobó)
- Existe al menos un WorkOrderService en la orden

PASO 1 — ADMIN ASIGNA MECÁNICO A UN SERVICIO
- Admin entra al detalle de la WorkOrder
- Por cada WorkOrderService elige un Mechanic activo
- POST /api/work-order-services/{serviceId}/assign  (Admin)
  - body: { mechanicId: Guid }
- Backend:
  - Carga WorkOrderService con AssignmentStatus IN (Unassigned, Pending, Accepted)
  - Si está Completed: 400 BadRequest (no se reasigna lo finalizado)
  - Verifica que el mecánico existe y está activo
  - Setea AssignedMechanicId, AssignmentStatus=Pending
  - Si venía de Accepted (reasignación), resetea AcceptedAt=null
  - SaveChanges

PASO 2 — MECÁNICO VE SU LISTA DE TRABAJOS
- GET /api/mechanics/me/tasks
  - Solo servicios donde AssignedMechanicId == JWT.mechanicId
  - Filtra por AssignmentStatus IN (Pending, Accepted)  por defecto
  - Soporta ?status=Completed para ver historial propio
  - Devuelve info mínima: WorkOrderId, vehículo (marca/modelo/patente), 
    nombre del servicio, descripción, cantidad, AssignmentStatus, 
    AcceptedAt, customerNote (lo que pidió el cliente)
  - NO devuelve datos del cliente ni precios

PASO 3 — MECÁNICO ACEPTA EL TRABAJO
- POST /api/work-order-services/{serviceId}/accept  (Mechanic)
- Backend:
  - Carga WorkOrderService
  - Valida que AssignedMechanicId == JWT.mechanicId  → si no, 404
  - Valida que AssignmentStatus == Pending  → si no, 400
  - Valida que la WorkOrder está en InProgress  → si no, 400
  - Setea AssignmentStatus=Accepted, AcceptedAt=UtcNow
  - SaveChanges

PASO 4 — MECÁNICO FINALIZA EL TRABAJO
- POST /api/work-order-services/{serviceId}/complete  (Mechanic)
  - body: { notes: string (REQUIRED, min 10 chars), findings: string? (optional) }
- Backend:
  - Carga WorkOrderService
  - Valida ownership (AssignedMechanicId == JWT.mechanicId)
  - Valida que AssignmentStatus == Accepted  → si no, 400
  - notes obligatorio (FluentValidation: min 10 chars, max 2000)
  - findings opcional (max 2000)
  - Setea AssignmentStatus=Completed, CompletedAt=UtcNow,
    MechanicNotes=notes, MechanicFindings=findings
  - SaveChanges

PASO 5 — ADMIN PASA LA ORDEN A COMPLETED
- Solo es válido si TODOS los WorkOrderService activos están en Completed
- Si falta alguno: 400 con mensaje "Hay servicios pendientes de finalizar por mecánicos"

VISIBILIDAD AL CLIENTE:
- En GET /api/work-orders/{id} (ownership) — el cliente ve por cada servicio:
  - Nombre + descripción
  - Estado de la asignación (sin nombre del mecánico)
  - MechanicNotes y MechanicFindings (cuando estén)
  - Esto da transparencia: el cliente ve QUÉ se hizo en cada servicio.

AUTORIZACIÓN — RESUMEN
| Acción                              | Admin | Mechanic (asignado) | Mechanic (otro) |
|-------------------------------------|-------|---------------------|-----------------|
| Asignar/reasignar/desasignar        | ✅    | ❌                  | ❌              |
| Ver mis tareas                      | n/a   | ✅                  | ❌              |
| Aceptar trabajo                     | ❌    | ✅                  | ❌              |
| Finalizar trabajo                   | ❌    | ✅                  | ❌              |
| Editar notas después de Completed   | ❌    | ❌                  | ❌              |
```

---

### Endpoints — Mecánicos

```
# Gestión (Admin)
GET    /api/mechanics                    → Lista (paginada, ?includeInactive=true)
GET    /api/mechanics/{id}               → Detalle
POST   /api/mechanics                    → Crea mecánico + ApplicationUser + role
PATCH  /api/mechanics/{id}               → Actualiza datos
DELETE /api/mechanics/{id}               → Soft delete + IsActive=false

# Self-service (Mechanic)
GET    /api/mechanics/me                 → Su perfil
GET    /api/mechanics/me/tasks           → Sus servicios asignados (?status=)

# Asignación / ejecución del servicio
POST   /api/work-order-services/{id}/assign     (Admin)
POST   /api/work-order-services/{id}/unassign   (Admin)
POST   /api/work-order-services/{id}/accept     (Mechanic asignado)
POST   /api/work-order-services/{id}/complete   (Mechanic asignado)
```

---


### WorkOrderStatus Enum (estado real implementado)

```csharp
public enum WorkOrderStatus
{
    Received = 0,           // Orden creada, vehículo recibido
    Diagnosing = 1,         // En diagnóstico
    AwaitingApproval = 2,   // Presupuesto generado, esperando aprobación
    InProgress = 3,         // Trabajo en curso (cliente aprobó)
    Completed = 4,          // Trabajo completado (todos los servicios Completed)
    Delivered = 5,          // Vehículo entregado al cliente
    Cancelled = 6           // Orden cancelada
}
```

> ⚠️ **Decisión tomada**: NO existe un estado `Approved` separado. La transición `AwaitingApproval → InProgress` se hace cuando el cliente aprueba el presupuesto vía token. El "vehículo aprobado pero aún no llegó" se modela por separado (futuro: flag `IsVehicleAtShop` o similar) si hace falta.

DEBE VALIDARSE LA TRANSICION DE ESTADOS LOGICAMENTE

### Validaciones de Transición (real)

```csharp
// En WorkOrder.ChangeStatus(newStatus, userId, note)

Received         → Diagnosing,       Cancelled
Diagnosing       → AwaitingApproval, Cancelled
AwaitingApproval → InProgress,       Cancelled   (InProgress vía token de aprobación)
InProgress       → Completed,        Cancelled
Completed        → Delivered,        Cancelled
Delivered        → ❌ terminal
Cancelled        → ❌ terminal       (requiere nota obligatoria)
```

**Validación adicional para `InProgress → Completed`**:
- Todos los `WorkOrderService` no eliminados deben tener `AssignmentStatus == Completed`.
- Si hay servicios sin completar, el admin no puede pasar la orden a `Completed`.

---

## API Endpoints

### Autenticación

#### POST /api/auth/register
**Público** (sin autenticación)

**Request**:
```json
{
  "email": "juan@example.com",
  "password": "SecurePass123!",
  "firstName": "Juan",
  "lastName": "García",
  "documentNumber": "12345678",
  "phone": "+54 9 11 1234567",
  "role": "Customer",  // "Customer" o "Admin"
  "fleetId": null      // opcional, si es encargado de flota
}
```

**Response (201 Created)**:
```json
{
  "id": "user-id-uuid",
  "email": "juan@example.com",
  "firstName": "Juan",
  "lastName": "García",
  "customerId": "customer-uuid",
  "fleetId": null,
  "fullName": "Juan García",
  "role": "Customer",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh-token-uuid"
}
```

**Validaciones**:
- Email único
- Password: min 8 chars, mayús, minús, número, símbolo
- FirstName + LastName: no vacíos
- DocumentNumber único
- Phone único
- Si fleetId: verificar que existe y no tiene contacto asignado

---

#### POST /api/auth/login
**Público**

**Request**:
```json
{
  "email": "juan@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK)**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh-token-uuid",
  "customerId": "customer-uuid",
  "fleetId": null,
  "fullName": "Juan García",
  "role": "Customer"
}
```

**Errores**:
- 401 Unauthorized: Email o password incorrecto

---

### Clientes

#### POST /api/customers
**Requiere**: Admin

**Request**:
```json
{
  "firstName": "Carlos",
  "lastName": "López",
  "email": "carlos@example.com",
  "documentNumber": "87654321",
  "phone": "+54 9 11 9876543",
  "fleetId": null
}
```

**Response (201 Created)**:
```json
{
  "id": "customer-uuid",
  "firstName": "Carlos",
  "lastName": "López",
  "email": "carlos@example.com",
  "documentNumber": "87654321",
  "phone": "+54 9 11 9876543",
  "fleetId": null,
  "createdAt": "2026-05-03T10:30:00Z"
}
```

**Validaciones**:
- Email único
- DocumentNumber único
- Phone único
- Si fleetId: no puede haber otro contacto para esa flota
  - Error: "La flota ya tiene un contacto encargado asignado. Una flota solo puede tener un encargado."

---

#### GET /api/customers/{id}
**Requiere**: Admin o (Customer propietario)

**Response (200 OK)**:
```json
{
  "id": "customer-uuid",
  "firstName": "Carlos",
  "lastName": "López",
  "email": "carlos@example.com",
  "documentNumber": "87654321",
  "phone": "+54 9 11 9876543",
  "fleetId": null,
  "createdAt": "2026-05-03T10:30:00Z",
  "updatedAt": "2026-05-03T11:00:00Z"
}
```

---

#### PATCH /api/customers/{id}
**Requiere**: Admin o (Customer propietario)

**Request**:
```json
{
  "firstName": "Carlos",
  "lastName": "López López",
  "phone": "+54 9 11 9999999"
}
```

**Response (200 OK)**: Customer actualizado

**Validaciones**:
- No permitir cambiar Email o DocumentNumber (son PK lógicas)
- No permitir cambiar FleetId (rol es inmutable)

---

### Flotas

#### POST /api/fleets
**Requiere**: Admin

**Request**:
```json
{
  "companyName": "Banco Santander Uruguay",
  "cuit": "12345678901",
  "email": "flota@santander.com",
  "contactId": "customer-uuid"
}
```

**Response (201 Created)**:
```json
{
  "id": "fleet-uuid",
  "companyName": "Banco Santander Uruguay",
  "cuit": "12345678901",
  "email": "flota@santander.com",
  "contactId": "customer-uuid",
  "contact": {
    "id": "customer-uuid",
    "firstName": "Pierina",
    "lastName": "Martínez",
    "email": "pierina@example.com"
  },
  "createdAt": "2026-05-03T10:30:00Z"
}
```

**Validaciones**:
- Cuit único
- ContactId existe y es Customer
- ContactId.FleetId == null (el contacto no debe estar asignado a otra flota)

---

#### GET /api/fleets/mine
**Requiere**: Customer + FleetId (fleet contact)

**Response (200 OK)**:
```json
{
  "id": "fleet-uuid",
  "companyName": "Banco Santander Uruguay",
  "cuit": "12345678901",
  "email": "flota@santander.com",
  "contactId": "customer-uuid",
  "contact": {
    "id": "customer-uuid",
    "firstName": "Pierina",
    "lastName": "Martínez",
    "email": "pierina@example.com"
  }
}
```

**Restricción**: Solo devuelve la flota del usuario actual
- Si JWT.fleetId != null: devuelve esa flota
- Si JWT.fleetId == null: 404 Not Found

---

#### GET /api/fleets/{id}
**Requiere**: Admin o (Fleet contact de esa flota)

---

### Vehículos

#### POST /api/vehicles
**Requiere**: Admin o (Customer propietario) o (Fleet contact)

**Request - Cliente Individual**:
```json
{
  "licensePlate": "ABC1234",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "vin": "JT2BV29K270123456",
  "customerId": "customer-uuid"
}
```

**Request - Flota**:
```json
{
  "licensePlate": "XYZ9876",
  "brand": "Chevrolet",
  "model": "Cruze",
  "year": 2021,
  "vin": "3G1BR6FG6FS546789",
  "fleetId": "fleet-uuid"
}
```

**Response (201 Created)**:
```json
{
  "id": "vehicle-uuid",
  "licensePlate": "ABC1234",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "vin": "JT2BV29K270123456",
  "customerId": "customer-uuid",
  "fleetId": null,
  "createdAt": "2026-05-03T10:30:00Z"
}
```

**Validaciones**:
- LicensePlate único
- Brand, Model, Year requeridos
- CustomerId XOR FleetId (exactamente uno)
- Si CustomerId: verificar que no es fleet contact
  - Error: "Este cliente es contacto de una flota. No puede tener vehículos individuales. Cree una cuenta personal."
- Si FleetId: verificar que flota existe

**Ownership**:
- Si JWT es Customer: solo puede crear para sí mismo (CustomerId == JWT.customerId)
- Si JWT es Fleet Contact: solo puede crear para su flota (FleetId == JWT.fleetId)
- Si JWT es Admin: puede crear para cualquiera

---

#### GET /api/vehicles
**Requiere**: Autenticado

**Query Params**:
- `customerId`: Guid (opcional)
- `fleetId`: Guid (opcional)
- `page`: int (default 1)
- `pageSize`: int (default 10)

**Response (200 OK)**:
```json
{
  "items": [
    {
      "id": "vehicle-uuid",
      "licensePlate": "ABC1234",
      "brand": "Toyota",
      "model": "Corolla",
      "year": 2022,
      "customerId": "customer-uuid",
      "fleetId": null
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 10
}
```

**Ownership Filter**:
- Si JWT es Customer: solo sus vehículos (CustomerId == JWT.customerId)
- Si JWT es Fleet Contact: solo vehículos de su flota (FleetId == JWT.fleetId)
- Si JWT es Admin: todos los vehículos

---

### Órdenes de Trabajo

#### POST /api/work-orders
**Requiere**: Autenticado (Customer o Fleet Contact)

**Request**:
```json
{
  "vehicleId": "vehicle-uuid",
  "initialNote": "Revisar motor, hace ruido",
  "contactPersonName": "Diego López",      // Solo para flotas, opcional
  "contactPersonPhone": "+54 9 11 1234567" // Solo para flotas, opcional
}
```

**Response (201 Created)**:
```json
{
  "id": "work-order-uuid",
  "vehicleId": "vehicle-uuid",
  "currentStatus": 0,  // Received
  "customerIdAtEntry": "customer-uuid",
  "fleetIdAtEntry": null,
  "contactPersonName": null,
  "contactPersonPhone": null,
  "diagnosisNote": null,
  "estimatedCost": 0,
  "totalAmount": 0,
  "services": [],
  "photos": [],
  "timeline": [],
  "createdAt": "2026-05-03T10:30:00Z"
}
```

**Validaciones**:
- Vehículo existe
- Vehículo pertenece a usuario (ownership)
- Vehículo no tiene orden activa (Received → Delivered o Cancelled)
- contactPersonName: whitespace trimmed
- contactPersonPhone: whitespace trimmed

---

#### GET /api/work-orders/{id}
**Requiere**: Admin o (propietario de la orden: customer/fleet contact)

**Response (200 OK)**:
```json
{
  "id": "work-order-uuid",
  "vehicleId": "vehicle-uuid",
  "currentStatus": 2,  // AwaitingApproval
  "customerIdAtEntry": "customer-uuid",
  "fleetIdAtEntry": null,
  "contactPersonName": null,
  "contactPersonPhone": null,
  "diagnosisNote": "Revisar correa de distribución",
  "estimatedCost": 15000.00,
  "totalAmount": 15000.00,
  "services": [
    {
      "id": "service-uuid",
      "catalogServiceId": "catalog-uuid",
      "priceSnapshot": 15000.00,
      "quantity": 1,
      "subtotal": 15000.00
    }
  ],
  "photos": [],
  "timeline": [
    {
      "id": "change-uuid",
      "previousStatus": 0,
      "newStatus": 1,
      "changedAt": "2026-05-03T10:35:00Z",
      "note": "Comienza diagnóstico"
    },
    {
      "id": "change-uuid-2",
      "previousStatus": 1,
      "newStatus": 2,
      "changedAt": "2026-05-03T11:00:00Z",
      "note": "Presupuesto listo para aprobación"
    }
  ]
}
```

---

#### PUT /api/work-orders/{id}/status
**Requiere**: Admin (normalmente el taller)

**Request**:
```json
{
  "newStatus": 1,
  "note": "Comienza diagnóstico"
}
```

**Response (200 OK)**:
```json
{
  "id": "work-order-uuid",
  "currentStatus": 1,
  "... resto de campos ..."
}
```

**Lógica**:
1. Obtiene orden actual
2. Valida transición: WorkOrder.ChangeStatus(newStatus, userId, note)
   - Si transición inválida: 400 BadRequestException
3. Crea WorkOrderStatusChange
4. Si newStatus == AwaitingApproval:
   - Genera approval token (48h)
   - Fire-and-forget: TryEnqueueQuoteEmailAsync()
5. Guarda cambios
6. Retorna DTO actualizado

**Transiciones Permitidas** (ver sección "Estados y Máquina de Estados"):
- Received → Diagnosing, Cancelled
- Diagnosing → AwaitingApproval, Cancelled
- AwaitingApproval → Approved, Cancelled
- Approved → InProgress, Cancelled
- InProgress → Completed, Cancelled
- Completed → Delivered, Cancelled
- Delivered, Cancelled: no más transiciones

---

#### POST /api/work-orders/{id}/services
**Requiere**: Admin

**Request**:
```json
{
  "catalogServiceId": "service-uuid",
  "quantity": 2
}
```

**Response (201 Created)**:
```json
{
  "id": "work-order-service-uuid",
  "catalogServiceId": "service-uuid",
  "priceSnapshot": 8500.00,
  "quantity": 2,
  "subtotal": 17000.00
}
```

**Lógica**:
1. Obtiene CatalogService (para priceSnapshot)
2. Crea WorkOrderService
3. Actualiza WorkOrder.TotalAmount += subtotal
4. Retorna servicio creado

---

#### POST /api/work-orders/approve
**Público** (sin autenticación - importante para aprobación vía email)

**Request**:
```json
{
  "token": "abc123def456..."
}
```

**Response (200 OK)**:
```json
{
  "id": "work-order-uuid",
  "currentStatus": 3,  // Approved (o InProgress según decisión)
  "... resto de campos ..."
}
```

**Lógica**:
1. Obtiene WorkOrderApprovalToken con Token == payload.token
2. Valida:
   - Token existe: si no → 400 BadRequestException("Invalid token")
   - Token no expirado (ExpiresAt > UtcNow): si expirado → 400 BadRequestException("Token expired")
   - Token no usado (IsUsed == false): si usado → 400 BadRequestException("Token already used")
3. Marca token:
   - IsUsed = true
   - UsedAt = UtcNow
4. Obtiene WorkOrder
5. Cambia estado: AwaitingApproval → Approved (o InProgress)
6. Crea WorkOrderStatusChange:
   - ChangedBy: usuario del sistema o null (no hay auth)
   - Note: "Cliente aprobó presupuesto via email"
7. Guarda cambios
8. Retorna WorkOrderDetailDto actualizado

---

### Catálogo de Servicios

#### GET /api/catalog-services
**Requiere**: Autenticado

**Response (200 OK)**:
```json
[
  {
    "id": "service-uuid-1",
    "name": "Cambio de correa de distribución",
    "description": "Reemplazo de correa según especificaciones OEM",
    "price": 15000.00,
    "estimatedTime": 120
  },
  {
    "id": "service-uuid-2",
    "name": "Cambio de aceite y filtro",
    "description": "Cambio de aceite 5W-30 y nuevo filtro",
    "price": 3500.00,
    "estimatedTime": 30
  }
]
```

---

## Autenticación y Autorización

### JWT Token

**Estructura**:
```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user-id-uuid",
  "email": "juan@example.com",
  "role": "Customer",
  "customerId": "customer-uuid",
  "fleetId": null,
  "iat": 1714750800,
  "exp": 1714754400
}

Signature: HMAC-SHA256(secret_key)
```

**Claim Details**:
- `sub`: ApplicationUserId (del usuario que inició sesión)
- `email`: Email del usuario
- `role`: "Admin" o "Customer"
- `customerId`: Guid del Customer (siempre presente)
- `fleetId`: Guid de la flota (null si no es fleet contact)
- `iat`: Issued At (Unix timestamp)
- `exp`: Expiration (15-30 minutos desde iat)

**Secret Key**:
- Almacenado en variable de entorno: `JWT_SECRET_KEY`
- Mínimo: 64 caracteres
- Generado con: `openssl rand -base64 48`
- NUNCA hardcodeado

---

### Políticas de Autorización

```csharp
// Policy: Admin
[Authorize(Roles = "Admin")]

// Policy: Autenticado (cualquier rol)
[Authorize]

// Policy: Customer propietario
// Implementado en handler:
if (JWT.customerId != resource.customerId) throw 404;

// Policy: Fleet contact de la flota
// Implementado en handler:
if (JWT.fleetId != resource.fleetId) throw 404;
```

---

### Flujo de Login

1. **POST /api/auth/login** con email/password
2. Backend valida credenciales contra IdentityUser
3. Si válido:
   - Obtiene Customer del usuario (por ApplicationUserId)
   - Llena claims: customerId, fleetId, role
   - Genera JWT (15 min expiry)
   - Genera RefreshToken
   - Retorna ambos al frontend
4. Frontend almacena JWT en localStorage (access token)
5. Frontend incluye en headers: `Authorization: Bearer {jwt}`
6. Backend valida JWT con secret key en cada request

---

### Endpoints Públicos (Sin Autenticación)

- POST /api/auth/register
- POST /api/auth/login
- POST /api/work-orders/approve (aprobación vía token)
- GET /health (si está implementado)

---

## Email y Comunicaciones

### Configuración SMTP

**Archivo**: `appsettings.json` (o `appsettings.Production.json`)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SenderEmail": "noreply@mycarapp.com",
    "ApiKey": "SG.xxxxxxxxxxxx"
  }
}
```

**Variables de Entorno**:
- `SMTP_SERVER`
- `SMTP_PORT`
- `SENDER_EMAIL`
- `SMTP_API_KEY`

---

### Plantillas de Email

#### 1. Email de Presupuesto (AwaitingApproval)

**Cuándo se envía**: Cuando WorkOrder cambia a AwaitingApproval

**A quién**: 
- Cliente individual: customer.Email
- Fleet contact: contact.Email (obtenido de Fleet)

**Asunto**: `Presupuesto para su vehículo {Brand} {Model} — MyCarApp`

**Cuerpo**:
```html
<h2>Hola, {RecipientName}!</h2>
<p>El diagnóstico de tu <strong>{Brand} {Model}</strong> está listo.</p>
<p>Adjuntamos el presupuesto detallado. El total estimado es de <strong>$ {TotalAmount:N0}</strong>.</p>
<p>Para autorizar el trabajo, hacé clic en el siguiente botón:</p>
<p style="margin:24px 0;">
  <a href="{ApprovalLink}"
     style="background:#1d4ed8;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;margin-right:12px;">
    Aprobar presupuesto
  </a>
  <a href="https://wa.me/WHATSAPP_NUMBER_PLACEHOLDER"
     style="background:#25D366;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;display:inline-block;">
    Contactar por WhatsApp
  </a>
</p>
<p style="color:#6b7280;font-size:0.875rem;">
  Este enlace es válido por 48 horas. Si no solicitaste este presupuesto, ignorá este mensaje.
</p>
<br>
<p><em>MyCarApp — Taller de Servicios Automotores</em></p>
```

**Adjunto**: 
- Nombre: `Presupuesto-{LicensePlate}-{YYYYMMDD}.pdf`
- Contenido: PDF generado por IPdfService

**ApprovalLink**:
```
{ApprovalBaseUrl}?token={token}
Ejemplo: http://localhost:3000/approve?token=abc123def456...
```

---

### Servicio de Email (IEmailService)

**Contrato**:
```csharp
Task SendAsync(
    string to,
    string subject,
    string htmlBody,
    byte[]? attachment = null,
    string? attachmentName = null,
    CancellationToken cancellationToken = default
);
```

**Implementación esperada**:
- Usar SMTP con credenciales de config
- Enviar con From = SenderEmail
- Soportar HTML body
- Soportar adjuntos opcionalmente
- Loguear errores pero no fallar si está deshabilitado (graceful degradation)

---

## Validaciones

### Validaciones de Entidad

#### Customer
- FirstName: required, max 100
- LastName: required, max 100
- Email: required, unique, valid email format
- DocumentNumber: required, unique, formato DNI/RUT válido (regex)
- Phone: required, unique, formato teléfono válido (+54 9 11 12345678)
- FleetId: si presente, debe referencia existente + sin otro contacto

#### Fleet
- CompanyName: required, max 200
- Cuit: required, unique, 11 dígitos
- Email: optional, max 200
- ContactId: required, referencia a Customer, unique

#### Vehicle
- LicensePlate: required, unique
- Brand: required, max 100
- Model: required, max 100
- Year: required, 1900 ≤ Year ≤ currentYear+1
- VIN: optional, max 17
- CustomerId XOR FleetId: exactamente uno (constraint a nivel DB + lógica en handler)

#### WorkOrder
- VehicleId: required, referencia válida
- CurrentStatus: enum válido (0-7)
- DiagnosisNote: optional, max 5000
- EstimatedCost: >= 0
- TotalAmount: >= 0
- ContactPersonName: optional, max 100, whitespace trimmed
- ContactPersonPhone: optional, max 20, whitespace trimmed

#### WorkOrderService
- WorkOrderId: required, referencia válida
- CatalogServiceId: required, referencia válida
- Quantity: >= 1

---

### Validaciones de Negocio

#### Al Crear Customer
```
IF fleetId IS NOT NULL
    AND FleetContactExistsAsync(fleetId)
THEN
    throw BadRequestException(
        "La flota ya tiene un contacto encargado asignado. "
        "Una flota solo puede tener un encargado."
    )
```

#### Al Crear Vehicle
```
IF customerId IS NOT NULL
    LET customer = GetCustomer(customerId)
    IF customer.FleetId IS NOT NULL
    THEN
        throw BadRequestException(
            "Este cliente es contacto de una flota. "
            "No puede tener vehículos individuales. "
            "Cree una cuenta personal."
        )

IF fleetId IS NOT NULL
    AND (customerId IS NOT NULL)
THEN
    throw BadRequestException(
        "Un vehículo debe pertenecer a EXACTAMENTE "
        "uno: cliente individual O flota, no ambos."
    )

IF (fleetId IS NULL AND customerId IS NULL)
THEN
    throw BadRequestException(
        "Un vehículo debe pertenecer a un cliente "
        "individual o a una flota."
    )
```

#### Al Crear WorkOrder
```
LET vehicle = GetVehicle(vehicleId)

// Ownership
IF JWT.role == "Customer"
    AND vehicle.CustomerId != JWT.customerId
THEN
    throw 404  // No existe (leak prevention)

IF JWT.role == "FleetContact"
    AND vehicle.FleetId != JWT.fleetId
THEN
    throw 404  // No existe

// Ya existe orden activa
IF GetActiveWorkOrderCount(vehicleId) > 0
THEN
    throw BadRequestException(
        "Este vehículo ya tiene una orden de trabajo activa."
    )

// Whitespace trim
contactPersonName = contactPersonName?.Trim()
contactPersonPhone = contactPersonPhone?.Trim()
```

#### Al Cambiar Estado de WorkOrder
```
SWITCH currentStatus
    CASE Received:
        IF newStatus NOT IN (Diagnosing, Cancelled)
            throw InvalidOperationException(
                "No se puede cambiar a este estado desde Received"
            )
    
    CASE Diagnosing:
        IF newStatus NOT IN (AwaitingApproval, Cancelled)
            throw InvalidOperationException(...)
    
    CASE AwaitingApproval:
        IF newStatus NOT IN (Approved, Cancelled)
            throw InvalidOperationException(...)
    
    CASE Approved:
        IF newStatus NOT IN (InProgress, Cancelled)
            throw InvalidOperationException(...)
    
    CASE InProgress:
        IF newStatus NOT IN (Completed, Cancelled)
            throw InvalidOperationException(...)
    
    CASE Completed:
        IF newStatus NOT IN (Delivered, Cancelled)
            throw InvalidOperationException(...)
    
    CASE Delivered, Cancelled:
        throw InvalidOperationException(
            "Esta orden ya finalizó. No se puede cambiar de estado."
        )
```

#### Al Aprobar Presupuesto (Token)
```
LET token = GetApprovalToken(payload.token)

IF token IS NULL
THEN
    throw BadRequestException("Token inválido.")

IF token.ExpiresAt < UtcNow
THEN
    throw BadRequestException("Token expirado.")

IF token.IsUsed == true
THEN
    throw BadRequestException("Token ya utilizado.")

// Proceder a cambiar estado a Approved
```

---

## Seguridad

### OWASP Top 10

#### 1. SQL Injection
- ✅ EF Core parameterized queries
- ✅ No raw SQL sin parámetros

#### 2. Broken Authentication
- ✅ JWT con secret key seguro (min 64 chars, env var)
- ✅ Token expiry: 15-30 minutos
- ✅ Password hash: AspNet Core Identity (Bcrypt)
- ✅ Password complexity: min 8 chars, mayús, minús, número, símbolo

#### 3. Sensitive Data Exposure
- ✅ HTTPS requerido en producción (HSTS)
- ✅ No loguear tokens, emails, teléfonos
- ✅ No exponer error details en production
- ✅ Approval token de 64 chars (RandomNumberGenerator.GetBytes(32))

#### 4. XML External Entities (XXE)
- ✅ No usamos XML en esta API

#### 5. Broken Access Control
- ✅ Ownership check en cada GET/PUT/DELETE
- ✅ JWT claims validados
- ✅ 404 si ownership no match (leak prevention)
- ✅ Fleet contact no ve otras flotas
- ✅ Customer no ve clientes ajenos

#### 6. Security Misconfiguration
- ✅ CORS restringido (no AllowAnyOrigin en prod)
- ✅ Error messages no exponen stack traces
- ✅ HTTPS/HSTS habilitado
- ✅ Security headers (X-Content-Type-Options, X-Frame-Options, etc.)

#### 7. Cross-Site Scripting (XSS)
- ✅ Responde JSON (no HTML)
- ✅ Frontend maneja XSS de HTML

#### 8. Insecure Deserialization
- ✅ JSON.NET no ejecuta código arbitrario

#### 9. Using Components with Known Vulnerabilities
- ✅ Mantener nuget packages actualizados
- ✅ CI/CD scan (dotnet list package --vulnerable)

#### 10. Insufficient Logging & Monitoring
- ✅ Serilog con file + console sinks
- ✅ Log de errores con contexto
- ✅ Audit trail: WorkOrderStatusChange
- ✅ Enviar logs a centralized logging en producción

---

### Prácticas de Seguridad Implementadas

#### Ownership Enforcement
```csharp
// En handlers
if (workOrder.CustomerIdAtEntry.HasValue &&
    workOrder.CustomerIdAtEntry != currentUser.CustomerId)
    throw new NotFoundException();  // 404, no 403

if (workOrder.FleetIdAtEntry.HasValue &&
    workOrder.FleetIdAtEntry != currentUser.FleetId)
    throw new NotFoundException();  // 404
```

#### Soft Delete
```csharp
// GlobalQueryFilter en DbContext
modelBuilder.Entity<WorkOrder>()
    .HasQueryFilter(x => !x.IsDeleted);
```

#### Password Hashing
- Automático con AspNet Core Identity
- Bcrypt por defecto
- Verificación con IPasswordHasher<ApplicationUser>

#### Rate Limiting
- ⚠️ Pendiente implementación
- Sugerencia: `AspNetCoreRateLimit` package
- Limites: 100 requests/minuto por IP

---

## Logging

### Configuración Serilog

**Archivo**: `appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/mycarapp-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext" ]
  }
}
```

### Niveles de Log

- **Trace**: No usado en esta app
- **Debug**: Información de debug (dev only)
- **Information**: Eventos importantes
  - Login exitoso
  - WorkOrder creado
  - Presupuesto enviado
  - Orden aprobada
- **Warning**: Situaciones inusuales
  - Token expirado
  - Validación fallida
  - Recurso no encontrado
- **Error**: Errores de negocio y runtime
  - Email no enviado
  - DB connection failed
  - Excepción no manejada
- **Fatal**: Sistema no operativo
  - Startup fallido
  - DB totalmente inaccesible

### Convenciones de Logging

```csharp
// ✅ Bueno: mensaje estructurado, valores parametrizados
_logger.LogInformation(
    "WorkOrder {WorkOrderId} created for vehicle {VehicleId} by customer {CustomerId}",
    workOrder.Id, vehicle.Id, customerId
);

// ✅ Bueno: contexto adicional
using (LogContext.PushProperty("WorkOrderId", workOrderId))
{
    _logger.LogInformation("Changing status to {NewStatus}", newStatus);
}

// ❌ Malo: strings interpolados
_logger.LogInformation($"Order {id} created");

// ❌ Malo: datos sensibles
_logger.LogInformation("Email sent to {Email}", customer.Email);

// ✅ Mejor: sanitizar
_logger.LogInformation(
    "Email sent to {RecipientDomain}",
    customer.Email.Split('@')[1]
);
```

### Archivos de Log

**Ubicación**: `{ProjectRoot}/logs/`

**Naming**: `mycarapp-YYYYMMDD.log`

**Ejemplos**:
- `mycarapp-20260503.log` (3 de mayo)
- `mycarapp-20260504.log` (4 de mayo)

**Retención**: 30 días

---

## Configuración

### appsettings.json (Development)

```json
{
  "Serilog": { ... },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mycarapp_dev;User Id=postgres;Password=postgres;"
  },
  "JwtSettings": {
    "Secret": "dev-secret-min-64-chars-for-testing-only-xxxxxxxxxxxxxxxx",
    "ExpiryMinutes": 15
  },
  "AppSettings": {
    "ApprovalBaseUrl": "http://localhost:3000/approve"
  },
  "EmailSettings": {
    "SmtpServer": "localhost",
    "SmtpPort": 1025,
    "SenderEmail": "noreply@mycarapp.local"
  }
}
```

### appsettings.Production.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Error",
        "Microsoft.AspNetCore": "Error",
        "Microsoft.EntityFrameworkCore": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/mycarapp/mycarapp-.log",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "https://seq.example.com"
        }
      }
    ]
  },
  "AllowedHosts": "api.mycarapp.com",
  "ConnectionStrings": {
    "DefaultConnection": "Server=db.prod;Database=mycarapp;User Id=appuser;Password=SecurePassword123;"
  },
  "JwtSettings": {
    "Secret": "{{ VAULT_JWT_SECRET_KEY }}",
    "ExpiryMinutes": 30
  },
  "AppSettings": {
    "ApprovalBaseUrl": "https://mycarapp.com/approve"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SenderEmail": "noreply@mycarapp.com",
    "ApiKey": "{{ VAULT_SENDGRID_KEY }}"
  }
}
```

### Variables de Entorno (Production)

```bash
# Database
DB_HOST=db.prod
DB_PORT=5432
DB_NAME=mycarapp
DB_USER=appuser
DB_PASSWORD=SecurePassword123

# JWT
JWT_SECRET_KEY=<generated-key-64-chars>

# Email
SMTP_SERVER=smtp.sendgrid.net
SMTP_PORT=587
SMTP_SENDER_EMAIL=noreply@mycarapp.com
SMTP_API_KEY=SG.xxxxxxxxxxxxx

# URLs
APPROVAL_BASE_URL=https://mycarapp.com/approve
CORS_ORIGIN=https://mycarapp.com

# Aspnet
ASPNETCORE_ENVIRONMENT=Production
```

---

## Notas y Decisiones Pendientes

### 1. ⚠️ Estado Intermedio (PENDIENTE)
Decidir si después de aprobación del cliente:
- **Opción A**: Status = Approved (esperar entrega del auto)
- **Opción B**: Status = InProgress (comenzar trabajo de inmediato)

**Impacto**: Cambios en Enum (agregar Approved = 3), máquina de estados, transiciones.

### 2. ⚠️ Endpoints Faltantes (TBD)
- PATCH /api/work-orders/{id}/diagnosis (actualizar nota/costo estimado)
- POST /api/work-orders/{id}/photos (agregar fotos)
- DELETE /api/work-orders/{id}/services/{serviceId}
- PUT /api/work-orders/{id}/services/{serviceId}

### 3. ⚠️ Email de Confirmación (TBD)
Después de aprobación, ¿enviar email confirmando que se inició trabajo?

### 4. ⚠️ Facturas (TBD)
¿Generar factura al completar orden? ¿Al entregar?

### 5. ⚠️ Rate Limiting (NO IMPLEMENTADO)
Agregar límites de requests:
- 100 requests/minuto por IP
- 10 registros/hora (anti-spam)
- 5 intentos fallidos de login = lockout 15 min

### 6. ⚠️ Notificaciones (TBD)
¿Push notifications, SMS, WhatsApp cuando cambia estado?

### 7. ⚠️ Dashboard Estadísticas (TBD)
- Total órdenes completadas
- Ingresos mensuales
- Tiempo promedio resolución
- Clientes frecuentes

### 8. ✅ Soft Delete (IMPLEMENTADO)
Verificar que global query filters están aplicados en todas las entidades.


## Checklist Pre-Producción

- [ ] Migrations aplicadas (AddContactPersonToWorkOrder)
- [ ] Tests de integración para flujos críticos
- [ ] Email: SMTP real configurado
- [ ] JWT: Secret key seguro en env var
- [ ] CORS: Frontend URL configurada
- [ ] HTTPS: HSTS habilitado
- [ ] Logging: Verificar que logs escriben a disco
- [ ] Database: Backups configurados
- [ ] Database: Índices en queries frecuentes
- [ ] Rate limiting: Implementado
- [ ] Security headers: Agregados
- [ ] Error messages: No exponen internals
- [ ] Soft delete: Verificado en todas las entidades
- [ ] N+1 queries: Auditadas y optimizadas
- [ ] Load test: 100+ WOs simultáneos
- [ ] Health check endpoint: Implementado
- [ ] Monitoring: Alertas para errors

---

**Fin de Especificación v1.0**
