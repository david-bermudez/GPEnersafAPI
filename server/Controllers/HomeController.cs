using System;
using Microsoft.AspNetCore.Mvc;

namespace GpEnerSaf.Controllers
{
    public partial class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
