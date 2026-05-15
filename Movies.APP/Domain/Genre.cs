using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Movies.APP.Domain
{
    public class Genre : Entity
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<MovieGenre> MovieGenres { get; set; }
    }
}
