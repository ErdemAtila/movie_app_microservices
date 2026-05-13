using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.APP.Domain
{
    public class User : Entity
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

        public Group Group { get; set; } // navgiation property

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>(); // navigation property

        [NotMapped]
        public List<int> RoleIds
        {
            get => UserRoles.Select(userRoleEntity => userRoleEntity.RoleId).ToList();
            set => UserRoles = value.Select(roleIdValue => new UserRole() { RoleId = roleIdValue }).ToList();
        }

        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
