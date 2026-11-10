using BaseApp.Domain.Entities;

namespace BaseApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<SampleItem> SampleItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
