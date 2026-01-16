
namespace Its.PleaseProtect.Api.Authentications
{
    public interface IAuthenticationRepo
    {
        public User? Authenticate(string orgId, string user, string password, HttpRequest request);
    }
}
