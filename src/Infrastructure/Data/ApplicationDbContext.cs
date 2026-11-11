using System.Reflection;
using BaseApp.Application.Common.Interfaces;
using BaseApp.Domain.Entities;
using BaseApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaseApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<SampleItem> SampleItems => Set<SampleItem>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Cobrador> Cobradores => Set<Cobrador>();

    public DbSet<Socio> Socios => Set<Socio>();

    public DbSet<Cuota> Cuotas => Set<Cuota>();

    public DbSet<Reporte> Reportes => Set<Reporte>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
