using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;

namespace Movies.APP.Features.Genres
{
    public class GenreUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    public class GenreUpdateHandler : Service<Genre>, IRequestHandler<GenreUpdateRequest, CommandResponse>
    {
        public GenreUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GenreUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(g => g.Id != request.Id && g.Name == request.Name.Trim(), cancellationToken))
                return Error("Genre already exists!");

            var entity = await DbSet().SingleOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");

            entity.Name = request.Name.Trim();
            await UpdateAsync(entity, cancellationToken);
            return Success("Genre updated successfully.", entity.Id);
        }
    }
}
