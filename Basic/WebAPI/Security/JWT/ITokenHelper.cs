namespace WebAPI.Security;

public interface ITokenHelper
{
    AccessToken CreateToken(User user, IList<OperationClaim> operationClaims);
    RefreshToken CreateRefreshToken(User user, string ipAddress);
}