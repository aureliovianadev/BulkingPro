using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BulkingPro.Models;

namespace BulkingPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;

        public AccountController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Preencha e-mail e senha.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                userName: email,
                password: senha,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (await _userManager.IsInRoleAsync(user!, "Administrador"))
                    return RedirectToAction("Index", "Admin");

                if (await _userManager.IsInRoleAsync(user!, "Moderador"))
                    return RedirectToAction("Index", "Home"); // trocar pela área do personal quando existir

                return RedirectToAction("Index", "Home"); // área do aluno
            }

            ViewBag.Erro = "E-mail ou senha incorretos.";
            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}