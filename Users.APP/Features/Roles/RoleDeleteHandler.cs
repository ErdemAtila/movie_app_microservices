using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    public class RoleDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class RoleDeleteHandler : Service<Role>, IRequestHandler<RoleDeleteRequest, CommandResponse>
    {
        public RoleDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet().Include(r => r.UserRoles);
        }

        public async Task<CommandResponse> Handle(RoleDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Role not found!");

            // delete relational users through UserRoles
            Delete(entity.UserRoles);

            await DeleteAsync(entity, cancellationToken);
            return Success("Role deleted successfully.", entity.Id);
        }
    }
}
