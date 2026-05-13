using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Movies.APP.Domain;
using Movies.APP.Features.Directors;
using Movies.APP.Features.Genres;

namespace Movies.APP.Features.Movies
{
    public class MovieQueryRequest : Request, IRequest<IQueryable<MovieQueryResponse>>
    {
    }

    public class MovieQueryResponse : Response
    {
        // Entity data
        public string Name { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int DirectorId { get; set; }
        public List<int> GenreIds { get; set; }

        // Custom/Formatted data
        public string ReleaseDateF { get; set; }
        public string TotalRevenueF { get; set; }
        public string DirectorF { get; set; }
        public List<string> GenresF { get; set; }

        // Nested data (Ways 1 and 2)
        public DirectorQueryResponse Director { get; set; }
        public List<GenreQueryResponse> Genres { get; set; }
    }

    public class MovieQueryHandler : Service<Movie>, IRequestHandler<MovieQueryRequest, IQueryable<MovieQueryResponse>>
    {
        public MovieQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Movie> DbSet()
        {
            return base.DbSet()
                .Include(m => m.Director)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .OrderBy(m => m.Name);
        }

        public async Task<IQueryable<MovieQueryResponse>> Handle(MovieQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(m => new MovieQueryResponse
            {
                Id = m.Id,
                Name = m.Name,
                ReleaseDate = m.ReleaseDate,
                TotalRevenue = m.TotalRevenue,
                DirectorId = m.DirectorId,
                GenreIds = m.GenreIds,

                ReleaseDateF = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.ToString("MM/dd/yyyy") : string.Empty,
                TotalRevenueF = m.TotalRevenue.ToString("C2"), // Currency format
                
                // Way 1
                DirectorF = m.Director.FirstName + " " + m.Director.LastName,
                GenresF = m.MovieGenres.Select(mg => mg.Genre.Name).ToList(),

                // Way 2
                Director = new DirectorQueryResponse 
                { 
                    Id = m.Director.Id, 
                    FirstName = m.Director.FirstName, 
                    LastName = m.Director.LastName 
                },
                Genres = m.MovieGenres.Select(mg => new GenreQueryResponse 
                { 
                    Id = mg.Genre.Id, 
                    Name = mg.Genre.Name 
                }).ToList()
            });

            return await Task.FromResult(query);
        }
    }
}
