using auth.DTOs.Auth;
using auth.Repositories;
using Auth;

namespace auth.Services;

public class AuthService
{
    private readonly AuthRepository _authRepository;
    private readonly ServiceToken _serviceToken;

    public AuthService(
        AuthRepository authRepository,
        ServiceToken serviceToken)
    {
        _authRepository = authRepository;
        _serviceToken = serviceToken;
    }

    public async Task<DtoUserResponse?> GetUserById(string userId)
    {
        return await _authRepository.FindUserById(userId);
    }

    public async Task<string> CreateToken(string email, string password)
    {
        DtoUserResponse loginUser = await _authRepository.CheckUserAsync(email, password);

        return _serviceToken.GenerateToken(
            loginUser.email,
            loginUser.Id,
            loginUser.firstName,
            loginUser.lastName);
    }

    public async Task<DtoUserResponse?> CreateUserAsync(DtoCreateUser request)
    {
        return await _authRepository.CreateUserAsync(request);
    }
}
