using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CORE.APP.Models.Authentication;

public class RefreshTokenRequestBase
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string RefreshToken { get; set; }

    [JsonIgnore]
    public string SecurityKey { get; set; }

    [JsonIgnore]
    public string Issuer { get; set; }

    [JsonIgnore]
    public string Audience { get; set; }
}
