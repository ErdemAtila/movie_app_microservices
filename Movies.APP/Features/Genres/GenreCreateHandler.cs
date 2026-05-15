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
    public class GenreCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    public class GenreCreateHandler : Service<Genre>, IRequestHandler<GenreCreateRequest, CommandResponse>
    {
        public GenreCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GenreCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(g => g.Name == request.Name.Trim(), cancellationToken))
                return Error("Genre already exists!");

            var entity = new Genre
            {
                Name = request.Name.Trim()
            };

            await CreateAsync(entity, cancellationToken);
            return Success("Genre created successfully.", entity.Id);
        }
    }
}
