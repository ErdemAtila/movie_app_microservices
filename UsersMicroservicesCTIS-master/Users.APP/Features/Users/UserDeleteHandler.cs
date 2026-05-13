using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class UserDeleteHandler : Service<User>, IRequestHandler<UserDeleteRequest, CommandResponse>
    {
        public UserDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet().Include(u => u.UserRoles);
        }

        public async Task<CommandResponse> Handle(UserDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("User not found!");

            // delete relational roles through UserRoles
            Delete(entity.UserRoles);

            await DeleteAsync(entity, cancellationToken);
            return Success("User deleted successfully.", entity.Id);
        }
    }
}
