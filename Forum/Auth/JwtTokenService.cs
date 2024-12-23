using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Forum.Auth
{
    public class JwtTokenService
    {
        private readonly SymmetricSecurityKey _authSigninKey;
        private readonly string? _issuer;
        private readonly string? _audience;

        public JwtTokenService(IConfiguration configuration)
        {
            _authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
            _issuer = configuration["JWT:ValidIssuer"];
            _audience = configuration["JWT:ValidAudience"];
        }

        public string CreateAccessToken(string userName, string userId, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
            };

            authClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                expires: DateTime.Now.AddMinutes(20),
                claims: authClaims,
                signingCredentials: new SigningCredentials(_authSigninKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefreshToken(Guid sessionId, string userId, DateTime expires)
        {
            var authClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, userId),
                new("SessionId", sessionId.ToString())
            };


            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                expires: expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(_authSigninKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler() { MapInboundClaims = false };
                var validationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = _authSigninKey,
                    ValidateLifetime = true
                };

                claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out var _);
                return true;
            }
            catch
            {
                claims = null;
                return false;
            }
        }
    }
}
