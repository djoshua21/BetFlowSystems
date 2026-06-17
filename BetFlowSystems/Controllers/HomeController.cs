using BetFlowSystems.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BetFlowSystems.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            if (Request.Path.Equals("/home", StringComparison.OrdinalIgnoreCase)
                || Request.Path.Equals("/home/", StringComparison.OrdinalIgnoreCase)
                || Request.Path.Equals("/home/index", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/");
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
