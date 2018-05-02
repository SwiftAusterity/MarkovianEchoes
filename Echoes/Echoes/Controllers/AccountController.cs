using Cottontail.FileSystem.Logging;
using Echoes.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Echoes.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly FileLogger _logger;

        public AccountController(SignInManager<ApplicationUser> signInManager, FileLogger logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.WriteToLog("User logged out.");
            return RedirectToPage("/Index");
        }
    }
}
