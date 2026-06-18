namespace MultiPurposeServer.Models.Auth;

public class LoginResponse
{
    public string? Token { get; set; }
    public DateTime Expires { get; set; }
}
