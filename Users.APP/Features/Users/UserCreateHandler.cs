using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserCreateRequest : Request, IRequest<CommandResponse>
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

    public class UserCreateHandler : Service<User>, IRequestHandler<UserCreateRequest, CommandResponse>
    {
        public UserCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(UserCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.UserName == request.UserName.Trim() && u.IsActive, cancellationToken))
                return Error("Active user with the same user name exists!");

            var entity = new User
            {
                GroupId = request.GroupId,
                Gender = request.Gender,
                FirstName = request.FirstName?.Trim(),
                Address = request.Address?.Trim(),
                BirthDate = request.BirthDate,
                CityId = request.CityId,
                CountryId = request.CountryId,
                IsActive = request.IsActive,
                LastName = request.LastName?.Trim(),
                Password = request.Password,
                RegistrationDate = request.RegistrationDate,
                RoleIds = request.RoleIds,
                Score = request.Score,
                UserName = request.UserName?.Trim()
            };
            await CreateAsync(entity, cancellationToken);
            return Success("User created successfully.", entity.Id);
        }
    }
}
