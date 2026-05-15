using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

using System.ComponentModel.DataAnnotations;

namespace Movies.APP.Features.Movies;

public class MovieUpdateRequest : Request, IRequest<CommandResponse>
{
    [Required, StringLength(150)]
    public string Name { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public decimal TotalRevenue { get; set; }

    public int DirectorId { get; set; }

    public List<int> GenreIds { get; set; }
}

public class MovieUpdateHandler : Service<Movie>, IRequestHandler<MovieUpdateRequest, CommandResponse>
{
    public MovieUpdateHandler(DbContext db) : base(db)
    {
    }

    public async Task<CommandResponse> Handle(MovieUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await DbSet().AnyAsync(m => m.Id != request.Id && m.Name == request.Name.Trim(), cancellationToken))
            return Error("Movie with the same name already exists!");

        var entity = await DbSet().Include(m => m.MovieGenres).SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (entity is null)
            return Error("Movie not found!");

        if (!await DbSet<Director>().AnyAsync(d => d.Id == request.DirectorId, cancellationToken))
            return Error("Director not found!");

        if (request.GenreIds != null && request.GenreIds.Any())
        {
            var existingGenreIds = await DbSet<Genre>().Where(g => request.GenreIds.Contains(g.Id)).Select(g => g.Id).ToListAsync(cancellationToken);
            var missingGenreIds = request.GenreIds.Except(existingGenreIds).ToList();
            if (missingGenreIds.Any())
                return Error("One or more of the provided genres were not found!");
        }

        Delete(entity.MovieGenres);
        entity.Name = request.Name?.Trim();
        entity.ReleaseDate = request.ReleaseDate;
        entity.TotalRevenue = request.TotalRevenue;
        entity.DirectorId = request.DirectorId;
        entity.GenreIds = request.GenreIds;

        await UpdateAsync(entity, cancellationToken);
        return Success("Movie updated successfully.", entity.Id);
    }
}
