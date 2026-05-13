using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    public class DirectorQueryRequest : Request, IRequest<IQueryable<DirectorQueryResponse>>
    {
    }

    public class DirectorQueryResponse : Response
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsRetired { get; set; }

        public string FullName { get; set; }
        public string IsRetiredF { get; set; }
    }

    public class DirectorQueryHandler : Service<Director>, IRequestHandler<DirectorQueryRequest, IQueryable<DirectorQueryResponse>>
    {
        public DirectorQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Director> DbSet()
        {
            return base.DbSet().OrderBy(d => d.LastName).ThenBy(d => d.FirstName);
        }

        public async Task<IQueryable<DirectorQueryResponse>> Handle(DirectorQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(d => new DirectorQueryResponse
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                IsRetired = d.IsRetired,
                FullName = d.FirstName + " " + d.LastName,
                IsRetiredF = d.IsRetired ? "Retired" : "Active"
            });

            return await Task.FromResult(query);
        }
    }
}
