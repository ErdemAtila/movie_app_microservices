using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;

namespace Movies.APP.Features.Genres
{
    public class GenreDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class GenreDeleteHandler : Service<Genre>, IRequestHandler<GenreDeleteRequest, CommandResponse>
    {
        public GenreDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Genre> DbSet()
        {
            return base.DbSet().Include(g => g.MovieGenres);
        }

        public async Task<CommandResponse> Handle(GenreDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");

            Delete(entity.MovieGenres);
            await DeleteAsync(entity, cancellationToken);
            return Success("Genre deleted successfully.", entity.Id);
        }
    }
}
