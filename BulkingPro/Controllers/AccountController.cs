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
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Admin");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                TempData["ErroLogin"] = "Preencha e-mail e senha.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _signInManager.PasswordSignInAsync(email, senha, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                
                if (await _userManager.IsInRoleAsync(user, "Administrador"))
                    return RedirectToAction("Index", "Admin");
                
                return RedirectToAction("Index", "Home");
            }

            TempData["ErroLogin"] = "E-mail ou senha incorretos.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}