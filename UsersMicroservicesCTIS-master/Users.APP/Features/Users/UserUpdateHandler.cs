using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(30)]
        public string UserName { get; set; }

        [Required, StringLength(15)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public Genders Gender { get; set; } // (int)user.Gender: 1, user.Gender.ToString(): "Woman" 

        public DateTime? BirthDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public decimal Score { get; set; } // float, double

        public bool IsActive { get; set; }

        public string Address { get; set; }

        public int? CountryId { get; set; } // null

        public int? CityId { get; set; } // null

        public int? GroupId { get; set; } // 0 to M relationship

        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserUpdateHandler : Service<User>, IRequestHandler<UserUpdateRequest, CommandResponse>
    {
        public UserUpdateHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet().Include(u => u.UserRoles);
        }

        public async Task<CommandResponse> Handle(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.Id != request.Id && u.UserName == request.UserName.Trim() && u.IsActive, cancellationToken))
                return Error("Active user with the same user name exists!");

            var entity = await DbSet().SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("User not found!");

            // delete relational roles through UserRoles
            Delete(entity.UserRoles);

            entity.RoleIds = request.RoleIds; // assign new role ids
            entity.GroupId = request.GroupId;
            entity.Gender = request.Gender;
            entity.FirstName = request.FirstName.Trim();
            entity.Address = request.Address.Trim();
            entity.BirthDate = request.BirthDate;
            entity.CityId = request.CityId;
            entity.CountryId = request.CountryId;
            entity.IsActive = request.IsActive;
            entity.LastName = request.LastName.Trim();
            entity.Password = request.Password;
            entity.RegistrationDate = request.RegistrationDate;
            entity.Score = request.Score;
            entity.UserName = request.UserName.Trim();

            await UpdateAsync(entity, cancellationToken);
            return Success("User updated successfully.", entity.Id);
        }
    }
}
