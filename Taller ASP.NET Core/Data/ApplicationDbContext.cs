using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Taller_ASP.NET_Core.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		// DbSet para la tabla Tasks
		public DbSet<Models.TaskItem> TaskItems { get; set; }		
	}
}
