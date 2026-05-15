using Microsoft.EntityFrameworkCore;

namespace Users.APP.Domain
{
    public class UsersDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public UsersDb(DbContextOptions options) : base(options)
        {
        }
    }
}
