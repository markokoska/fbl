using System.ComponentModel.DataAnnotations;

namespace FBL.Api.DTOs;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3)] string DisplayName,
    [Required, MinLength(6)] string Password
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    string UserId,
    string DisplayName,
    string Email,
    IList<string> Roles
);

public record RefreshTokenDto(
    [Required] string Token,
    [Required] string RefreshToken
);
