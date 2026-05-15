using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.APP.Domain
{
    public class Role : Entity
    {
        [Required, StringLength(25)]
        public string Name { get; set; }

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>(); // navigation property

        [NotMapped]
        public List<int> UserIds 
        {   
            get =>  UserRoles.Select(userRoleEntity => userRoleEntity.UserId).ToList(); 
            set => UserRoles = value.Select(userIdValue => new UserRole() { UserId = userIdValue }).ToList(); 
        }
    }
}
