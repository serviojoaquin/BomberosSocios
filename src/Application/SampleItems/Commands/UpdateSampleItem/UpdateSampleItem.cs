using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.SampleItems.Commands.UpdateSampleItem;

[Authorize(Permissions = Permissions.EditSampleItems)]
public record UpdateSampleItemCommand : IRequest
{
    public int Id { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }
}

public class UpdateSampleItemCommandHandler : IRequestHandler<UpdateSampleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSampleItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateSampleItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SampleItems
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Description = request.Description!;
        entity.Amount = request.Amount;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
