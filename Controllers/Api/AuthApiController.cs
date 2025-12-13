using Microsoft.AspNetCore.Mvc;
using DuPharma.Services;

namespace DuPharma.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthApiController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.AuthenticateAsync(request.Email, request.Password);
        
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.UserId,
                user.FullName,
                user.Email,
                role = _authService.GetRoleName(user.Role),
                user.BranchId,
                branchName = user.Branch?.BranchName
            }
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
