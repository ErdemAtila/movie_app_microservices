using CORE.APP.Domain;

namespace Users.APP.Domain
{
    public class UserRole : Entity
    {
        public int UserId { get; set; }
        public User User { get; set; } // navigation property
        public int RoleId { get; set; }
        public Role Role { get; set; } // navigation property
    }
}
