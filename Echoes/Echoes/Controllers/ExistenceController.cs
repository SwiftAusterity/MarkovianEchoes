using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echoes.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class ExistenceController : Controller
    {
        [HttpGet("", Name = "Existence_Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{place}", Name = "Existence_Place")]
        public IActionResult Place(string place)
        {
            return View();
        }

        [HttpPost("{place}", Name = "Existence_Place_Input")]
        public IActionResult Input(string place, string input)
        {
            return View();
        }
    }
}