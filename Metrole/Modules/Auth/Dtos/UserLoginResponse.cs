namespace Metrole.Modules.Auth.Dtos;

public class UserLoginResponse
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
}