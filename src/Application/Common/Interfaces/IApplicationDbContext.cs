using BaseApp.Domain.Entities;

namespace BaseApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
