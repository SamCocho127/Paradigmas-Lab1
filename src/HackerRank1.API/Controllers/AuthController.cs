using HackerRank1.API.DTO;
using HackerRank1.Application;
using HackerRank1.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.API.Controllers;

public record TokenResponse(string token);

[ApiController]
public class AuthController : Controller
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthController(IAuthenticationService authenticationService, ITokenGenerator tokenGenerator, JwtSettings jwtSettings)
    {
        _authenticationService = authenticationService;
        _tokenGenerator = tokenGenerator;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var validUser = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
        if (validUser is null)
            return Unauthorized();

        var token = _tokenGenerator.Generate(validUser, _jwtSettings);

        return Ok(new TokenResponse(token));
    }
}
