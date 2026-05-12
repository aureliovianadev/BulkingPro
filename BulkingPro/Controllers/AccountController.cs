using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BulkingPro.Models;
using BulkingPro.ViewModels;

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
                    return RedirectToAction("Index", "Personal");

                return RedirectToAction("Index", "Home");
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

        // GET: /Account/Configuracoes
        [Authorize]
        public async Task<IActionResult> Configuracoes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var vm = new ConfiguracoesViewModel
            {
                NomeCompleto = user.NomeCompleto,
                Email        = user.Email ?? ""
            };

            return View(vm);
        }

        // POST: /Account/AtualizarNome
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarNome(ConfiguracoesViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(vm.NomeCompleto) || string.IsNullOrWhiteSpace(vm.Email))
            {
                TempData["Erro"] = "Nome e e-mail são obrigatórios.";
                return RedirectToAction("Configuracoes");
            }

            user.NomeCompleto      = vm.NomeCompleto;
            user.Email             = vm.Email;
            user.UserName          = vm.Email;
            user.NormalizedEmail   = vm.Email.ToUpper();
            user.NormalizedUserName = vm.Email.ToUpper();
            user.DataAtualizacao   = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Sucesso"] = "Dados atualizados com sucesso!";
            }
            else
            {
                TempData["Erro"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Configuracoes");
        }

        // POST: /Account/AlterarSenha
        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(ConfiguracoesViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(vm.SenhaAtual) || string.IsNullOrWhiteSpace(vm.NovaSenha))
            {
                TempData["Erro"] = "Preencha a senha atual e a nova senha.";
                return RedirectToAction("Configuracoes");
            }

            if (vm.NovaSenha != vm.ConfirmarSenha)
            {
                TempData["Erro"] = "As senhas não coincidem.";
                return RedirectToAction("Configuracoes");
            }

            var result = await _userManager.ChangePasswordAsync(user, vm.SenhaAtual, vm.NovaSenha);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Sucesso"] = "Senha alterada com sucesso!";
            }
            else
            {
                TempData["Erro"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Configuracoes");
        }
    }
}

