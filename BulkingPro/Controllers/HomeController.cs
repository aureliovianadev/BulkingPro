using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BulkingPro.Models;

namespace BulkingPro.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Redireciona diretamente para a página de login
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