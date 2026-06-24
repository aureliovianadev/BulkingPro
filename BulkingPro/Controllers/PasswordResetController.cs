using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using BulkingPro.Services;
using BulkingPro.ViewModels;

namespace BulkingPro.Controllers;

public class PasswordResetController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordResetController> _logger;

    public PasswordResetController(
        ApplicationDbContext context,
        UserManager<Usuario> userManager,
        IEmailService emailService,
        ILogger<PasswordResetController> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    // GET: /PasswordReset/ForgotPassword
    [HttpGet("PasswordReset/ForgotPassword")]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /PasswordReset/ForgotPassword
    [HttpPost("PasswordReset/ForgotPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            TempData["Sucesso"] = "Se o e-mail estiver cadastrado, enviaremos um código de recuperação.";
            
            if (user == null)
            {
                _logger.LogWarning($"Tentativa de recuperação para e-mail não cadastrado: {model.Email}");
                return RedirectToAction(nameof(VerifyCode), new { email = model.Email });
            }

            var existingToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => 
                    t.UserId == user.Id && 
                    !t.Used && 
                    t.ExpiresAt > DateTime.Now);

            if (existingToken != null)
            {
                _context.PasswordResetTokens.Remove(existingToken);
                await _context.SaveChangesAsync();
            }

            var code = new Random().Next(100000, 999999).ToString();

            var token = new PasswordResetToken
            {
                UserId = user.Id,
                Email = user.Email ?? model.Email,
                Code = code,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                Used = false,
                CreatedAt = DateTime.Now
            };

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email ?? model.Email,
                user.NomeCompleto,
                code
            );

            if (!emailSent)
            {
                TempData["Erro"] = "Erro ao enviar e-mail. Tente novamente mais tarde.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            _logger.LogInformation($"Código de recuperação enviado para {user.Email}");
            return RedirectToAction(nameof(VerifyCode), new { email = model.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro no processo de recuperação para {model.Email}");
            TempData["Erro"] = "Ocorreu um erro. Tente novamente.";
            return View(model);
        }
    }

    // GET: /PasswordReset/VerifyCode
    [HttpGet("PasswordReset/VerifyCode")]
    public IActionResult VerifyCode(string email)
    {
        var model = new VerifyCodeViewModel
        {
            Email = email ?? ""
        };
        return View(model);
    }

    // POST: /PasswordReset/VerifyCode
    [HttpPost("PasswordReset/VerifyCode")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var token = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => 
                    t.Email == model.Email &&
                    t.Code == model.Code &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.Now);

            if (token == null)
            {
                var expiredToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => 
                        t.Email == model.Email &&
                        t.Code == model.Code &&
                        !t.Used &&
                        t.ExpiresAt <= DateTime.Now);

                if (expiredToken != null)
                {
                    TempData["Erro"] = "Código expirado. Solicite um novo código.";
                    return RedirectToAction(nameof(ForgotPassword));
                }

                TempData["Erro"] = "Código inválido.";
                return View(model);
            }

            TempData["Sucesso"] = "Código verificado com sucesso!";
            return RedirectToAction(nameof(ResetPassword), new { 
                email = model.Email, 
                code = model.Code 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar código");
            TempData["Erro"] = "Ocorreu um erro. Tente novamente.";
            return View(model);
        }
    }

    // GET: /PasswordReset/ResetPassword
    [HttpGet("PasswordReset/ResetPassword")]
    public IActionResult ResetPassword(string email, string code)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Code = code
        };

        return View(model);
    }

    // POST: /PasswordReset/ResetPassword
    [HttpPost("PasswordReset/ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var token = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => 
                    t.Email == model.Email &&
                    t.Code == model.Code &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.Now);

            if (token == null)
            {
                TempData["Erro"] = "Código inválido ou expirado.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var user = token.User;

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            token.Used = true;
            token.ExpiresAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Senha alterada com sucesso para {user.Email}");

            TempData["Sucesso"] = "Senha alterada com sucesso! Faça login com sua nova senha.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao resetar senha para {model.Email}");
            TempData["Erro"] = "Ocorreu um erro. Tente novamente.";
            return View(model);
        }
    }

    // POST: /PasswordReset/ResendCode
    [HttpPost("PasswordReset/ResendCode")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode([FromBody] ResendCodeViewModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Json(new { success = false, message = "E-mail não encontrado" });
            }

            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.Used)
                .ToListAsync();
            _context.PasswordResetTokens.RemoveRange(existingTokens);
            await _context.SaveChangesAsync();

            var code = new Random().Next(100000, 999999).ToString();

            var token = new PasswordResetToken
            {
                UserId = user.Id,
                Email = user.Email ?? model.Email,
                Code = code,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                Used = false,
                CreatedAt = DateTime.Now
            };

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email ?? model.Email,
                user.NomeCompleto,
                code
            );

            if (!emailSent)
            {
                return Json(new { success = false, message = "Erro ao enviar e-mail" });
            }

            return Json(new { success = true, message = "Novo código enviado!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar código");
            return Json(new { success = false, message = "Erro ao reenviar código" });
        }
    }
}