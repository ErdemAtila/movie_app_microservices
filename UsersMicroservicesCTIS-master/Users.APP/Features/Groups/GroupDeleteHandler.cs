using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class GroupDeleteHandler : Service<Group>, IRequestHandler<GroupDeleteRequest, CommandResponse>
    {
        public GroupDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Group> DbSet()
        {
            return base.DbSet().Include(g => g.Users);
        }

        public async Task<CommandResponse> Handle(GroupDeleteRequest request, CancellationToken cancellationToken)
        {
            var existingEntity = await DbSet().SingleOrDefaultAsync(groupEntity => groupEntity.Id == request.Id, cancellationToken);
            if (existingEntity is null)
                return Error("Group not found!");

            // check if any users exist for the group
            if (existingEntity.Users.Any())
                return Error("Group can't be deleted because it has relational users!");

            await DeleteAsync(existingEntity, cancellationToken);
            return Success("Group deleted successfully.", existingEntity.Id);
        }
    }
}
