# Documentación Técnica - Sistema de Gestión de Tareas

**Autor:** Méndez Padrón Gustavo Emanuel  
**Universidad:** Universidad Autónoma de Tamaulipas  
**Fecha:** 30 de Noviembre de 2025

---

## 📌 Introducción

Este documento describe la implementación técnica del sistema de gestión de tareas desarrollado con ASP.NET Core 8.0 en el taller de desarrollo web, siguiendo el patrón arquitectónico MVC y aplicando buenas prácticas.

---

## 🏗️ Arquitectura del Sistema

### Patrón MVC Implementado

El proyecto sigue estrictamente el patrón Modelo-Vista-Controlador:

#### **Modelos (Models/)**
- `TaskItem.cs`: Entidad principal que representa una tarea
  - Propiedades: id, Title, Description, IsCompleted, Order, Image, CreatedAt, UserId
  - Validaciones mediante Data Annotations
  - Relación con AspNetUsers (Identity)

#### **Vistas (Views/)**
- `Tasks/Index.cshtml`: Lista principal con búsqueda y filtros
- `Tasks/Create.cshtml`: Formulario de creación
- `Tasks/Edit.cshtml`: Formulario de edición
- `Tasks/_TaskCard.cshtml`: Componente parcial para tarjetas
- `Tasks/_TaskDetail.cshtml`: Panel de detalles dinámico

#### **Controladores (Controllers/)**
- `TasksController.cs`: Lógica de negocio para CRUD de tareas
  - Métodos: Index, Create, Edit, Delete, ToggleComplete, UpdateOrder
  - Validaciones de seguridad y autorización

---

## 🔒 Sistema de Autenticación

### ASP.NET Core Identity

Se implementó el sistema de identidad completo de ASP.NET Core:

```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Características:**
- Registro de usuarios con validación de email
- Login seguro con hash de contraseñas
- Protección de rutas con `[Authorize]`
- Aislamiento de datos por usuario

**Seguridad Implementada:**
```csharp
// Verificación de propiedad antes de editar/eliminar
var userId = _userManager.GetUserId(User);
if (task.UserId != userId)
{
    _logger.LogWarning($"Acceso no autorizado...");
    return Forbid();
}
```

---

## 💾 Base de Datos

### Entity Framework Core con SQLite

**Configuración:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

**Esquema de Base de Datos:**

**Tabla: TaskItems**
```sql
CREATE TABLE TaskItems (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    IsCompleted BIT NOT NULL DEFAULT 0,
    Order INTEGER NOT NULL DEFAULT 0,
    Image BLOB,
    ImageContentType NVARCHAR(100),
    CreatedAt DATETIME NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
```

**Migraciones Aplicadas:**
- `20251123024539_InitialCreate`: Estructura inicial de la BD

---

## 🎯 Funcionalidades Implementadas

### 1. CRUD Completo

#### **Create (Crear)**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(TaskItem task)
{
    if (!ModelState.IsValid) return View(task);
    
    task.UserId = _userManager.GetUserId(User);
    task.CreatedAt = DateTime.Now;
    
    // Procesar imagen si existe
    if (task.ImageFile != null && task.ImageFile.Length > 0)
    {
        using var memoryStream = new MemoryStream();
        await task.ImageFile.CopyToAsync(memoryStream);
        task.Image = memoryStream.ToArray();
        task.ImageContentType = task.ImageFile.ContentType;
    }
    
    _context.TaskItems.Add(task);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

#### **Read (Leer)**
- Vista de lista con todas las tareas del usuario
- Panel de detalles cargado dinámicamente con AJAX

#### **Update (Actualizar)**
- Formulario de edición con datos precargados
- Bloqueo de edición si la tarea está completada

#### **Delete (Eliminar)**
- Eliminación con confirmación
- Validación de propiedad antes de eliminar

### 2. Búsqueda y Filtrado

```csharp
// Búsqueda en título y descripción
if (!string.IsNullOrWhiteSpace(searchTerm))
{
    query = query.Where(t =>
        t.Title.Contains(searchTerm) ||
        (t.Description != null && t.Description.Contains(searchTerm))
    );
}

// Filtro por estado
switch (filter?.ToLower())
{
    case "pending":
        query = query.Where(t => !t.IsCompleted);
        break;
    case "completed":
        query = query.Where(t => t.IsCompleted);
        break;
}
```

### 3. Ordenamiento Drag & Drop

**Frontend (JavaScript):**
```javascript
new Sortable(taskList, {
    animation: 150,
    handle: '.task-drag-handle',
    onEnd: function (evt) {
        if (evt.oldIndex !== evt.newIndex) {
            updateTaskOrder();
        }
    }
});
```

**Backend (API):**
```csharp
[HttpPost]
public async Task<IActionResult> UpdateOrder([FromBody] List<int> taskIds)
{
    var userId = _userManager.GetUserId(User);
    
    for (int i = 0; i < taskIds.Count; i++)
    {
        var task = await _context.TaskItems.FindAsync(taskIds[i]);
        if (task != null && task.UserId == userId)
        {
            task.Order = i;
        }
    }
    
    await _context.SaveChangesAsync();
    return Ok(new { success = true });
}
```

---

## 🎨 Frontend

### Tecnologías Utilizadas

1. **Bootstrap 5**
   - Grid system responsivo
   - Componentes: cards, modals, forms, buttons
   - Utilidades de espaciado y colores

2. **Bootstrap Icons**
   - Iconografía consistente en toda la aplicación
   - Más de 20 iconos utilizados

3. **JavaScript Moderno (ES6+)**
   - Async/await para AJAX
   - Fetch API para comunicación con backend
   - Event listeners y manipulación del DOM

4. **SortableJS**
   - Librería para drag & drop
   - Integración con API del backend

### Diseño Responsivo

```css
/* Ejemplo de estilos personalizados */
.task-card {
    background: white;
    border: 2px solid #e9ecef;
    border-radius: 10px;
    transition: all 0.3s ease;
}

.task-card:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
}
```

---

## 🔧 Configuración del Proyecto

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Paquetes NuGet Principales

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

---

## 📊 Flujo de Datos

### Flujo de Creación de Tarea

```
Usuario → Formulario Create
    ↓
POST /Tasks/Create
    ↓
TasksController.Create()
    ↓
Validaciones ModelState
    ↓
Asignar UserId y CreatedAt
    ↓
Procesar imagen (si existe)
    ↓
Guardar en BD (EF Core)
    ↓
Redirect → /Tasks/Index
```

### Flujo de Ordenamiento

```
Usuario arrastra tarea
    ↓
Sortable.js onEnd event
    ↓
JavaScript updateTaskOrder()
    ↓
Obtener IDs en nuevo orden
    ↓
POST /Tasks/UpdateOrder
    ↓
TasksController.UpdateOrder()
    ↓
Actualizar Order en BD
    ↓
Respuesta JSON { success: true }
```

---

## 🧪 Validaciones Implementadas

### Validaciones del Modelo

```csharp
[Required(ErrorMessage = "El título es obligatorio")]
[StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
public string Title { get; set; }

[StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
public string? Description { get; set; }
```

### Validaciones de Seguridad

1. **Autorización:**
   - Atributo `[Authorize]` en el controlador
   - Verificación de propiedad en cada acción

2. **CSRF Protection:**
   - Token antifalsificación en formularios
   - Atributo `[ValidateAntiForgeryToken]`

3. **Sanitización:**
   - Validación de tipos de archivo
   - Límites de tamaño de imagen
   - Escape automático en Razor

---

## 📈 Mejoras Aplicadas

### Optimizaciones de Rendimiento

1. **Consultas Eficientes:**
   ```csharp
   var tasks = await query.OrderBy(t => t.Order).ToListAsync();
   ```

2. **Carga Asíncrona:**
   - Todos los métodos del controlador son `async`
   - Uso de `await` para operaciones I/O

3. **Partial Views:**
   - Componentes reutilizables (`_TaskCard`, `_TaskDetail`)
   - Reduce duplicación de código

### Experiencia de Usuario

1. **Feedback Visual:**
   - Animaciones en hover
   - Indicadores de estado (completado/pendiente)
   - Contador de tareas

2. **Interactividad:**
   - Búsqueda sin recargar página
   - Panel de detalles dinámico
   - Drag & drop intuitivo

---

## 🐛 Manejo de Errores

### Logging Implementado

```csharp
private readonly ILogger<TasksController> _logger;

// Ejemplo de uso
_logger.LogWarning($"Acceso no autorizado a tarea {id} por usuario {userId}");
_logger.LogError(ex, "Error al crear tarea");
```

### Try-Catch en Operaciones Críticas

```csharp
try
{
    _context.TaskItems.Remove(task);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
catch (Exception ex)
{
    _logger.LogError(ex, $"Error al eliminar tarea {id}");
    TempData["Error"] = "Error al eliminar la tarea.";
    return RedirectToAction(nameof(Index));
}
```

---

## 📚 Conceptos Aprendidos

Durante el desarrollo de este proyecto se aplicaron:

### Backend
- ✅ Patrón MVC
- ✅ Entity Framework Core
- ✅ ASP.NET Core Identity
- ✅ Inyección de dependencias
- ✅ Programación asíncrona
- ✅ LINQ para consultas
- ✅ Data Annotations
- ✅ Logging y manejo de errores

### Frontend
- ✅ Razor Syntax
- ✅ Tag Helpers
- ✅ Partial Views
- ✅ Bootstrap 5
- ✅ JavaScript ES6+
- ✅ AJAX con Fetch API
- ✅ Manipulación del DOM
- ✅ Librerías de terceros (SortableJS)

---

## 🚀 Despliegue

### Requisitos del Sistema

- .NET 8.0 SDK
- Visual Studio 2022 (recomendado)
- SQLite (incluido)

### Comandos de Ejecución

```bash
# Restaurar paquetes
dotnet restore

# Aplicar migraciones
dotnet ef database update

# Ejecutar aplicación
dotnet run
```

---

## 📞 Información del Desarrollador

**Nombre Completo:** Méndez Padrón Gustavo Emanuel  
**Universidad:** Universidad Autónoma de Tamaulipas  
**Curso:** Desarrollo de Aplicaciones Web con ASP.NET Core  
**Año:** 2025

---

## 📄 Referencias

- [Documentación oficial de ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0)
- [SortableJS GitHub](https://github.com/SortableJS/Sortable)

---

**Última actualización:** 30 de Noviembre de 2025  
**Versión del proyecto:** 1.0.0
