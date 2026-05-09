using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BulkingPro.Models;

namespace BulkingPro.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Se o usuário já estiver logado, vai pro dashboard do admin
        // Caso contrário, redireciona para o login
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Admin");
        }
        return RedirectToAction("Login", "Account");
    }

    // Adiciona este método para resolver o erro 404
    public IActionResult Login()
    {
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}