using Forum.Auth.Model;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Transactions;

namespace Forum.Auth
{
    public static class AuthEndpoints
    {
        public static void AddAuthApi(this WebApplication app)
        {
            // register
            app.MapPost("api/accounts", async (UserManager<ForumUser> userManager, RegisterUserDTO dto) => 
            {
                //check existing
                var user = await userManager.FindByNameAsync(dto.UserName);
                if (user != null) 
                {
                    return Results.UnprocessableEntity("Username already taken");
                }

                var newUser = new ForumUser()
                {
                    Email = dto.Email,
                    UserName = dto.UserName,
                };

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var createdUserResult = await userManager.CreateAsync(newUser, dto.Password);
                    if (!createdUserResult.Succeeded)
                    {
                        return Results.UnprocessableEntity();
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(newUser, ForumRoles.ForumUser);
                    if (!addToRoleResult.Succeeded)
                    {
                        // Rollback transaction if adding to role fails
                        return Results.UnprocessableEntity("Failed to add role");
                    }

                    transaction.Complete();
                }

                return Results.Created();
            });

            // login
            app.MapPost("api/login", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService , HttpContext httpContext , LoginDTO dto) =>
            {
                //check existing
                var user = await userManager.FindByNameAsync(dto.UserName);
                if (user == null)
                {
                    return Results.UnprocessableEntity("User does not exist");
                }

                var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
                if (!isPasswordValid)
                {
                    return Results.UnprocessableEntity("Username or password was incorrect");
                }

                var roles = await userManager.GetRolesAsync(user);

                var sessionId = Guid.NewGuid();
                var expiresAt = DateTime.UtcNow.AddDays(1);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var refreshToken = jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

                await sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expiresAt,
                    // Secure = false // shoul be true??
                };

                httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

                return Results.Ok(new SuccessfullLoginDTO(accessToken));
            });

            app.MapPost("api/accessToken", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService, HttpContext httpContext) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    return Results.UnprocessableEntity();
                }

                if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionId = claims.FindFirstValue("SessionId");
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionIdAsGuid = Guid.Parse(sessionId);
                if (!await sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
                {
                    return Results.UnprocessableEntity();
                }

                var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.UnprocessableEntity();
                }

                var roles = await userManager.GetRolesAsync(user);

                var expiresAt = DateTime.UtcNow.AddDays(1);
                var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
                var newRefreshToken = jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expiresAt,
                    // Secure = false // shoul be true??
                };

                httpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

                await sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

                return Results.Ok(new SuccessfullLoginDTO(accessToken));
            });

            // logout
            app.MapPost("api/logout", async (UserManager<ForumUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService, HttpContext httpContext) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                {
                    return Results.UnprocessableEntity();
                }

                if (!jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
                {
                    return Results.UnprocessableEntity();
                }

                var sessionId = claims.FindFirstValue("SessionId");
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Results.UnprocessableEntity();
                }

                await sessionService.InvalidateSessionAsync(Guid.Parse(sessionId));
                httpContext.Response.Cookies.Delete("RefreshToken");

                return Results.Ok();
            });
        }

        public record RegisterUserDTO(string UserName, string Email, string Password);  
        public record LoginDTO(string UserName, string Password);
        public record SuccessfullLoginDTO(string AccessToken);

    }
}
