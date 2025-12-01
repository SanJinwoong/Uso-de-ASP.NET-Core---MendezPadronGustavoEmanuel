/*
 * Program.cs - Punto de entrada de la aplicación
 * Autor: Méndez Padrón Gustavo Emanuel
 * Universidad Autónoma de Tamaulipas
 * 
 * Configuración de servicios, middleware y pipeline de la aplicación ASP.NET Core
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taller_ASP.NET_Core.Data;

var builder = WebApplication.CreateBuilder(args);

// ============ CONFIGURACIÓN DE SERVICIOS ============

// Configurar cadena de conexión a SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrar DbContext con SQLite (Gustavo Méndez - UAT)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar ASP.NET Core Identity para autenticación
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
	.AddEntityFrameworkStores<ApplicationDbContext>();

// Agregar soporte para controladores con vistas (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============ CONFIGURACIÓN DEL PIPELINE HTTP ============

// Configuración para ambiente de desarrollo
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint(); // Página de migraciones para desarrollo
}
else
{
	app.UseExceptionHandler("/Home/Error"); // Manejo de errores en producción
	// Configuración HSTS para seguridad (HTTPS obligatorio)
	app.UseHsts();
}

// Redireccionar HTTP a HTTPS automáticamente
app.UseHttpsRedirection();

// Habilitar archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// Configurar enrutamiento
app.UseRouting();

// Habilitar autenticación y autorización
app.UseAuthorization();

// Configurar ruta por defecto: Home/Index
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear Razor Pages (usado por Identity)
app.MapRazorPages();

// Iniciar la aplicación
app.Run();
