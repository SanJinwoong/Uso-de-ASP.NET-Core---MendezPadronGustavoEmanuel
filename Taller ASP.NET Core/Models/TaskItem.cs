/*
 * TaskItem Model - Modelo de datos para las tareas
 * Autor: Méndez Padrón Gustavo Emanuel
 * Universidad Autónoma de Tamaulipas
 * 
 * Este modelo representa una tarea en el sistema con todas sus propiedades
 * y validaciones necesarias para garantizar integridad de datos.
 */

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taller_ASP.NET_Core.Models
{
    public class TaskItem
    {
        // ============ PROPIEDADES PRINCIPALES ============

        // ============ PROPIEDADES PRINCIPALES ============

        // Llave primaria autoincrementable
        public int id { get; set; }

        // Título de la tarea (obligatorio, máximo 200 caracteres)
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título de la Tarea")]
        public string Title { get; set; } = string.Empty;

        // Descripción detallada (opcional, máximo 1000 caracteres)
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        // ============ ESTADO Y CONTROL ============

        // Indica si la tarea está completada
        [Display(Name = "Completada")]
        public bool IsCompleted { get; set; } = false;

        // Orden para funcionalidad drag & drop (personalizado por Gustavo Méndez)
        public int Order { get; set; } = 0;

        // ============ SOPORTE DE IMÁGENES ============

        // Almacenamiento de imagen como bytes en BD
        public byte[]? Image { get; set; }
        
        // Tipo MIME de la imagen (ej: image/jpeg, image/png)
        public string? ImageContentType { get; set; }

        // Archivo temporal para subir imagen (no se persiste en BD)
        [NotMapped]
        [Display(Name = "Imagen de la Tarea")]
        public IFormFile? ImageFile { get; set; }

        // ============ AUDITORÍA Y SEGURIDAD ============

        // Fecha y hora de creación
        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relación con el usuario propietario (Foreign Key a AspNetUsers)
        [BindNever] // No se vincula desde formularios por seguridad
        public string UserId { get; set; } = string.Empty;
    }
}