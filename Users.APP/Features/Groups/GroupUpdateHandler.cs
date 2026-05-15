using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }
    }

    public class GroupUpdateHandler : Service<Group>, IRequestHandler<GroupUpdateRequest, CommandResponse>
    {
        public GroupUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GroupUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(groupEntity => groupEntity.Id != request.Id && groupEntity.Title == request.Title.Trim(), cancellationToken))
                return Error("Group with the same title exists!");
            var existingEntity = await DbSet().SingleOrDefaultAsync(groupEntity => groupEntity.Id == request.Id, cancellationToken);
            if (existingEntity is null)
                return Error("Group not found!");
            existingEntity.Title = request.Title.Trim();
            await UpdateAsync(existingEntity, cancellationToken);
            return Success("Group updated successfully.", existingEntity.Id);
        }
    }
}
