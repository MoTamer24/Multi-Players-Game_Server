using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}