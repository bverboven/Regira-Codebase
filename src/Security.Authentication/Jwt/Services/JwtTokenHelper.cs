using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Jwt.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Regira.Security.Authentication.Jwt.Services;

public class JwtTokenHelper : ITokenHelper
{
    private readonly string _secret;
    private readonly string? _algorithm;
    private readonly string? _authority;
    private readonly string? _defaultAudience;
    private readonly int _defaultLifeSpan;
    private readonly bool _includeIssuedDate;
    private readonly ICollection<string>? _validAudiences;
    public JwtTokenHelper(JwtTokenOptions options)
    {
        _secret = options.Secret;
        _algorithm = options.Algorithm ?? SecurityAlgorithms.HmacSha512Signature;
        _authority = options.Authority;
        _defaultAudience = options.Audience;
        _validAudiences = options.Audiences;
        _defaultLifeSpan = options.LifeSpan;
        _includeIssuedDate = options.IncludeIssuedDate;
    }


    public string Create(IEnumerable<Claim> claims, string? audience = null, int? lifeSpan = null)
    {
        var secretKey = Encoding.ASCII.GetBytes(_secret);
        var symmetricKey = new SymmetricSecurityKey(secretKey);

        lifeSpan ??= _defaultLifeSpan;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _authority,
            Audience = audience ?? _defaultAudience,
            Expires = lifeSpan.Value > 0 ? DateTime.UtcNow.AddSeconds(lifeSpan.Value) : null,
            SigningCredentials = new SigningCredentials(symmetricKey, _algorithm),
            IssuedAt = _includeIssuedDate ? DateTime.UtcNow : null
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    public async Task<bool> Validate(string token)
    {
        var secretKey = Encoding.ASCII.GetBytes(_secret);
        var symmetricKey = new SymmetricSecurityKey(secretKey);

        var tokenHandler = new JsonWebTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            IssuerSigningKey = symmetricKey,
            ValidIssuer = _authority,
            ValidAudiences = _validAudiences ?? (_defaultAudience != null ? new[] { _defaultAudience } : null)
        };
        var validateResult = await tokenHandler.ValidateTokenAsync(token, validationParams);

        return validateResult.IsValid;
    }
}