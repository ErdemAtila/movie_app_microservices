using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Users;

namespace Users.APP.Features.Roles
{
    public class RoleQueryRequest : Request, IRequest<IQueryable<RoleQueryResponse>>
    {
    }

    public class RoleQueryResponse : Response
    {
        public string Name { get; set; }

        public int UserCount { get; set; }

        public string UsersF { get; set; }

        public List<UserQueryResponse> Users { get; set; }
    }

    public class RoleQueryHandler : Service<Role>, IRequestHandler<RoleQueryRequest, IQueryable<RoleQueryResponse>>
    {
        public RoleQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet().Include(r => r.UserRoles).ThenInclude(ur => ur.User).OrderBy(r => r.Name);
        }

        public Task<IQueryable<RoleQueryResponse>> Handle(RoleQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(r => new RoleQueryResponse()
            {
                // entity properties
                Id = r.Id,
                Name = r.Name,

                // custom properties
                // Way 1: 
                UsersF = string.Join(", ", r.UserRoles.Select(ur => ur.User.UserName)),
                UserCount = r.UserRoles.Count,
                // Way 2: 
                Users = r.UserRoles.Select(ur => new UserQueryResponse
                {
                    // entity properties
                    Id = ur.User.Id,
                    UserName = ur.User.UserName,
                    Password = ur.User.Password,
                    IsActive = ur.User.IsActive,
                    RegistrationDate = ur.User.RegistrationDate,
                    BirthDate = ur.User.BirthDate,
                    Score = ur.User.Score,
                    FirstName = ur.User.FirstName,
                    LastName = ur.User.LastName,
                    Gender = ur.User.Gender,
                    Address = ur.User.Address,
                    GroupId = ur.User.GroupId,
                    RoleIds = ur.User.RoleIds,
                    CountryId = ur.User.CountryId,
                    CityId = ur.User.CityId,

                    // custom properties
                    FullName = ur.User.FirstName + " " + ur.User.LastName,
                    IsActiveF = ur.User.IsActive ? "Active" : "Inactive",
                    BirthDateF = ur.User.BirthDate.HasValue ? ur.User.BirthDate.Value.ToString("MM/dd/yyyy") : string.Empty,
                    RegistrationDateF = ur.User.RegistrationDate.ToShortDateString(),
                    ScoreF = ur.User.Score.ToString("N1"),
                    GenderF = ur.User.Gender.ToString()
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}
