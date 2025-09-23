namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.SignUpFeature;

public class SignUpRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

