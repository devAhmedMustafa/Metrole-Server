using Google.Apis.Auth;
using Metrole.Data;
using Metrole.Modules.Auth.Dtos;
using Metrole.Modules.Auth.Google;
using Microsoft.EntityFrameworkCore;

namespace Metrole.Modules.Auth;

public class AuthService
{
    private readonly MetroleDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly JWTService _jwtService;

    public AuthService(MetroleDbContext db, IConfiguration configuration, JWTService jwtService)
    {
        _db = db;
        _configuration = configuration;
        _jwtService = jwtService;

    }

    public async Task<UserLoginResponse> GoogleLogin(GoogleLoginRequest request)
    {
        try
        {
            var clientId = _configuration["Google:OAuth:ClientId"];
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            if (payload == null)
            {
                throw new BadHttpRequestException("Invalid Google ID token");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == payload.Email);
            if (user == null)
            {
                user = new User
                {
                    Username = payload.Email,
                    Password = Guid.NewGuid().ToString()
                };
                
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var assignToken = _jwtService.GenerateJWT(user.Id.ToString());

            return new UserLoginResponse {
                UserId = user.Id,
                Token = assignToken
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Google login failed", ex);
        }
    }

}