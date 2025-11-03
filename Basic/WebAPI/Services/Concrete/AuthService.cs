using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.ModelViews;
using WebAPI.Security;
using WebAPI.Security.Enums;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class AuthService : IAuthService
{
    private readonly ApplicationDBContext _applicationDBContext;
    private readonly ITokenHelper _tokenHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDBContext applicationDBContext,
        ITokenHelper tokenHelper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _applicationDBContext = applicationDBContext ?? throw new ArgumentNullException(nameof(applicationDBContext));
        _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        if (loginRequest == null) throw new ArgumentNullException(nameof(loginRequest));
        try
        {
            var user = await _applicationDBContext.Users
                .FirstOrDefaultAsync(user => user.Email == loginRequest.Email || user.Username == loginRequest.Email);

            if (user == null)
            {
                _logger.LogWarning($"User not found for email/username: {loginRequest.Email}");
                throw new KeyNotFoundException("User not found.");
            }

            var result = HashingHelper
                .VerifyPasswordHash(loginRequest.Password, user.PasswordHash, user.PasswordSalt);

            if (!result)
            {
                _logger.LogWarning($"Wrong credentials for user: {loginRequest.Email}");
                throw new BadHttpRequestException("Wrong credentials.");
            }

            AccessToken accessToken = await CreateAccessToken(user);

            //CREATE AND SAVE REFRESH TOKEN
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            var refreshToken = _tokenHelper.CreateRefreshToken(user, ipAddress);
            await _applicationDBContext.RefreshTokens.AddAsync(refreshToken);
            await _applicationDBContext.SaveChangesAsync();

            LoginResponse loginResponse = new()
            {
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                Expiration = accessToken.Expiration
            };

            return loginResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login.");
            throw new ApplicationException("An error occurred during login.", ex);
        }
    }

    public async Task UpdateUserAsync(int userId, UpdateUserRequest updateUserRequest)
    {
        var user = await _applicationDBContext.Users.FindAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        user.FirstName = updateUserRequest.FirstName;
        user.LastName = updateUserRequest.LastName;
        user.Username = updateUserRequest.Username;
        user.Email = updateUserRequest.Email;
        user.PhoneNumber = updateUserRequest.PhoneNumber;
        user.Status = updateUserRequest.Status;
        user.Role = (Role)updateUserRequest.Role;
        await _applicationDBContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _applicationDBContext.Users.FindAsync(id);
        if (user is null) return;
        _applicationDBContext.Users.Remove(user);
        await _applicationDBContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetUserListAsync()
    {
        return await _applicationDBContext.Users.ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        var user = await _applicationDBContext.Users.Include(x => x.Addresses).FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            throw new KeyNotFoundException("User not found");
        return user;
    }

    public async Task Register(RegisterRequest registerRequest)
    {
        var user = await _applicationDBContext.Users
            .FirstOrDefaultAsync(user => user.Username.ToLower() == registerRequest.Username.ToLower()
                || user.Email.ToLower() == registerRequest.Email.ToLower());

        if (user is not null) throw new BadHttpRequestException("Username or email is already in use");

        HashingHelper.CreatePasswordHash(registerRequest.Password,
            out byte[] passwordHash, out byte[] passwordSalt);

        var userToRegister = new User()
        {
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            Username = registerRequest.Username,
            PhoneNumber = registerRequest.PhoneNumber,
            PasswordSalt = passwordSalt,
            PasswordHash = passwordHash,
            Role = Role.Seller
        };
        await _applicationDBContext.Users.AddAsync(userToRegister);
        await _applicationDBContext.SaveChangesAsync();
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        var refreshToken = await _applicationDBContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == refreshTokenRequest.Token)
            ?? throw new BadHttpRequestException("Refresh Token does not exist");

        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        if (refreshToken.Revoked is not null)
        {
            await RevokeDescendantRefreshTokens(refreshToken, ipAddress, $"Attempted reuse of revoked ancestor token: {refreshToken.Token}");
            throw new BadHttpRequestException("This refresh token is revoked because attempted reuse of revoked ancestor");
        }

        if (DateTime.UtcNow > refreshToken.Expires)
            throw new BadHttpRequestException("Refresh token is not active");

        var user = await _applicationDBContext.Users
            .FirstOrDefaultAsync(user => user.Id == refreshToken.UserId);
        if (user is null)
            throw new BadHttpRequestException("User not found for refresh token");
        var newRefreshToken = await RotateRefreshToken(user, refreshToken, ipAddress);
        await _applicationDBContext.RefreshTokens.AddAsync(newRefreshToken);
        await _applicationDBContext.SaveChangesAsync();

        var accessToken = await CreateAccessToken(user);

        LoginResponse loginResponse = new()
        {
            AccessToken = accessToken.Token,
            RefreshToken = newRefreshToken.Token,
            Expiration = accessToken.Expiration
        };

        return loginResponse;
    }

    public async Task MakeUserSellerAsync(int userId)
    {
        var user = await _applicationDBContext.Users.FindAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        user.Role = Role.Seller;
        await _applicationDBContext.SaveChangesAsync();
    }

    public async Task MakeUserNormalAsync(int userId)
    {
        var user = await _applicationDBContext.Users.FindAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        user.Role = Role.User;
        await _applicationDBContext.SaveChangesAsync();
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenEntry = await _applicationDBContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token && t.Email == request.Email);
        if (tokenEntry == null || tokenEntry.IsUsed || tokenEntry.ExpirationDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset failed. Token invalid or expired: {Token}", request.Token);
            return false;
        }

        var user = await _applicationDBContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Email);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed. User not found: {Email}", request.Email);
            return false;
        }

        HashingHelper.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        tokenEntry.IsUsed = true;
        await _applicationDBContext.SaveChangesAsync();
        return true;
    }

    private async Task<AccessToken> CreateAccessToken(User user)
    {
        var operationClaims = await _applicationDBContext.UserOperationClaims
                    .Where(userOperationClaim => userOperationClaim.UserId == user.Id)
                    .Include(userOperationClaim => userOperationClaim.OperationClaim)
                    .Select(userOperationClaim => userOperationClaim.OperationClaim)
                    .ToListAsync();

        var trimmedOperationClaims = operationClaims.Select(op =>
        {
            op.Name = op.Name.Trim();
            return op;
        }).ToList();

        var accessToken = _tokenHelper.CreateToken(user, trimmedOperationClaims);
        return accessToken;
    }

    private async Task RevokeDescendantRefreshTokens(RefreshToken refreshToken, string ipAddress, string reason)
    {
        if (refreshToken == null || refreshToken.ReplacedByToken is null) return;

        var childToken = await _applicationDBContext.RefreshTokens
            .FirstOrDefaultAsync(refreshTokenQuery => refreshTokenQuery.Token == refreshToken.ReplacedByToken);

        if (childToken is not null && childToken.Revoked is null)
            await RevokeRefreshToken(childToken, ipAddress, reason, null);
        else if (childToken != null)
            await RevokeDescendantRefreshTokens(childToken, ipAddress, reason);
    }

    private async Task RevokeRefreshToken(RefreshToken refreshToken, string ipAddress, string? reason, string? replacedByToken)
    {
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = reason;
        refreshToken.ReplacedByToken = replacedByToken;
        await _applicationDBContext.SaveChangesAsync();
    }

    private async Task<RefreshToken> RotateRefreshToken(User user, RefreshToken refreshToken, string ipAddress)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null when rotating refresh token");
        var newRefreshToken = _tokenHelper.CreateRefreshToken(user, ipAddress);
        newRefreshToken.Created = DateTime.UtcNow;

        await RevokeRefreshToken(refreshToken,
            ipAddress,
            "New refresh token requested",
            newRefreshToken.Token
        );

        return newRefreshToken;
    }
}