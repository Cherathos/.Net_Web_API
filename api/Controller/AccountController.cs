using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;



[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<UserInfo> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(UserManager<UserInfo> userManager, 
    RoleManager<IdentityRole> roleManager, 
    IConfiguration configuration, 
    ITokenService tokenService, 
    ApplicationDbContext context,
    ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [EnableRateLimiting("login")]
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return BadRequest("User already exists");

        var emailExists = await _userManager.FindByEmailAsync(model.Email);
        if (emailExists != null)
            return BadRequest("Email already in use");

        var user = new UserInfo
        {
            UserName = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "User registered successfully" });
    }

 [EnableRateLimiting("login")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto model,
        UserManager<UserInfo> userManager,
        ApplicationDbContext context)
    {
        var user = await userManager.FindByNameAsync(model.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var jwt = await _tokenService.GenerateJwtTokenAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = jwt,
            refreshToken = refreshToken.Token
        });
    }


    [EnableRateLimiting("refresh")]
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
    {
        var storedToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

        if (storedToken == null)
            return Unauthorized("Invalid refresh token");

        if (storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
            return Unauthorized("Invalid refresh token");

        storedToken.IsRevoked = true;

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = storedToken.UserId,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var newJwt = await _tokenService.GenerateJwtTokenAsync(storedToken.User);

        return Ok(new
        {
            accessToken = newJwt,
            refreshToken = newRefreshToken.Token
        });
    }
    
    [EnableRateLimiting("refresh")]
    [HttpPost("add-role")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Refresh")]
    public async Task<IActionResult> AddRole([FromBody] string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {Role} created successfully", role);

                return Ok(new { message = "Role added successfully" });
            }

            return BadRequest(result.Errors);
        }

        return BadRequest("Role already exists");
    }

    [EnableRateLimiting("refresh")]
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Refresh")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return BadRequest("User not found");

        if (!await _roleManager.RoleExistsAsync(model.Role))
            return BadRequest("Role does not exist");

        if (await _userManager.IsInRoleAsync(user, model.Role))
            return BadRequest("User already has this role");

        var result = await _userManager.AddToRoleAsync(user, model.Role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        _logger.LogInformation("Role {Role} assigned to user {Username}", model.Role, model.Username);

        return Ok(new { message = "Role assigned successfully" });
    }

    [EnableRateLimiting("refresh")]
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequestDto model,
        [FromServices] ApplicationDbContext context)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

        if (token == null)
            return Ok();

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user ID {UserId}", token.UserId);

        return Ok(new { message = "Logged out successfully" });
    }
}