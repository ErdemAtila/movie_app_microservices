using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;

namespace Movies.APP.Features.Movies
{
    public class MovieCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public decimal TotalRevenue { get; set; }

        [Required]
        public int DirectorId { get; set; }

        public List<int> GenreIds { get; set; } = new List<int>();
    }

    public class MovieCreateHandler : Service<Movie>, IRequestHandler<MovieCreateRequest, CommandResponse>
    {
        public MovieCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(MovieCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(m => m.Name == request.Name && m.DirectorId == request.DirectorId, cancellationToken))
                return Error("Movie with the same name and director already exists!");

            if (!await DbSet<Director>().AnyAsync(d => d.Id == request.DirectorId, cancellationToken))
                return Error("Director not found!");

            if (request.GenreIds != null && request.GenreIds.Any())
            {
                var existingGenreIds = await DbSet<Genre>().Where(g => request.GenreIds.Contains(g.Id)).Select(g => g.Id).ToListAsync(cancellationToken);
                var missingGenreIds = request.GenreIds.Except(existingGenreIds).ToList();
                if (missingGenreIds.Any())
                    return Error("One or more of the provided genres were not found!");
            }

            var entity = new Movie
            {
                Name = request.Name.Trim(),
                ReleaseDate = request.ReleaseDate,
                TotalRevenue = request.TotalRevenue,
                DirectorId = request.DirectorId,
                MovieGenres = request.GenreIds.Distinct().Select(id => new MovieGenre { GenreId = id }).ToList()
            };

            await CreateAsync(entity, cancellationToken);
            return Success("Movie created successfully.", entity.Id);
        }
    }
}
