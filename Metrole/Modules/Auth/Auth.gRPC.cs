using Grpc.Core;

namespace Metrole.Modules.Auth;

public class AuthGrpcService : AuthGrpc.AuthGrpcBase
{
    private readonly AuthService _authService;

    public AuthGrpcService(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<LoginResponse> GoogleLogin(GoogleLoginRequest request, ServerCallContext context)
    {
        var result = await _authService.GoogleLogin(request);

        return new LoginResponse
        {
            UserId = result.UserId,
            Token = result.Token
        };
    }
}