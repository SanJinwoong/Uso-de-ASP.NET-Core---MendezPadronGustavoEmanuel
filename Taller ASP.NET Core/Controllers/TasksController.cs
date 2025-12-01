/*
 * TasksController - Controlador principal para la gestión de tareas
 * Autor: Méndez Padrón Gustavo Emanuel
 * Universidad Autónoma de Tamaulipas
 * Fecha: Noviembre 2025
 * 
 * Este controlador implementa el patrón MVC para manejar todas las operaciones CRUD
 * de tareas, incluyendo autenticación, autorización y funcionalidades avanzadas
 * como filtrado, búsqueda y ordenamiento drag & drop.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taller_ASP.NET_Core.Data;
using Taller_ASP.NET_Core.Models;

namespace Taller_ASP.NET_Core.Controllers
{
    [Authorize] // Requiere autenticación para todas las acciones
    public class TasksController : Controller
    {
        // Inyección de dependencias para acceso a BD, usuarios y logging
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<TasksController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ==================== INDEX (CON BÚSQUEDA Y FILTROS) ====================
        /// <summary>
        /// Muestra la lista de tareas del usuario autenticado con capacidades de búsqueda y filtrado.
        /// Implementación personalizada por Gustavo Méndez para optimizar la experiencia del usuario.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda en título y descripción</param>
        /// <param name="filter">Filtro por estado: all, pending, completed</param>
        public async Task<IActionResult> Index(string searchTerm, string filter = "all")
        {
            // Obtener el ID del usuario actual para seguridad
            var userId = _userManager.GetUserId(User);
            
            // Query base: solo tareas del usuario autenticado
            var query = _context.TaskItems.Where(t => t.UserId == userId);

            // Aplicar filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm))
                );
            }

            // Aplicar filtro de estado
            switch (filter?.ToLower())
            {
                case "pending":
                    query = query.Where(t => !t.IsCompleted);
                    break;
                case "completed":
                    query = query.Where(t => t.IsCompleted);
                    break;
            }

            var tasks = await query.OrderBy(t => t.Order).ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Filter = filter ?? "all";

            return View(tasks);
        }

        // ==================== GET DETAIL ====================

        public async Task<IActionResult> GetTaskDetail(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (task.UserId != userId)
            {
                _logger.LogWarning($"Acceso no autorizado a tarea {id} por usuario {userId}");
                return Forbid();
            }

            return PartialView("_TaskDetail", task);
        }

        // ==================== CREATE ====================

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                return View(task);
            }

            try
            {
                task.UserId = _userManager.GetUserId(User) ?? string.Empty;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tarea");
                ModelState.AddModelError("", "Error al guardar la tarea. Por favor, intenta de nuevo.");
                return View(task);
            }
        }

        // ==================== EDIT ====================

        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (task.UserId != userId)
            {
                _logger.LogWarning($"Intento de edición no autorizada de tarea {id} por usuario {userId}");
                return Forbid();
            }

            // Bloquear edición si está completada
            if (task.IsCompleted)
            {
                TempData["Error"] = "No puedes editar una tarea completada. Desmárcala como completada primero.";
                return RedirectToAction(nameof(Index));
            }

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {
            if (id != task.id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            var existingTask = await _context.TaskItems.FindAsync(id);

            if (existingTask == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (existingTask.UserId != userId)
            {
                _logger.LogWarning($"Intento de edición no autorizada de tarea {id} por usuario {userId}");
                return Forbid();
            }

            // Bloquear edición si está completada
            if (existingTask.IsCompleted)
            {
                TempData["Error"] = "No puedes editar una tarea completada. Desmárcala como completada primero.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;

                // Procesar imagen si existe
                if (task.ImageFile != null && task.ImageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await task.ImageFile.CopyToAsync(memoryStream);
                    existingTask.Image = memoryStream.ToArray();
                    existingTask.ImageContentType = task.ImageFile.ContentType;
                }

                _context.Update(existingTask);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar tarea {id}");
                ModelState.AddModelError("", "Error al guardar los cambios. Por favor, intenta de nuevo.");
                return View(task);
            }
        }

        // ==================== DELETE ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (task.UserId != userId)
            {
                _logger.LogWarning($"Intento de eliminación no autorizada de tarea {id} por usuario {userId}");
                return Forbid();
            }

            try
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar tarea {id}");
                TempData["Error"] = "Error al eliminar la tarea. Por favor, intenta de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== TOGGLE COMPLETE ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (task.UserId != userId)
            {
                _logger.LogWarning($"Intento no autorizado de cambiar estado de tarea {id} por usuario {userId}");
                return Forbid();
            }

            try
            {
                task.IsCompleted = !task.IsCompleted;
                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar estado de tarea {id}");
                TempData["Error"] = "Error al cambiar el estado de la tarea. Por favor, intenta de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== UPDATE ORDER ====================

        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> taskIds)
        {
            if (taskIds == null || taskIds.Count == 0)
            {
                return BadRequest(new { success = false, message = "No se recibieron IDs" });
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                int updated = 0;

                for (int i = 0; i < taskIds.Count; i++)
                {
                    var task = await _context.TaskItems.FindAsync(taskIds[i]);
                    if (task != null && task.UserId == userId)
                    {
                        task.Order = i;
                        updated++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, updated = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar orden de tareas");
                return StatusCode(500, new { success = false, message = "Error al actualizar el orden" });
            }
        }
    }
}