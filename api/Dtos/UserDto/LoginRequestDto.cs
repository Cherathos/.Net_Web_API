public class LoginRequestDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RefreshTokenRequestDto
{
    public required string RefreshToken { get; set; } = null!;
}