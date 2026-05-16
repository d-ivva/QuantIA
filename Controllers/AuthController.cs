using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantIA.DTOs;
using QuantIA.Services;

namespace QuantIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly KeycloakAdminService _admin;

    public AuthController(KeycloakAdminService admin)
    {
        _admin = admin;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Nome é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "E-mail é obrigatório." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Senha é obrigatória." });

        if (dto.Password != dto.ConfirmPassword)
            return BadRequest(new { message = "As senhas não coincidem." });

        if (dto.Password.Length < 8)
            return BadRequest(new { message = "A senha deve ter no mínimo 8 caracteres." });

        try
        {
            await _admin.CreateUserAsync(dto.Name, dto.Email, dto.Password);
            return Ok(new { message = "Conta criada com sucesso." });
        }
        catch (ApplicationException ex)
        {
            var msg = TranslateKeycloakError(ex.Message);
            return BadRequest(new { message = msg });
        }
        catch
        {
            return StatusCode(500, new { message = "Erro ao criar conta. Tente novamente." });
        }
    }

    private static string TranslateKeycloakError(string raw)
    {
        var m = raw.ToLowerInvariant();
        if (m.Contains("user exists with same username") ||
            m.Contains("user exists with same email")    ||
            m.Contains("already exists"))
            return "Este e-mail já está cadastrado.";
        if (m.Contains("password policy"))
            return "A senha não atende aos requisitos de segurança. Use ao menos 8 caracteres.";
        if (m.Contains("invalid email"))
            return "E-mail inválido.";
        return "Erro ao criar conta. Tente novamente.";
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "E-mail é obrigatório." });

        try
        {
            // Always return success to prevent user enumeration
            var userId = await _admin.FindUserIdByEmailAsync(dto.Email);
            if (userId != null)
                await _admin.SendPasswordResetEmailAsync(userId);
        }
        catch
        {
            // Silently fail — user still sees success message
        }

        return Ok(new { message = "Se o e-mail existir, você receberá as instruções de recuperação." });
    }
}
