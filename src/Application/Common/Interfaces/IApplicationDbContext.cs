using BaseApp.Domain.Entities;

namespace BaseApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<SampleItem> SampleItems { get; }

    DbSet<Categoria> Categorias { get; }

    DbSet<Cobrador> Cobradores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
