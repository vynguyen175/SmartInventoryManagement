using Microsoft.AspNetCore.Mvc;

namespace SmartInventoryManagement.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult NotFoundPage()
        {
            return View("404"); // ✅ Ensure it matches the actual view filename
        }

        [Route("Error/500")]
        public IActionResult InternalServerError()
        {
            return View("500"); // ✅ Matches your 500.cshtml file
        }

        [Route("Error/Custom")]
        public IActionResult CustomError()
        {
            return View("CustomError"); // ✅ Matches CustomError.cshtml
        }
    }
}