using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors;

public class DirectorDeleteRequest : Request, IRequest<CommandResponse>
{
}

public class DirectorDeleteHandler : Service<Director>, IRequestHandler<DirectorDeleteRequest, CommandResponse>
{
    public DirectorDeleteHandler(DbContext db) : base(db)
    {
    }
    
    protected override IQueryable<Director> DbSet()
    {
        return base.DbSet().Include(d => d.Movies);
    }

    public async Task<CommandResponse> Handle(DirectorDeleteRequest request, CancellationToken cancellationToken)
    {
        var entity = await DbSet().SingleOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (entity is null)
            return Error("Director not found!");
        if (entity.Movies.Any())
            return Error("Director can't be deleted because it has relational movies!");
        await DeleteAsync(entity, cancellationToken);
        return Success("Director deleted successfully.", entity.Id);
    }
}
