using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;
using System.Linq;

namespace Movies.APP.Features.Genres
{
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
}
