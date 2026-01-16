namespace Its.PleaseProtect.Api.Services;

public class IdpResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
}