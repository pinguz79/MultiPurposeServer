namespace MultiPurposeServer.Models.Auth;

public class ExternalGoogleCodeRequest
{
    public string? Code { get; set; }
    public string? RedirectUri { get; set; }
    public string? CodeVerifier { get; set; }
}
