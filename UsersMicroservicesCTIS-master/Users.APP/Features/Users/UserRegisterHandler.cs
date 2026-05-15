using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserRegisterRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(30)]
        public string UserName { get; set; }

        [Required, StringLength(15)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }
    }

    public class UserRegisterHandler : Service<User>, IRequestHandler<UserRegisterRequest, CommandResponse>
    {
        private readonly DbContext _db;

        public UserRegisterHandler(DbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(UserRegisterRequest request, CancellationToken cancellationToken)
        {
            // Check for duplicate active username
            if (await DbSet().AnyAsync(u => u.UserName == request.UserName.Trim() && u.IsActive, cancellationToken))
                return Error("An active user with this username already exists.");

            // Find the "User" role seeded in Program.cs
            var userRole = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
            if (userRole is null)
                return Error("The 'User' role has not been configured. Please contact an administrator.");

            var entity = new User
            {
                UserName         = request.UserName.Trim(),
                Password         = request.Password,
                FirstName        = request.FirstName?.Trim(),
                LastName         = request.LastName?.Trim(),
                IsActive         = true,
                RegistrationDate = DateTime.Now,
                RoleIds          = new List<int> { userRole.Id }
            };

            await CreateAsync(entity, cancellationToken);
            return Success("Registration successful. You can now log in.", entity.Id);
        }
    }
}
