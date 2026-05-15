using System.ComponentModel.DataAnnotations;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors;

public class DirectorUpdateRequest : Request, IRequest<CommandResponse>
{
    [Required, StringLength(150)]
    public string FirstName { get; set; }
    
    [Required, StringLength(150)]
    public string LastName { get; set; }
    
    public bool IsRetired { get; set; }
}

public class DirectorUpdateHandler : Service<Director>, IRequestHandler<DirectorUpdateRequest, CommandResponse>
{
    public DirectorUpdateHandler(DbContext db) : base(db)
    {
    }

    public async Task<CommandResponse> Handle(DirectorUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await DbSet().AnyAsync(d => d.Id != request.Id && d.FirstName == request.FirstName.Trim() && d.LastName == request.LastName.Trim(), cancellationToken))
            return Error("Director with the same name exists!");
        var entity = await DbSet().SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (entity is null)
            return Error("Director not found!");
        entity.FirstName = request.FirstName?.Trim();
        entity.LastName = request.LastName?.Trim();
        entity.IsRetired = request.IsRetired;
        await UpdateAsync(entity, cancellationToken);
        return Success("Director updated successfully.", entity.Id);
    }
}
