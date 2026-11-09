using BaseApp.Application.Common.Interfaces;
using BaseApp.Application.Common.Security;
using BaseApp.Domain.Enums;

namespace BaseApp.Application.Categories.Queries.GetCategories;

[Authorize(Permissions = Permissions.ViewCategories)]
public record GetCategoriesQuery : IRequest<CategoriesVm>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, CategoriesVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CategoriesVm> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return new CategoriesVm
        {
            Categories = await _context.Categories
                .AsNoTracking()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .OrderBy(c => c.Description)
                .ToListAsync(cancellationToken)
        };
    }
}
