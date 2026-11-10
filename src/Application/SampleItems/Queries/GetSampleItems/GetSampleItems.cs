using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.SampleItems.Queries.GetSampleItems;

[Authorize(Permissions = Permissions.ViewSampleItems)]
public record GetSampleItemsQuery : IRequest<SampleItemsVm>;

public class GetSampleItemsQueryHandler : IRequestHandler<GetSampleItemsQuery, SampleItemsVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSampleItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SampleItemsVm> Handle(GetSampleItemsQuery request, CancellationToken cancellationToken)
    {
        return new SampleItemsVm
        {
            SampleItems = await _context.SampleItems
                .AsNoTracking()
                .ProjectTo<SampleItemDto>(_mapper.ConfigurationProvider)
                .OrderBy(c => c.Description)
                .ToListAsync(cancellationToken)
        };
    }
}
