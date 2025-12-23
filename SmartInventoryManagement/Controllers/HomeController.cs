using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagement.Models;
using System.Diagnostics;
using SmartInventoryManagement.Models;

namespace SmartInventoryManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Simulate a bug (force a null reference exception)
        /* string nullString = null;
        int length = nullString.Length; // Will throw a NullReferenceException */
        return View();
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