namespace Its.PleaseProtect.Api.Services;

public class RefreshTokenData
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime ExpireAt { get; set; }
}