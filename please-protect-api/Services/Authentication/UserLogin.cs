namespace Its.PleaseProtect.Api.Services
{
    public class UserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserLogin()
        {
            UserName = "";
            Password = "";
        }
    }
}
