using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    public class DirectorCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        public bool IsRetired { get; set; } = false;
    }

    public class DirectorCreateHandler : Service<Director>, IRequestHandler<DirectorCreateRequest, CommandResponse>
    {
        public DirectorCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(DirectorCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(d => d.FirstName == request.FirstName && d.LastName == request.LastName, cancellationToken))
                return Error("Director already exists!");

            var entity = new Director
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                IsRetired = request.IsRetired
            };

            await CreateAsync(entity, cancellationToken);
            return Success("Director created successfully.", entity.Id);
        }
    }
}
