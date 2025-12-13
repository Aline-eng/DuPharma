using Microsoft.AspNetCore.Mvc;
using DuPharma.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DuPharma.Controllers.Api;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthApiController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        if (userId == 0)
            return Unauthorized();

        return Ok(new
        {
            userId,
            fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            branchId = User.FindFirst("BranchId")?.Value,
            branchName = User.FindFirst("BranchName")?.Value
        });
    }
}
