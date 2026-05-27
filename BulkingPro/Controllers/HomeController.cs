using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BulkingPro.Models;

namespace BulkingPro.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Se não estiver logado, vai para o login
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Account");

        // Redireciona baseado na role
        if (User.IsInRole("Administrador"))
            return RedirectToAction("Index", "Admin");

        if (User.IsInRole("Moderador"))
            return RedirectToAction("Index", "Personal");

        if (User.IsInRole("Usuario"))
            return RedirectToAction("Index", "Aluno");  // <-- ALTERADO!

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}