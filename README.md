# Taller ASP.NET Core - Sistema de Gestión de Tareas

**Autor:** Méndez Padrón Gustavo Emanuel  
**Universidad:** Universidad Autónoma de Tamaulipas  
**Taller:** Uso de ASP.NET Core  
**Fecha:** Noviembre 2025

---

## Descripción del Proyecto

Sistema web de gestión de tareas desarrollado con **ASP.NET Core 8.0** (actualizado a .NET 9.0) que implementa el patrón arquitectónico **Modelo-Vista-Controlador (MVC)**. 

La aplicación permite a los usuarios autenticados crear, editar, eliminar y organizar sus tareas personales de manera eficiente, con funcionalidades de búsqueda, filtrado y ordenamiento mediante drag & drop.

---

## Características Principales

### Autenticación y Seguridad
- Sistema de autenticación con **ASP.NET Core Identity**
- Registro e inicio de sesión de usuarios
- Cada usuario tiene acceso únicamente a sus propias tareas
- Validación de sesiones y protección de rutas

### Gestión de Tareas (CRUD Completo)
- **Crear** tareas con título, descripción e imagen opcional
- **Leer/Visualizar** lista completa de tareas del usuario
- **Editar** tareas existentes
- **Eliminar** tareas
- **Marcar** tareas como completadas o pendientes

### Funcionalidades Avanzadas
- **Búsqueda:** Filtrar tareas por título o descripción en tiempo real
- **Filtros:** Visualizar todas las tareas, solo pendientes o solo completadas
- **Ordenamiento:** Reorganizar tareas mediante drag & drop (SortableJS)
- **Imágenes:** Adjuntar imágenes a las tareas
- **Interfaz responsiva:** Compatible con dispositivos móviles y escritorio

---

## Tecnologías Utilizadas

| Tecnología | Versión | Descripción |
|-----------|---------|-------------|
| **ASP.NET Core** | 9.0 | Framework web MVC |
| **C#** | 12.0 | Lenguaje de programación |
| **Entity Framework Core** | 8.0 | ORM para acceso a datos |
| **SQLite** | - | Base de datos relacional |
| **ASP.NET Core Identity** | 8.0 | Sistema de autenticación |
| **Bootstrap** | 5.x | Framework CSS |
| **jQuery** | 3.x | Librería JavaScript |
| **SortableJS** | 1.15.x | Drag & Drop |

---

## Arquitectura del Proyecto

### Patrón MVC (Modelo-Vista-Controlador)

```
Taller ASP.NET Core/
├── Controllers/          # Lógica de negocio
│   ├── HomeController.cs
│   └── TasksController.cs
├── Models/               # Entidades de datos
│   ├── TaskItem.cs
│   └── ErrorViewModel.cs
├── Views/                # Interfaz de usuario
│   ├── Home/
│   ├── Tasks/
│   └── Shared/
├── Data/                 # Contexto de base de datos
│   └── ApplicationDbContext.cs
├── wwwroot/              # Archivos estáticos (CSS, JS)
└── Migrations/           # Migraciones de EF Core
```

### Base de Datos (SQLite)

**Tabla: TaskItems**
- `id`: Identificador único (PK)
- `Title`: Título de la tarea (máx 200 caracteres)
- `Description`: Descripción (máx 1000 caracteres)
- `IsCompleted`: Estado (completada/pendiente)
- `Order`: Orden para drag & drop
- `Image`: Imagen adjunta (bytes)
- `ImageContentType`: Tipo MIME
- `CreatedAt`: Fecha de creación
- `UserId`: Usuario propietario (FK)

---

## Instalación y Ejecución

### Requisitos Previos
- Visual Studio 2022 o superior
- .NET 9.0 SDK
- Git (opcional)

### Pasos de Instalación

**1. Clonar o descargar el repositorio**
```bash
git clone https://github.com/TU_USUARIO/Taller-ASP-Core-MendezPadron.git
cd Taller-ASP-Core-MendezPadron
```

**2. Abrir el proyecto**
- Abrir Visual Studio 2022
- Abrir el archivo `Taller ASP.NET Core.sln`

**3. Restaurar paquetes NuGet**
```bash
dotnet restore
```

**4. Ejecutar la aplicación**
```bash
dotnet run
```

O presionar `F5` en Visual Studio.

**5. Acceder a la aplicación**

Abrir el navegador en: `http://localhost:5152`

---

## Uso de la Aplicación

### Primera Vez
1. Hacer clic en **"Registrarse"**
2. Crear una cuenta con email y contraseña
3. Iniciar sesión
4. Comenzar a crear tareas

### Funcionalidades Principales
- **Nueva Tarea:** Botón verde "Nueva Tarea"
- **Editar:** Click en el ícono de lápiz
- **Eliminar:** Click en el ícono de papelera
- **Completar:** Marcar el checkbox
- **Ordenar:** Arrastrar y soltar tareas
- **Buscar:** Escribir en el campo de búsqueda
- **Filtrar:** Seleccionar "Todas", "Pendientes" o "Completadas"

---

## Capturas de Pantalla

### Página de Inicio
Pantalla de bienvenida con opciones de registro e inicio de sesión.

### Panel de Tareas
Vista principal con lista de tareas, búsqueda, filtros y panel de detalles.

### Crear/Editar Tarea
Formularios con validaciones para gestionar tareas.

---

## Funcionalidades Destacadas

### 1. Búsqueda Inteligente
```csharp
// Búsqueda en título y descripción (TasksController.cs)
if (!string.IsNullOrWhiteSpace(searchTerm))
{
    query = query.Where(t =>
        t.Title.Contains(searchTerm) ||
        (t.Description != null && t.Description.Contains(searchTerm))
    );
}
```

### 2. Ordenamiento Drag & Drop
```javascript
// Implementación con SortableJS (tasks.js)
new Sortable(taskList, {
    animation: 150,
    handle: '.task-drag-handle',
    onEnd: function (evt) {
        updateTaskOrder();
    }
});
```

### 3. Seguridad por Usuario
```csharp
// Solo el propietario puede ver/editar sus tareas
var userId = _userManager.GetUserId(User);
var query = _context.TaskItems.Where(t => t.UserId == userId);
```

---

## Seguridad Implementada

- Autenticación obligatoria para acceder a tareas
- Validación de propiedad antes de editar/eliminar
- Protección CSRF con tokens antifalsificación
- Validaciones de modelo en cliente y servidor
- Sanitización de entradas de usuario
- Logging de intentos de acceso no autorizado

---

## Aprendizajes y Conceptos Aplicados

Durante el desarrollo de este proyecto se aplicaron los siguientes conceptos:

### Programación
- Patrón MVC y separación de responsabilidades
- Programación asíncrona con `async/await`
- LINQ para consultas a base de datos
- Inyección de dependencias
- Manejo de excepciones y logging

### ASP.NET Core
- Razor Pages y sintaxis Razor
- Tag Helpers para formularios
- Partial Views para componentes reutilizables
- Model Binding y validación
- Entity Framework Core y migraciones

### Frontend
- Diseño responsivo con Bootstrap
- JavaScript moderno (ES6+)
- AJAX con Fetch API
- Manipulación del DOM
- Eventos y delegación

---

## Solución de Problemas

### Error: "No se puede conectar a la base de datos"
**Solución:** Ejecutar `Update-Database` en la consola de paquetes.

### Error: "Página no encontrada (404)"
**Solución:** Verificar que la aplicación esté ejecutándose en el puerto correcto.

### Las imágenes no se muestran
**Solución:** Verificar que el tipo MIME esté correctamente configurado en el modelo.

### No se puede ordenar con drag & drop
**Solución:** Verificar que SortableJS esté cargado desde el CDN en `_Layout.cshtml`.

---

## Estructura de Archivos Importantes

```
TasksController.cs       → Lógica principal de tareas (Index, Create, Edit, Delete)
TaskItem.cs              → Modelo de datos con validaciones
_Layout.cshtml           → Layout maestro de la aplicación
Index.cshtml (Tasks)     → Vista principal de tareas
tasks.js                 → JavaScript para drag & drop y AJAX
ApplicationDbContext.cs  → Contexto de Entity Framework
```

---

## Posibles Mejoras Futuras

- Agregar fechas de vencimiento a las tareas
- Implementar categorías o etiquetas
- Notificaciones push para tareas próximas a vencer
- Exportar tareas a PDF o Excel
- Modo oscuro para la interfaz
- Aplicación móvil nativa (Xamarin/MAUI)
- API RESTful para integración con terceros

---

## Contacto

**Autor:** Méndez Padrón Gustavo Emanuel  
**Universidad:** Universidad Autónoma de Tamaulipas  
**Email:** [tu-email@uat.edu.mx]  
**GitHub:** [https://github.com/TU_USUARIO]

---

## Licencia

Este proyecto fue desarrollado con fines educativos como parte del curso de Desarrollo de Aplicaciones Web en la Universidad Autónoma de Tamaulipas.

---

## Agradecimientos

- Universidad Autónoma de Tamaulipas
- Profesor del curso de Desarrollo Web
- Comunidad de ASP.NET Core
- Microsoft Documentation

---

**Desarrollado por Gustavo Méndez | UAT 2025**
