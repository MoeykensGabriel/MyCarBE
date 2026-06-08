# MyCarApp - Especificación Completa

**Versión**: 1.1  
**Última actualización**: 2026-05-06  
**Estado**: Pre-Producción  

**Changelog**:
- **v1.3** (2026-06-08): Issue abierto "Duraciones de servicios y organización del taller" (ver sección al final): los dos campos de duración (`EstimatedDurationMinutesSnapshot` del catálogo vs `EstimatedDurationMinutes` del mecánico) no se hablan. Preguntas de producto pendientes. Specs de batería (capacidad, caja, borne) agregados a `VehicleBattery`.
- **v1.1** (2026-05-06): Alineación con código real (sin estado `Approved`, snapshots en `WorkOrderService`). Nueva entidad `Mechanic` y flujo de asignación/aceptación/finalización de servicios por mecánicos. Notas obligatorias al finalizar.
- **v1.2** (2026-05-23): Aclaraciones de cliente/mecánico:
  - Stock NO maneja precios — es intermediario de disponibilidad. El precio se carga en la cotización (info externa del taller).
  - Calendario de turnos: una patente ocupa el rectángulo del área durante TODOS los días que dura el trabajo (ej. motor 30 días = misma patente en la celda de motor durante 30 días).
  - Servicios NO son catálogo fijo de precio — la mano de obra la define el mecánico por trabajo (no es lo mismo tren delantero de Peugeot que de Mercedes). El mecánico manda costo de mano de obra junto con su inspección.

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


Una vez que se hace el informe de la recepcion (orden) y al ser la primera vez no hay un diagnostico general, entonces cada mecanico del area que le corresponde a cada uno, fijarse si el auto tiene algo que le corresponde a su area, si tiene algo el mecanico hace un detalle de lo que encuentra en su area. Ese detalle debe ser muy especidifico en palabras por el propioo mecanico para que en otra instancia eso sirva para hacer una cotizacion sin que el cliente tenga que volver a traer el vehiculo innecesariamente. Todo ese informe inicial del mecanico, por ej llega un auto con problemas en el tren delantero, buueno, todos hacen un informe incluido el de tren delantero pero la oficina es que el que filtra de todos los informes de todas las areas, y se fijara en el motivo por el cual vino el auto al taller (eso ya se incluye). Lo filtra y lo desarrolla, busca el codigo de los repuestos, precios, precios del servicio de mano de obra, y se le manda al cliente

Eso logra que ya se genere un informe inicial del auto, y la proxima vez que venga el taller ya le da un TURNO porque ya saben lo que tiene el auto, que auto es y que tenia. Le pueden dar un turno cuando ya tenga los repuestos, capacidad o lo que sea.

=== INTEGRACION CON STOCK DE REPUESTOS ===

La responsabilidad principal de este sistema es capturar la necesidad de repuestos, enviarla a validar y hacer el seguimiento del estado.

1. Datos necesarios en el flujo
Cada presupuesto debe permitir que se cargue, por cada ítem, el código del proveedor (o código de repuesto) y la cantidad requerida.

El presupuesto debe estar asociado a una identificación única del vehículo (la patente).

2. Eventos y Lógica del Sistema
Al aprobarse un presupuesto: 1. El sistema debe tomar de forma automática todos los códigos de repuestos cargados en ese presupuesto junto con sus cantidades y la patente del auto.
2. Debe enviar estos datos al Sistema de Stock para consultar la disponibilidad.
3. Al recibir la respuesta del Sistema de Stock (que le dirá qué hay y qué falta), el Sistema de Taller debe registrar internamente este pedido asociado a la patente, guardando el estado inicial que le devolvió el depósito.

Actualización de estados:

El sistema debe quedar escuchando o permitir consultar los cambios de estado que haga el depósito (si los repuestos ya se compraron, si están en camino o si ya se le entregaron al mecánico).

3. Cambios en la Experiencia de Usuario (Oficina)
Botonera: Agregar la acción de "Aprobar Presupuesto" que dispara todo el proceso anterior.

---

## ESTADO ACTUAL DE IMPLEMENTACIÓN — Integración con Stock (GestionPGB)

> Sección de trabajo en pausa. A revisar con el cliente antes de continuar.

---

### Objetivo

Cuando se aprueba un presupuesto en el taller, los repuestos que el mecánico necesita deben
pedirse automáticamente al depósito (GestionPGB). El taller tiene que poder ver en tiempo real
si los repuestos están disponibles, en camino, o si hay faltantes — sin tener que llamar por
teléfono al depósito.

---

### Lo que tenemos hoy (implementado y funcionando)

#### Backend — MyCar

**Dominio**
- Entidad `PartsStockRequest`: representa un pedido al depósito, asociado a una WO.
  Campos: `WorkOrderId`, `LicensePlateSnapshot`, `ExternalReference` (ID del pedido en GestionPGB),
  `Status`, `Items`.
- Entidad `PartsStockRequestItem`: un ítem del pedido. Campos: `ProductCode`, `Name`,
  `Quantity`, `Status`, `Notes`.
- Enum `StockRequestStatus`: `PendingReview`, `HasShortages`, `InProgress`, `Ready`.
- Enum `StockRequestItemStatus`: `PendingReview`, `Available`, `Missing`, `InTransit`, `Delivered`.
- Lógica `RecomputeStatus()`: calcula el estado agregado del pedido a partir del estado de sus ítems.

**Persistencia**
- Migración `AddPartsStockRequests` aplicada. Tablas `PartsStockRequests` y
  `PartsStockRequestItems` creadas en PostgreSQL.
- Repositorio `IPartsStockRequestRepository` con métodos de lectura filtrada, detalle y
  búsqueda de ítems individuales.

**Integración con GestionPGB**
- Interfaz `IStockService` con método `SubmitRequestAsync`.
- `HttpStockService`: implementación real que llama a `POST /api/workshop-orders` en GestionPGB
  con autenticación `X-Api-Key`. Mapea las respuestas de GestionPGB (enums como strings) al
  modelo interno de MyCar.
- `StubStockService`: fallback que solo loguea — activo si `StockSystem:BaseUrl` no está
  configurado.
- Orquestador `StockRequestOrchestrator`: se dispara al aprobar un presupuesto. Es idempotente
  (no crea pedido duplicado si ya existe). Si GestionPGB no responde, swallow del error —
  la aprobación de la WO no se interrumpe. El pedido queda en `PendingReview` para reintento.

**Endpoints API**
- `GET /api/stock-requests` — listado con filtros por estado y patente (Admin/Receptionist).
- `GET /api/stock-requests/{id}` — detalle con ítems.
- `POST /api/stock-requests/items/{itemId}/status` — override manual de estado de un ítem.
- `POST /api/stock-requests/{id}/retry-submission` — reintenta el envío a GestionPGB si el
  pedido quedó sin `ExternalReference` por fallo en la llamada original.
- `POST /api/stock-requests/callback` — endpoint que invoca GestionPGB cuando confirma entrega.
  Valida `X-Api-Key` (máquina a máquina). Actualiza ítems y recomputa estado del pedido.

**Disparo automático al aprobar**
- Los tres handlers de aprobación (`ApproveAsCustomer`, `ApproveWorkOrder`,
  `ChangeWorkOrderStatus`) llaman al orquestador. Sólo procesa partes con
  `ApprovalStatus == Approved` y `ProductCode != null`.

**Configuración** (`appsettings.Development.json`)
```json
"StockSystem": {
  "BaseUrl": "http://localhost:5000",
  "ApiKey": "dev-workshop-key-change-me-in-production-min-32-chars",
  "CallbackApiKey": "dev-callback-key-change-me-in-production-min-32-chars",
  "CallbackBaseUrl": "http://localhost:5216"
}
```

#### Frontend — MyCar

- Pantalla `/admin/stock` con:
  - Chips de estado filtrable (Pendiente / Con faltantes / En camino / Listo) con conteos.
  - Búsqueda por patente.
  - Indicador de última actualización automática (cada 30 segundos).
  - Lista de pedidos con borde de color por estado, resumen de ítems con íconos.
  - Panel expandido por pedido: ítems con ícono + badge de estado + fecha.
  - Override manual de estado por ítem (dropdown).
  - Botón "Reintentar envío a GestionPGB" cuando el pedido no llegó al depósito.
- Enlace en el menú lateral de admin.
- Hook `useStockRequests` con refetch automático cada 30 segundos.

#### GestionPGB (sistema de stock externo — código en `ProyectoGestionInventario`)

- Corre en `http://localhost:5000`.
- Recibe pedidos en `POST /api/workshop-orders` con `X-Api-Key`.
- Responde con disponibilidad inmediata por ítem (Available, Shortage, NotFound, etc.).
- Guarda un `callbackUrl` para notificar al taller cuando confirma la entrega final.
- Usa enums como strings en JSON (`JsonStringEnumConverter`).

---

### Lo que queremos lograr (visión completa)

0. **Buscador de productos del depósito en el presupuesto** *(prerequisito de todo lo demás)*:
   Cuando la recepcionista agrega un repuesto al presupuesto, tiene que poder buscar por nombre
   o código en el catálogo de GestionPGB y seleccionarlo. El código, nombre y precio del
   depósito se cargan solos — sin tener que tipear nada de memoria.

1. **Flujo automático de pedido**: Al aprobar un presupuesto, el pedido llega a GestionPGB sin
   intervención manual. La oficina no tiene que llamar al depósito.

2. **Seguimiento en tiempo real**: La pantalla `/admin/stock` muestra el estado actualizado de
   cada repuesto. El mecánico sabe si puede arrancar a trabajar o si tiene que esperar.

3. **Notificación de entrega automática**: Cuando el depósito entrega los repuestos al taller,
   GestionPGB llama al callback de MyCar y los ítems pasan automáticamente a "Entregado al
   mecánico". La oficina no tiene que actualizar nada manualmente.

4. **Override manual como respaldo**: Si el depósito avisa por teléfono o WhatsApp antes de que
   el sistema se actualice, la oficina puede cambiar el estado manualmente desde cada ítem.

5. **Resiliencia**: Si GestionPGB está caído en el momento de la aprobación, el pedido queda
   registrado en MyCar y se puede reenviar cuando el depósito vuelva a estar disponible.

---

### Lo que falta / pendiente

#### 🚨 Bloqueante crítico — Buscador de productos del catálogo

**El problema:** Hoy, para agregar un repuesto a un presupuesto, la recepcionista tiene que
tipear manualmente el `ProductCode` (ej. `RRR-123`). En la práctica esto es imposible: nadie
sabe de memoria los códigos de cientos de productos. Sin este buscador, la integración con
GestionPGB no es usable en producción.

**Lo que se necesita:**

- [ ] **GestionPGB debe exponer un endpoint de búsqueda de productos**, por ejemplo:
      `GET /api/products?q=filtro` que devuelva nombre, código (barcode), precio y stock
      disponible. Sin este endpoint, MyCar no puede ofrecer el buscador.
      *→ Requiere coordinación con el equipo/cliente de GestionPGB.*

- [ ] **MyCar BE — endpoint proxy de productos**: `GET /api/catalog/stock-products?q=...`
      que llame a GestionPGB y devuelva los resultados al frontend. Esto evita exponer
      las credenciales de GestionPGB al navegador y permite cachear resultados.
      ⚠️ **El response del proxy NO debe incluir precio** — solo `code`, `name` y
      `stock` (disponibilidad). El precio del repuesto siempre lo escribe la oficina
      al armar la cotización (info externa de proveedores). Decisión confirmada con
      cliente 2026-05-23.

- [ ] **MyCar FE — selector de producto en el formulario de presupuesto**: Cuando la
      recepcionista agrega un repuesto al presupuesto, en lugar de un campo de texto libre
      para el código, debe haber un buscador tipo autocomplete que consulte el catálogo de
      GestionPGB. Al seleccionar un producto, se autocompletan: nombre, código, y precio
      sugerido del depósito.

**Flujo esperado:**
```
Recepcionista tipea "filtro de aceite" en el buscador de partes
    → MyCar FE llama a GET /api/catalog/stock-products?q=filtro+de+aceite
    → MyCar BE llama a GestionPGB GET /api/products?q=filtro+de+aceite
    → GestionPGB devuelve lista de coincidencias con código y precio
    → Recepcionista selecciona el producto correcto
    → ProductCode, nombre y precio quedan cargados en el ítem del presupuesto
```

**Nota sobre repuestos sin código:** Los repuestos que el taller consigue por su cuenta
(sin código de GestionPGB) deben seguir siendo posibles — el campo de código queda vacío
y ese ítem no se envía al depósito al aprobar el presupuesto.

---

#### Técnico

- [ ] **Prueba de integración end-to-end**: Aprobar un presupuesto con MyCar y GestionPGB
      corriendo simultáneamente, verificar que el pedido llega y los estados se actualizan.
      *Bloqueado: conflicto de puertos al correr ambos localmente. Solución: deployar
      GestionPGB en Railway, o configurar puertos distintos.*

- [ ] **Callback de entrega**: Verificar que cuando GestionPGB confirma la entrega, el callback
      llega a MyCar y los ítems pasan a "Entregado". Requiere que MyCar BE sea accesible
      públicamente (Railway) o que se use ngrok para testing local.

- [ ] **Callbacks de estados intermedios**: GestionPGB actualmente solo llama al callback en la
      entrega final. Los estados intermedios (Shortage → PurchasedInTransit) no se notifican
      automáticamente — hay que pedirle al equipo de GestionPGB que agregue callbacks en esas
      transiciones, o implementar polling periódico desde MyCar.

- [ ] **Deployment en producción**: Definir si GestionPGB se deploya en Railway (recomendado)
      y apuntar `StockSystem:BaseUrl` a la URL pública. El `CallbackBaseUrl` también debe ser
      la URL pública de MyCar BE.

- [ ] **Variables de entorno en producción**: Las API keys deben ser secretos reales, no los
      valores de desarrollo actuales.

#### Negocio / a hablar con el cliente

- [ ] **¿Quién opera GestionPGB?** ¿Es un sistema interno del taller o lo opera un depósito
      externo? Esto define quién tiene acceso y cómo se gestiona.

- [ ] **Flujo de faltantes**: Cuando hay repuestos que faltan, ¿qué hace la oficina? ¿Espera
      que el depósito los consiga? ¿Le avisa al cliente? ¿Puede aceptar una entrega parcial?

- [ ] **Visibilidad del mecánico**: ¿El mecánico necesita ver en su panel si sus repuestos ya
      llegaron, o alcanza con que la oficina lo gestione?

- [ ] **Relación con la WO**: Cuando todos los repuestos están en estado "Entregado", ¿la WO
      avanza de estado automáticamente (ej. pasa a InProgress)? ¿O la oficina lo hace manual?

- [ ] **¿Qué pasa con repuestos custom?** Los repuestos sin `ProductCode` (que el taller
      consigue por su cuenta) no se envían a GestionPGB. ¿Está bien? ¿Necesitan algún
      seguimiento propio?

- [ ] **Historial y auditoría**: ¿El cliente necesita ver un historial de pedidos pasados,
      incluyendo los ya entregados? ¿O solo los activos?

---

### Notas técnicas para retomar

- `ProductCode` en `WorkOrderPart` debe coincidir con el `Barcode` del producto en GestionPGB.
  Producto de prueba disponible en GestionPGB: `RRR-123`.
- El orquestador es idempotente: si ya existe un `PartsStockRequest` para una WO, no crea otro.
  Para reenviar un pedido huérfano (sin `ExternalReference`), usar el endpoint
  `POST /api/stock-requests/{id}/retry-submission` o el botón en `/admin/stock`.
- Si se necesita resetear una prueba: borrar las filas de `PartsStockRequestItems` y
  `PartsStockRequests` para el `WorkOrderId` en cuestión, luego usar "Reintentar envío".

Pantalla de Seguimiento: Una nueva sección para las chicas de la oficina donde puedan ver un listado por patente. Ahí verán si el pedido de repuestos está: Pendiente de revisión en depósito, Con faltantes para comprar, Comprado/En viaje o Listo/Entregado. Esto les permite coordinar el turno con el cliente.

---

## ESTADO ACTUAL DE IMPLEMENTACIÓN — Turnos y Disponibilidad del Taller

> Sección de trabajo en pausa. A definir con el cliente antes de diseñar.

---

### Objetivo

Que la oficina pueda saber en todo momento cuándo tiene lugar para recibir un auto, y que
el cliente sepa exactamente cuándo traerlo. Hoy esa coordinación se hace por teléfono y
se pierde información — el sistema tiene que ser la única fuente de verdad para saber
qué días están ocupados, cuándo están listos los repuestos y cuándo le conviene al taller
recibir ese auto.

---

### Lo que hay hoy

Nada implementado. Solo dos menciones en la especificación original:

- *"La próxima vez que venga al taller ya le da un TURNO porque ya saben lo que tiene el
  auto... Le pueden dar un turno cuando ya tenga los repuestos, capacidad o lo que sea."*
- *"Esto les permite coordinar el turno con el cliente."*

El concepto está mencionado pero no diseñado.

---

### Lo que queremos lograr

1. **La oficina ve un calendario de disponibilidad**: qué días tienen lugar libre, cuántos
   autos pueden recibir por día, y qué días ya están completos.

2. **La oficina crea un turno para un cliente**: selecciona el día, el cliente y el auto,
   y el sistema registra que ese lugar está tomado.

3. **El cliente recibe una confirmación**: por email o desde su portal, sabe el día y
   hora acordados para traer el auto.

4. **El turno está conectado al historial del auto**: si el auto ya estuvo en el taller,
   la oficina ve qué trabajo se le hizo antes, qué se diagnosticó y si hay repuestos ya
   pedidos — todo antes de que el auto llegue.

5. **El turno considera si los repuestos están listos**: el sistema avisa si el auto
   tiene repuestos pedidos al depósito que todavía no llegaron, para no darle turno
   antes de tiempo.

---

### Propuesta de visualización — calendario por área de mecánico

La capacidad del taller no es un número único: es **capacidad por área**. Si todos los
mecánicos están ocupados con motor, el taller puede hacer 6 motores pero 0 frenos. La
vista debe reflejar eso.

**Boceto del calendario (vista por día):**

```
┌─────┬────────────────────────┬──────────────────────────────────┬──────────────┐
│     │ Servicio + Mecánico    │ Vehículos asignados              │ Capacidad    │
├─────┼────────────────────────┼──────────────────────────────────┼──────────────┤
│     │ Tren delantero (Juan)  │ GHK123 │ MPC101 │ KOQ27B         │ 3 vehículos  │
│ Día │ Frenos (Carlos)        │ ADO38KF                          │ 1 vehículo   │
│  1  │ Motor (Luis)           │ AH301PL │ AEF01MR                │ 2 vehículos  │
│     │ ...                    │ ...                              │              │
├─────┼────────────────────────┼──────────────────────────────────┼──────────────┤
│ Día │                                                                          │
│  2  │                                                                          │
└─────┴──────────────────────────────────────────────────────────────────────────┘
```

**Qué resuelve esta vista:**

- **Refleja la realidad del taller**: no se ven slots abstractos, se ve qué auto está
  en qué área con qué mecánico.
- **Detecta cuellos de botella al vistazo**: si frenos tiene 5 autos y motor tiene 0,
  se ve sin pensar.
- **Las patentes visibles** permiten reconocer al instante de qué auto se habla.
- **Escala sin romper el layout**: agregar áreas nuevas (electricidad, suspensión)
  es agregar filas, no rediseñar.
- **Colores por área** ayudan a escanear: cada servicio con su color, la oficina
  aprende a leer la pantalla sin esfuerzo.

**Preguntas adicionales que esta vista abre:**

- [ ] **¿Qué pasa con un auto que necesita varios servicios?** Por ejemplo GHK123
      entra para tren delantero **y** frenos. ¿Aparece en las dos filas, o se asigna
      a la "principal" y el resto se trabaja en otro día?
- [ ] **¿Qué pasa con autos que tardan más de un día?** Si el motor lleva 3 días,
      ¿el auto aparece en motor del día 1, día 2 y día 3? ¿O solo el día que entra?
- [ ] **¿La capacidad por área varía por día?** Si el mecánico de frenos se toma
      vacaciones, ¿ese día la capacidad de frenos es 0?
- [ ] **¿Cómo se diferencia un turno confirmado de uno tentativo?** ¿Color más claro,
      ícono, opacidad?
- [ ] **¿Se ven los espacios libres?** Si el día tiene capacidad para 4 trenes
      delanteros y hay 3 patentes, ¿hay un slot vacío clickeable para asignar al
      próximo cliente?

---

### Preguntas abiertas — a definir con el cliente

Estas preguntas definen completamente cómo se construye el sistema. Sin respuesta,
no se puede diseñar nada.

- [ ] **¿Cuántos autos puede atender el taller al mismo tiempo?**
      ¿Hay un límite físico (fosos, bahías, mecánicos) que define cuántos autos pueden
      estar adentro trabajándose a la vez? ¿Ese número es fijo o cambia?

- [ ] **¿El turno lo da siempre la oficina, o el cliente puede pedirlo solo?**
      ¿El cliente llama por teléfono y la chica de la oficina lo carga en el sistema?
      ¿O el cliente puede entrar al portal y pedir un turno él mismo?

- [ ] **¿El turno tiene hora, o solo día?**
      ¿"Traelo el martes a las 10" o simplemente "traelo el martes"?

- [ ] **¿Qué pasa cuando un cliente llama por primera vez?**
      ¿Se le da un turno antes de que llegue, o primero tiene que aparecer para que
      la oficina registre el ingreso del auto?

- [ ] **¿Qué pasa cuando el cliente ya tiene el presupuesto aprobado y hay que
      coordinar cuándo traer el auto para hacer el trabajo?**
      ¿Eso es un turno nuevo, o es una confirmación dentro del mismo ingreso que ya existe?

- [ ] **¿El taller tiene días y horarios fijos de atención?**
      ¿Lunes a viernes de 8 a 18? ¿Sábados? ¿Hay feriados u otros días en que
      el taller no atiende aunque quiera?

- [ ] **¿Qué pasa si el cliente no aparece en el día del turno?**
      ¿Se cancela solo? ¿La oficina lo reagenda? ¿Se le manda un recordatorio antes?

---

### Lo que falta (una vez resueltas las preguntas)

- [ ] Diseño del modelo de datos: Turno, configuración de capacidad del taller,
      horarios de atención.
- [ ] Pantalla de calendario para la oficina: vista semanal/mensual con disponibilidad.
- [ ] Creación de turno desde la oficina: buscar cliente, seleccionar auto y fecha.
- [ ] Notificación al cliente cuando se confirma el turno.
- [ ] Recordatorio automático al cliente un día antes.
- [ ] Vista del cliente: "mis turnos" en el portal.
- [ ] Bloqueo automático de días cuando se alcanza la capacidad máxima.
- [ ] Conexión con el estado de los repuestos: el sistema avisa si los repuestos
      del auto todavía no llegaron al darle turno.
- [ ] Configuración por área: cantidad de mecánicos / capacidad por servicio,
      días de vacaciones o ausencias.
- [ ] Soporte para autos que ocupan varios días: el turno bloquea el slot del
      mecánico durante toda la duración estimada del trabajo.

---

## 📋 RESUMEN CONSOLIDADO DE PENDIENTES

> Lista única de todo lo que está en pausa o por mejorar. Ordenado por urgencia.

---

### 🔴 Urgente (bugs visibles para el usuario hoy)

| # | Tema | Qué hay que hacer | Archivo / Pantalla |
|---|------|-------------------|--------------------|
| 1 | **Filtro "Inactivos" de Mecánicos no funciona** | El FE manda `isActive`, el BE espera `includeInactive`. Unificar el contrato. | `mechanics/page.tsx` ↔ `MechanicsController.cs` |
| 2 | **Filtro "Particular/Flota" de Clientes rompe la paginación** | Mover el filtro al BE. Hoy se aplica client-side sobre la página actual. | `customers/page.tsx` ↔ `CustomersController.cs` |
| 3 | **Búsqueda de servicios no funciona** | El FE manda `search`, el BE lo descarta. Agregar búsqueda real por nombre. | `services/page.tsx` ↔ `CatalogServicesController.cs` |

---

### 🟡 Importante antes de producción

| # | Tema | Por qué | Acción |
|---|------|---------|--------|
| 4 | **CORS hardcodeado a localhost:3000** | El frontend de producción no va a funcionar. | Leer origen desde `appsettings.Production.json`. |
| 5 | **Contraseña del admin impresa en el log** | Queda en texto plano en disco hasta 30 días. | Quitar el log o mover la contraseña a configuración. |
| 6 | **Soft-delete manual en los handlers** | Inconsistencia con el resto del proyecto que usa el mecanismo centralizado. | Mover el soft-delete al dominio o al repositorio. |
| 7 | **Paginación faltante en Fleets** | El FE manda paginación pero el BE devuelve toda la tabla. | Implementar paginación real en el repositorio. |
| 8 | **Posible N+1 en Vehicles** | Si Mapster accede por navegación, es una query por fila. | Verificar el profile de Mapster o agregar `Include(Customer/Fleet)`. |

---

### 🟢 Mejoras de arquitectura / limpieza

| # | Tema | Por qué |
|---|------|---------|
| 9 | **Dead code: `MaintenanceAlert` y `DeclaredServiceHistory`** | Tienen dominio, configuración y migración pero no hay ningún feature construido. Decidir: construir o eliminar. |
| 10 | **`Mechanic.Specialty` marcado como DEPRECATED** | La columna sigue viva. Planificar drop en próxima migración. |
| 11 | **Over-fetching en handlers de WorkOrder** | Casi todos cargan el grafo completo cuando solo necesitan una parte. |
| 12 | **Validators de FluentValidation en capa Data** | Dead code funcional — nunca se invocan. Eliminarlos o mover los útiles a Application. |

---

### 🔵 Features en pausa — a definir con el cliente

#### A. Integración con Stock (GestionPGB)

**Estado:** Implementación base lista, no usable en producción sin el catálogo de productos.

**Bloqueante crítico:**
- [ ] **Buscador de productos del catálogo de GestionPGB**: hoy la recepcionista
      tendría que tipear el código de producto de memoria. Requiere que GestionPGB
      exponga un endpoint `GET /api/products?q=...` y que MyCar haga un proxy.

**Técnico:**
- [ ] Prueba end-to-end con ambos sistemas corriendo (bloqueado por puertos locales).
- [ ] Callback de entrega validado (requiere URL pública de MyCar BE).
- [ ] Callbacks de estados intermedios (GestionPGB solo notifica delivery final).
- [ ] Deployment en producción (definir si GestionPGB va a Railway).
- [ ] Paginación en `/api/stock-requests` + bajar polling de 30s.

**Negocio:**
- [ ] ¿Quién opera GestionPGB?
- [ ] Flujo de faltantes: ¿qué hace la oficina, qué se le dice al cliente?
- [ ] Visibilidad del mecánico: ¿el mecánico ve si sus repuestos llegaron?
- [ ] ¿La WO avanza automáticamente cuando todos los repuestos están entregados?
- [ ] Repuestos custom (sin código): ¿necesitan algún seguimiento?
- [ ] Historial de pedidos pasados: ¿se muestran o solo los activos?

#### B. Turnos y Disponibilidad del Taller

**Estado:** No diseñado. Boceto de UI conceptual aprobado (calendario por área).

**Preguntas de negocio (bloquean el diseño):**
- [ ] ¿Capacidad por taller, por área o por mecánico individual?
- [ ] ¿Quién crea el turno: la oficina, el cliente, o ambos?
- [ ] ¿Turno con hora o solo día?
- [ ] ¿Cliente nuevo recibe turno antes de aparecer, o primero llega?
- [ ] ¿Coordinación post-aprobación es un turno nuevo o sigue el mismo ingreso?
- [ ] ¿Horarios y días de atención del taller?
- [ ] ¿Qué pasa si el cliente no aparece?
- [ ] ¿Auto que necesita varios servicios: una fila o varias?
- [ ] ¿Auto que tarda varios días: ocupa el slot en cada día?
- [ ] ¿Capacidad por área varía día a día (vacaciones, ausencias)?
- [ ] ¿Cómo se diferencia turno confirmado vs tentativo?
- [ ] ¿Se ven los espacios libres en el calendario?

**Técnico (cuando se resuelvan las preguntas):**
- [ ] Modelo de datos: Turno, configuración de capacidad, horarios.
- [ ] Pantalla de calendario por día/área para la oficina.
- [ ] Creación de turno + notificación al cliente.
- [ ] Recordatorio automático un día antes.
- [ ] Vista "mis turnos" en el portal del cliente.
- [ ] Conexión con stock: bloquear turno si los repuestos no llegaron.

#### C. Estación de Viajes (feature premium, apagada por defecto)

**Estado:** Construida y funcional (QR de estación + registro de viajes por chofer +
historial). Hoy se ofrece a TODA flota (se muestra cuando el vehículo tiene `FleetId`).

**Decisión de producto:** NO debe formar parte del sistema base. Es una función
premium que se habilita **a dedo, por flota**. La feature queda escrita y registrada,
pero apagada por defecto; se prende solo a las flotas que la contraten.

**Mecanismo elegido — entitlement por flota (flag en la entidad `Fleet`):**
Se prefiere un flag persistente sobre la entidad antes que un feature flag global por
config (que sería todo-o-nada) o una tabla genérica de feature flags (andamiaje
excesivo para una sola función). Si en el futuro aparecen varias funciones premium,
recién ahí conviene migrar a una tabla `FleetFeatureFlags` genérica.

**Plan de implementación (cuando se habilite):**
- [ ] **Dominio:** agregar `bool TripStationEnabled` a `Fleet` (default `false`).
- [ ] **Migración:** AddColumn `TripStationEnabled` (bool, default false) en `Fleets`.
- [ ] **DTOs:** exponer el flag en el DTO de flota y propagar `tripStationEnabled` al
      DTO de vehículo (lo consume el portal del cliente/flota).
- [ ] **Gate en FE:** en `my-vehicles/[id]/page.tsx` cambiar la condición actual
      `vehicle.fleetId && (...)` por `vehicle.fleetId && vehicle.tripStationEnabled && (...)`
      para `TripStationQrCard` + `VehicleTripsHistoryCard`.
- [ ] **Gate en BE (CLAVE — no alcanza con ocultar en FE):** en los endpoints de viajes
      (generar/rotar token de estación, registrar viaje, historial, y la página pública
      `/trip/{token}`) validar que la flota del vehículo tiene `TripStationEnabled == true`;
      si no, devolver 403/404. Ocultar la UI sin cerrar el backend deja la función
      accesible a cualquiera que conozca la URL.
- [ ] **Activación operativa:** toggle en el panel admin de la flota
      (`admin/fleets/[id]`) para que el taller la prenda/apague por flota. Alternativa
      rápida: setear el flag directo en DB.

**Por qué este enfoque:** encaja con el dominio (la función es "para dueños de flota"),
es persistente, auditable y gestionable desde la UI sin necesidad de deploy para
habilitar a un cliente nuevo.

---

## 📌 ACLARACIONES DEL CLIENTE / MECÁNICO — 2026-05-23

> Decisiones tomadas tras conversación con el cliente y el mecánico. Cierran preguntas
> que estaban abiertas en las tres áreas más sensibles del producto.

---

### 1. Stock e integración con depósito: NO maneja precios

**Decisión:**
El sistema de stock (GestionPGB) **no maneja precios**. Es un intermediario que solo
responde **si hay o no hay** un repuesto en el depósito propio del taller. El taller
(admin) lo consulta por nombre, código o lo que sea, para saber disponibilidad.

**Quién pone el precio:**
- El precio del repuesto lo carga **el empleado/recepcionista en el momento de armar
  la cotización**, porque esa información la obtienen de un sistema externo (proveedores)
  que NO es GestionPGB.
- Si el repuesto no está disponible en el depósito propio, el empleado consulta a
  proveedores externos (precio + disponibilidad) y ese precio lo escribe a mano en
  el ítem del presupuesto.

**Impacto en el diseño actual:**
- [ ] El buscador de productos del catálogo de GestionPGB (sección "Bloqueante crítico")
      debe traer **nombre + código + stock disponible**, pero **NO precio** del depósito.
      El campo "precio sugerido del depósito" sale del flujo esperado.
- [ ] El campo de precio en el ítem del presupuesto **siempre es editable manualmente**
      por el empleado, tanto si el repuesto vino del catálogo como si lo agregó a mano.
- [ ] El catálogo de GestionPGB sirve para **autocompletar nombre y código** y para
      saber si hay stock — no para sugerir precio.
- [ ] El `PriceSnapshot` de `WorkOrderService` / `WorkOrderPart` se llena con el valor
      que escribió el empleado, no con un valor traído del depósito.

**Reformulación del flujo:**
```
Recepcionista arma cotización
  → Busca repuesto en GestionPGB (nombre/código) → obtiene CÓDIGO + DISPONIBILIDAD
  → Si está disponible en depósito propio: marca el ítem, escribe precio manualmente
  → Si no está disponible: consulta sistema externo de proveedores (fuera del scope)
                            y escribe nombre + código + precio manualmente
  → Mano de obra: ver punto 3 (la define el mecánico)
```

---

### 2. Calendario de turnos: una patente ocupa la celda toda la duración del trabajo

**Decisión:**
La tabla/calendario por área de mecánico funciona así:
> **Si un vehículo va a estar 30 días con el motor, esa patente aparece en el
> rectángulo de "Motor" durante los 30 días completos.**

No es "el día que entra" ni "el día que termina" — es **cada día que el auto está
ocupando ese mecánico/área**.

**Esto cierra dos preguntas abiertas de la sección Turnos:**
- ~~"¿Qué pasa con autos que tardan más de un día? Si el motor lleva 3 días, ¿el auto
  aparece en motor del día 1, día 2 y día 3?"~~ → **Sí, aparece los 3 días.**
- ~~"¿Auto que tarda varios días: ocupa el slot en cada día?"~~ → **Sí.**

**Lo que sigue abierto:**
- [ ] **Auto con varios servicios simultáneos** (ej. tren delantero Y frenos al mismo
      tiempo): ¿aparece en las dos filas en simultáneo? Pregunta vigente.
- [ ] **¿Cómo se calcula la duración del trabajo?** El mecánico tiene que estimar
      cuántos días le va a llevar (ver punto 3 — el mecánico es el que sabe). Esa
      estimación es la que bloquea los días en el calendario.
- [ ] **¿Qué pasa si el trabajo se extiende más de lo estimado?** ¿El sistema corre
      automáticamente la ocupación o la oficina la extiende a mano?

**Implicancia en el modelo de datos:**
- La asignación de un servicio al mecánico necesita **fecha inicio + fecha fin estimada**
  (o duración en días). El calendario lee ese rango para pintar cada día.
- Si la fecha fin se mueve, el calendario refleja automáticamente la nueva ocupación.

---

### 3. Servicios y mano de obra: el mecánico cotiza, no es catálogo fijo

**Decisión:**
> No es lo mismo arreglar un tren delantero de Peugeot que un Mercedes. El mecánico
> informa lo que tiene. Para armar el presupuesto **el mecánico tiene que mandar,
> junto con su inspección, cuánto costará el trabajo, porque nadie sabe eso más que él.
> Su mano de obra.**

Esto choca con la entidad `CatalogService` actual, que tiene un `price` fijo. La
realidad es: el **nombre del servicio** puede estar catalogado ("Tren delantero",
"Cambio de embrague"), pero el **precio de mano de obra se define caso por caso**
según el auto y lo que encontró el mecánico.

**Impacto en el modelo:**
- [ ] El `price` de `CatalogService` deja de ser fuente de verdad. Pasa a ser, como
      mucho, un **precio orientativo / histórico** o se elimina.
- [ ] El `PriceSnapshot` de `WorkOrderService` siempre se llena con el valor que
      **el mecánico cotizó para ese auto puntual**, no con el del catálogo.
- [ ] El catálogo de servicios pasa a ser una lista de **nombres** + descripción
      estándar, no de precios.

**Impacto en el flujo (lo que cambia respecto al spec original):**

Hoy el spec dice:
```
2. El mecánico inicia el diagnóstico → admin agrega servicios del catálogo →
   3. admin cambia a "Esperando aprobación" → se envía presupuesto
```

Con esta aclaración, el flujo real es:
```
1. Auto entra (Received).
2. Admin pasa a Diagnosing.
3. CADA MECÁNICO DE CADA ÁREA hace su informe de lo que encuentra en su área
   (ya estaba mencionado al final del spec, líneas 2034-2037).
4. Junto con el informe, el mecánico responsable carga:
     - Servicios necesarios (nombres del catálogo o texto libre)
     - Costo de mano de obra por servicio (lo decide él)
     - Repuestos necesarios (con código de GestionPGB cuando aplica)
     - Estimación de días de trabajo (para reservar slot en el calendario)
5. La oficina/admin RECIBE esos informes, los CONSOLIDA en un presupuesto:
     - Filtra cuáles ítems van al cliente según el motivo de ingreso
     - Agrega precios de repuestos (manual, fuente externa — ver punto 1)
     - El precio de mano de obra ya viene del mecánico
6. Admin pasa a AwaitingApproval → se envía presupuesto al cliente.
```

**Endpoints / capacidades que faltan en el modelo actual:**
- [ ] **El mecánico necesita poder cargar `PriceSnapshot` y `EstimatedDays`** al
      reportar su inspección, no solo finalizar el trabajo. Hoy el mecánico solo
      tiene `accept` y `complete` con notas. Falta una acción tipo
      `submit-inspection` o `submit-quote` previa.
- [ ] **Estructura para "informe inicial por área"**: cada mecánico que revisa el
      auto deja un informe, no solo el del área del motivo de ingreso. Hoy el
      modelo asume que ya existen `WorkOrderService` cuando el mecánico interviene
      (los crea el admin). Tiene que invertirse: el mecánico propone, el admin
      decide qué entra al presupuesto.
- [ ] **Workflow de "borrador de presupuesto"**: necesitamos un estado intermedio
      donde los informes de los mecánicos están cargados pero el admin todavía no
      armó el presupuesto final. Hoy no existe.

**Preguntas que abre y faltan resolver:**
- [ ] ¿El catálogo de servicios se mantiene como **nombre + descripción** (sin precio)
      o se elimina y se reemplaza por texto libre?
- [ ] ¿Cómo se modela el "informe del mecánico por área" antes de que exista
      `WorkOrderService`? ¿Una nueva entidad `WorkOrderInspectionReport`?
- [ ] ¿Cada mecánico recibe automáticamente la orden en estado Diagnosing para
      hacer su informe, o el admin decide qué áreas/mecánicos miran cada auto?
- [ ] ¿Qué pasa si después de aprobado el presupuesto, el mecánico al desarmar
      encuentra más trabajo (y más mano de obra)? ¿Se vuelve a cotizar? ¿Hay un
      flujo de "ampliación de presupuesto"?

---

### Resumen de cambios al modelo (para próxima iteración)

| Área | Cambio | Entidades afectadas |
|------|--------|---------------------|
| Stock | El catálogo de GestionPGB devuelve disponibilidad, NO precio | `WorkOrderPart.PriceSnapshot` se carga manual |
| Catálogo | `CatalogService.Price` deja de ser fuente de verdad (o se elimina) | `CatalogService`, `WorkOrderService` |
| Inspección | El mecánico cotiza mano de obra + estima días en su informe | Nueva entidad o extensión de `WorkOrderService` |
| Calendario | Asignación de servicio necesita rango de fechas (no un día) | `WorkOrderService` (agregar `ScheduledStart`, `ScheduledEnd` o `EstimatedDays`) |
| Workflow | Estado intermedio: "informes cargados, presupuesto en armado" | `WorkOrderStatus` (posible nuevo valor) |


==== IMPLEMENTACION PARA SUMINSTRAR INFO A LOS CLIENTES DE SUS VEHICULOS ===

Resulta que las cubiertas de los vehiculos se desgastan. Esto se mide segun la cubierta. Pero cuando llega al 50% de altura, digamosle 5mm, ya es una señal de que se deben cambiar las cubiertas. Mi cliente quiere que el Software proporcione una estimacion por los datos que proporciona el cliente km, fecha de vencimiento de las cubiertas y la medida actual cuando llega al taller la cubierta. No se bien como encarar esto. Proporciono informacion en la web:

Inserta el sensor del medidor en el centro de las ranuras principales de la banda de rodamiento.
Toma varias medidas en diferentes puntos (interior, centro y exterior) para detectar desgastes irregulares.
Si el promedio de las mediciones es inferior a 3 mm, planifica el cambio de inmediato; si es inferior a 1,6 mm, el cambio es urgente

 Es un problema interesante porque tiene dos partes: modelo de datos (qué guardamos) y cálculo de estimación (cómo proyectamos el desgaste). Te paso cómo lo encararía.

Cómo lo modelo
Cada cubierta es una entidad propia (no un campo del vehículo), porque se reemplaza a lo largo de la vida del auto y mantener historial es clave para el cálculo.

Vehicle
  └─ VehicleTire (1 por posición: FL, FR, RL, RR, [Spare])
       ├─ Marca, Modelo, Medida ("185/65 R15")
       ├─ InstalledOn, InstalledAtKm
       ├─ InitialTreadDepthMm (default 8mm)
       ├─ ExpectedLifeKm (opcional, ej. 50.000)
       ├─ IsActive (false cuando se reemplaza)
       └─ VehicleTireMeasurement (N por cubierta)
            ├─ MeasuredOn, VehicleMileageAtMeasurement
            ├─ InnerDepthMm, CenterDepthMm, OuterDepthMm
            └─ AverageDepthMm (computed)
Por qué así:

Cuando se cambia una cubierta, no perdés el historial — la vieja queda con IsActive=false + ReplacedOn. Si el cliente vuelve a preguntar "¿cuántos km me duró la anterior?", el dato sigue ahí.
Las 3 mediciones por punto (interior/centro/exterior) detectan desgaste irregular — exactamente como dice la fuente que pasaste. Si la interior se desgasta más rápido que la exterior, el sistema avisa que probablemente hay un problema de alineación.
Cómo calculo la estimación
Es interpolación lineal sobre los km del auto. Dos modos según cuánta info tenemos:

Modo 1 — Solo hay 1 medición (o ninguna):

Tasa de desgaste = (Profundidad inicial − Profundidad actual) / (Km actual − Km al instalar)
Km restantes a 3mm = (Profundidad actual − 3) / Tasa
Km restantes a 1.6mm = (Profundidad actual − 1.6) / Tasa
Modo 2 — Hay ≥2 mediciones:

Regresión lineal sobre las últimas N mediciones (más preciso porque captura la tasa real de ese cliente, no la teórica).
Estados visuales:

Profundidad promedio	Estado	Color
≥ 5mm	Saludable	🟢
3 – 4.99mm	Atención	🟡
1.6 – 2.99mm	Cambiar pronto	🟠
< 1.6mm	URGENTE / ilegal	🔴
Detección de desgaste irregular:
Si en una medición la diferencia entre max(Inner, Center, Outer) y min(...) es > 2mm → flag "Desgaste irregular — revisar alineación/inflado".

Quién hace qué
Mecánico/Admin (en taller): mide y carga. Necesita un medidor de profundidad, el cliente no lo tiene.
Cliente: solo ve el estado + las estimaciones. Lo que mueve la confianza: "te queda X km en las delanteras".
Plan de sprint
#	Tarea	Esfuerzo
1	Entidades VehicleTire + VehicleTireMeasurement + enum TirePosition + migración	🟢
2	Servicio de cálculo (tasa de desgaste + proyección + estado)	🟡
3	Endpoints REST: CRUD cubiertas, agregar medición, GET con estimaciones	🟡
4	FE admin: panel "Cubiertas" en detalle del vehículo (4 slots + botones medir/reemplazar)	🟡
5	FE cliente: vista read-only en /my-vehicles/[id]	🟢

#	Tareas
#33	S10 — Dominio: VehicleTire + VehicleTireMeasurement — Enums TirePosition (FL/FR/RL/RR) + TireStatus. Entidades con todos los campos. Config EF. Migración + apply.	✅ YA HECHO	—
#34	S10 — Application: cálculo + commands + queries — TireWearCalculator (tasa de desgaste mm/km, proyección a 3mm y 1.6mm, status, flag de desgaste irregular). Commands: CreateVehicleTire, AddTireMeasurement, ReplaceTire. Query: GetTiresByVehicle con estimaciones.	⏸️ Pendiente	#33
#35	S10 — API: endpoints REST de cubiertas — GET /api/vehicles/{id}/tires, POST /api/vehicles/{id}/tires, POST /api/tires/{id}/measurements, POST /api/tires/{id}/replace. Autorización Admin + Receptionist + Mechanic.	⏸️ Pendiente	#34
#36	S10 — FE Admin: panel de cubiertas en detalle del vehículo — Card con 4 slots posicionales (visual del auto desde arriba). Estado + última medición + km estimados por cubierta. Botones "+ Medir" y "Reemplazar".	⏸️ Pendiente	#35
#37	S10 — FE Customer: vista read-only — En /my-vehicles/[id], card de solo lectura con estado de cubiertas + estimaciones + aviso de desgaste irregular.	⏸️ Pendiente	#35

! RECOMENDACION: No hacer esfuerzo exhaustivo por el FE, es importante hacer una arquitectura, plan y desarrollo solido de la funcionalidad primero. Es la prioridad. Y estar con confianza de que no rompe el sistema.

---

## ISSUE ABIERTO — Duraciones de servicios y organización del taller

**Estado:** 🟡 Abierto — pendiente de decisión de producto. NO implementar hasta resolver las preguntas. (Anotado 2026-06-08)

### Contexto
Ya se migró la duración estimada de **días → minutos** (`EstimatedDays` → `EstimatedDurationMinutes`, 1 jornada = 480 min). Funciona. Pero quedó destapado un tema más profundo: **cómo las duraciones de servicios alimentan la organización del taller.**

### Hallazgo clave: hay DOS duraciones que NO se hablan
Cada `WorkOrderService` tiene dos campos de duración independientes:

| Campo | Origen | Hoy se usa para… |
|-------|--------|------------------|
| `EstimatedDurationMinutesSnapshot` (int, congelado al agregar) | Copiado de `CatalogService.EstimatedDurationMinutes` | **Dashboard "Carga del taller"** (`WorkshopLoadCard`). `DashboardRepository.cs` (~L104): `Sum(EstimatedDurationMinutesSnapshot * Quantity)` → total pendiente + carga por mecánico |
| `EstimatedDurationMinutes` (nullable) | Lo carga el **mecánico** en la inspección | **Solo** calcular `ScheduledEnd = Start + min` (`ScheduleServiceCommandHandler`). Decide en qué días cae el trabajo en el **Calendario** (`admin/calendar/page.tsx`) |

### El gap
1. **La estimación del mecánico NO impacta la carga del taller.** El dashboard usa la duración del catálogo. La estimación hands-on (más realista) solo agenda.
2. **El calendario no muestra duración ni horario** en el chip. Un trabajo de 3 h se ve igual que uno de 2 días.
3. **`ScheduledEnd` es wall-clock** (`AddMinutes`). "2 jornadas" = 16 h de reloj corridas, no 2 días laborales. La grilla día×área puede no representar bien la ocupación.

### Preguntas abiertas (a resolver luego)
1. **Fuente de la carga del taller.** ¿El estimado del mecánico debería pisar el snapshot del catálogo para el cálculo de carga (con fallback al catálogo si el mecánico no estimó)? ¿O se mantienen separados?
2. **Visibilidad en el calendario.** ¿Mostrar duración (ej. "3h" / "2d") y/o horario de inicio en el chip?
3. **Semántica de `ScheduledEnd`.** ¿Modelar horario de taller (apertura/cierre) para que "2 jornadas" ocupe 2 días laborales en la grilla, en vez de 16 h corridas?
4. **¿Una sola duración o dos?** ¿Unificar (mecánico override catálogo) o mantener catálogo = default + mecánico = ajuste fino y mostrar ambas?

### Archivos relevantes (para cuando se retome)
- Backend: `Data/Repositories/DashboardRepository.cs` (~L104); `Application/Features/WorkOrderServices/Commands/ScheduleService/ScheduleServiceCommandHandler.cs`; `Domain/Entities/WorkOrderService.cs`; `Data/Configurations/WorkOrderServiceConfiguration.cs` (snapshot frozen).
- Frontend: `src/components/dashboard/WorkshopLoadCard.tsx`; `src/app/(admin)/admin/calendar/page.tsx` (`SlotChip`); `src/lib/format.ts` (`formatEstimatedDuration` cliente / `formatWorkDuration` interno).

### Plan tentativo (depende de las respuestas)
| # | Tarea | Estado | Dep |
|---|-------|--------|-----|
| D1 | Decidir las 4 preguntas de arriba (producto) | ⏸️ Pendiente | — |
| D2 | (Si aplica) Carga del taller usa estimado del mecánico con fallback a catálogo | ⏸️ Pendiente | D1 |
| D3 | (Si aplica) Mostrar duración/horario en el chip del calendario | ⏸️ Pendiente | D1 |
| D4 | (Si aplica) Modelar horario de taller para ocupación día×área | ⏸️ Pendiente | D1 |