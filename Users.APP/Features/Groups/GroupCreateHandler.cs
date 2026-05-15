using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }
    }

    public class GroupCreateHandler : Service<Group>, IRequestHandler<GroupCreateRequest, CommandResponse>
    {
        public GroupCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GroupCreateRequest request, CancellationToken cancellationToken)
        {
            // Way 1:
            //var existingEntity = await DbSet().SingleOrDefaultAsync(groupEntity => groupEntity.Title == request.Title.Trim(), cancellationToken);
            //if (existingEntity is not null)
            //    return Error("Group with the same title exists!");
            // Way 2:
            if (await DbSet().AnyAsync(groupEntity => groupEntity.Title == request.Title.Trim(), cancellationToken))
                return Error("Group with the same title exists!");
            var entity = new Group()
            {
                Title = request.Title.Trim()
            };
            await CreateAsync(entity, cancellationToken);
            return Success("Group created successfully.", entity.Id);
        }
    }
}
