using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CORE.APP.Models.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CORE.APP.Services.Authentication;

public class TokenAuthService : AuthServiceBase, ITokenAuthService
{
    public IEnumerable<Claim> GetClaims(string token, string securityKey)
    {
        token = token.StartsWith(JwtBearerDefaults.AuthenticationScheme) ?
            token.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length + 1) : token;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        return securityToken is null ? null : principal.Claims;
    }

    public string GetRefreshToken()
    {
        var bytes = new byte[32];
        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    public TokenResponse GetTokenResponse(int userId, string userName, string[] userRoleNames, DateTime expiration, string securityKey, string issuer, string audience, string refreshToken)
    {
        var claims = GetClaims(userId, userName, userRoleNames);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(issuer, audience, claims, DateTime.Now, expiration, signingCredentials);
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var jwt = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        return new TokenResponse
        {
            Token = JwtBearerDefaults.AuthenticationScheme + " " + jwt,
            RefreshToken = refreshToken
        };
    } 
}
