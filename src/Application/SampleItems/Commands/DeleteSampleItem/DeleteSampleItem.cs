using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.SampleItems.Commands.DeleteSampleItem;

[Authorize(Permissions = Permissions.DeleteSampleItems)]
public record DeleteSampleItemCommand(int Id) : IRequest;

public class DeleteSampleItemCommandHandler : IRequestHandler<DeleteSampleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSampleItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSampleItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SampleItems
            .Where(c => c.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.SampleItems.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
