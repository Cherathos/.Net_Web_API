using Microsoft.AspNetCore.Identity;

public class UserInfo : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
