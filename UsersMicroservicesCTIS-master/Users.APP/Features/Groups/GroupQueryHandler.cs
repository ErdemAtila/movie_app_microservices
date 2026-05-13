using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupQueryRequest : Request, IRequest<IQueryable<GroupQueryResponse>>
    {
    }

    public class GroupQueryResponse : Response
    {
        public string Title { get; set; }
    }

    public class GroupQueryHandler : Service<Group>, IRequestHandler<GroupQueryRequest, IQueryable<GroupQueryResponse>>
    {
        public GroupQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<GroupQueryResponse>> Handle(GroupQueryRequest request, CancellationToken cancellationToken)
        {
            // select Id, Title from Groups
            var query = DbSet().Select(groupEntity => new GroupQueryResponse
            {
                Id = groupEntity.Id,
                Title = groupEntity.Title
            });
            return Task.FromResult(query);
        }
    }
}
