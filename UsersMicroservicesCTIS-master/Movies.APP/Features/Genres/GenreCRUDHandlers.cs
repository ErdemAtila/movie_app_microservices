using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;
using System.Linq;

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

    public class GenreQueryRequest : Request, IRequest<IQueryable<GenreQueryResponse>>
    {
    }

    public class GenreQueryResponse : Response
    {
        public string Name { get; set; }
    }

    public class GenreQueryHandler : Service<Genre>, IRequestHandler<GenreQueryRequest, IQueryable<GenreQueryResponse>>
    {
        public GenreQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Genre> DbSet()
        {
            return base.DbSet().OrderBy(g => g.Name);
        }

        public async Task<IQueryable<GenreQueryResponse>> Handle(GenreQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(g => new GenreQueryResponse
            {
                Id = g.Id,
                Name = g.Name
            });

            return await Task.FromResult(query);
        }
    }

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
