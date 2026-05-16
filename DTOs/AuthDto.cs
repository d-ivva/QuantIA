namespace QuantIA.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; } = null!;
}

public class RegisterDto
{
    public string Name            { get; set; } = null!;
    public string Email           { get; set; } = null!;
    public string Password        { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
