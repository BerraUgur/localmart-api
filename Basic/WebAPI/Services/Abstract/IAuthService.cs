using WebAPI.ModelViews;
using WebAPI.Security;

namespace WebAPI.Services.Abstract;

public interface IAuthService
{
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task Register(RegisterRequest registerRequest);
    Task<LoginResponse> Login(LoginRequest loginRequest);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
    Task MakeUserSellerAsync(int userId);
    Task MakeUserNormalAsync(int userId);
    Task UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest);
    Task<List<User>> GetUserListAsync();
    Task<User> GetUserByIdAsync(int userId);
    Task DeleteUserAsync(int id);
}