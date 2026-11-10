using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Entities;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.SampleItems.Commands.CreateSampleItem;

[Authorize(Permissions = Permissions.CreateSampleItems)]
public record CreateSampleItemCommand : IRequest<int>
{
    public string? Description { get; init; }

    public decimal Amount { get; init; }
}

public class CreateSampleItemCommandHandler : IRequestHandler<CreateSampleItemCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSampleItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateSampleItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new SampleItem
        {
            Description = request.Description!,
            Amount = request.Amount
        };

        _context.SampleItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
