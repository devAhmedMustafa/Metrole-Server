using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Metrole.Modules.Auth;

public class JWTService(IConfiguration c)
{
    private readonly IConfiguration _config = c;

    public string GenerateJWT(string userId, string? role = null)
    {
        if (string.IsNullOrEmpty(_config["Jwt:Key"]) ||
            string.IsNullOrEmpty(_config["Jwt:Issuer"]) ||
            string.IsNullOrEmpty(_config["Jwt:Audience"]))
        {
            throw new Exception("JWT configuration is missing");
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireHours = _config["Jwt:ExpireHours"];

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [
                new Claim(ClaimTypes.NameIdentifier, userId),
                role != null ? new Claim(ClaimTypes.Role, role) : null
            ],
            expires: expireHours == "Infinite" ? DateTime.MaxValue :
             DateTime.Now.AddHours(double.Parse(expireHours ?? "1")),
            notBefore: DateTime.Now,
            signingCredentials: cred
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }
}