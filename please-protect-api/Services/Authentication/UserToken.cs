namespace Its.PleaseProtect.Api.Services
{
    public class UserToken
    {
        public string Status { get; set; }
        public string Message { get; set; }

        public string UserName { get; set; }
        public KeycloakTokenResponse Token { get; set; }

        public UserToken()
        {
            Status = "Success";
            UserName = "";
            Message = "";
            Token = new KeycloakTokenResponse();
        }
    }
}
