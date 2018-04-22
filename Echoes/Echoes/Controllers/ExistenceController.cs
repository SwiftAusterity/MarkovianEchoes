using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echoes.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class ExistenceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}