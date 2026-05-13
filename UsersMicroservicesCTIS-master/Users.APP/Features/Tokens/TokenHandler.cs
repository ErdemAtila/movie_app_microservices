using CORE.APP.Models.Authentication;
using CORE.APP.Services;
using CORE.APP.Services.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Tokens
{
    public class TokenRequest : TokenRequestBase, IRequest<TokenResponse>
    {
    }

    public class TokenHandler : Service<User>, IRequestHandler<TokenRequest, TokenResponse>
    {
        private readonly ITokenAuthService _tokenAuthService; 

        public TokenHandler(DbContext db, ITokenAuthService tokenAuthService) : base(db)
        {
            _tokenAuthService = tokenAuthService;
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet().Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
        }

        public async Task<TokenResponse> Handle(TokenRequest request, CancellationToken cancellationToken)
        {
            var user = await DbSet().SingleOrDefaultAsync(u => u.UserName == request.UserName && u.Password == request.Password 
                && u.IsActive, cancellationToken);

            if (user is null)
                return null;

            user.RefreshToken = _tokenAuthService.GetRefreshToken();
            user.RefreshTokenExpiration = DateTime.Now.AddDays(7); 
            await UpdateAsync(user, cancellationToken);

            var expiration = DateTime.Now.AddMinutes(5); 
            return _tokenAuthService.GetTokenResponse(user.Id, user.UserName, user.UserRoles.Select(ur => ur.Role.Name).ToArray(), 
                expiration, request.SecurityKey, request.Issuer, request.Audience, user.RefreshToken);
        }
    }
}
