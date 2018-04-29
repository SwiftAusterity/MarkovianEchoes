using System;
using System.Collections.Generic;
using Cottontail.FileSystem.Logging;
using Echoes.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Echoes.Web.Pages.Logs
{
    public class LogsModel : PageModel
    {
        private IHostingEnvironment _env;

        public UserManager<ApplicationUser> _userManager { get; set; }

        [BindProperty]
        public IEnumerable<string> ChannelNames { get; set; }

        [BindProperty]
        public string SelectedLogContent { get; set; }

        [BindProperty]
        public string SelectedLog { get; set; }

        public LogsModel(UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        public void OnGet(string selectedLog = "")
        {
            ChannelNames = LoggingUtility.GetCurrentLogNames(_env.ContentRootPath);

            if (!String.IsNullOrWhiteSpace(selectedLog))
                SelectedLogContent = LoggingUtility.GetCurrentLogContent(_env.ContentRootPath, selectedLog);
        }

        public void OnPost()
        {
            ChannelNames = LoggingUtility.GetCurrentLogNames(_env.ContentRootPath);

            var message = String.Empty;

            if (!String.IsNullOrWhiteSpace(SelectedLog))
            {
                if (!LoggingUtility.RolloverLog(_env.ContentRootPath, SelectedLog))
                    message = "Error rolling over log.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage(_env.ContentRootPath, "*WEB* - RolloverLog[" + SelectedLog + "]", _userManager.GetUserName(User));
                    message = "Rollover Successful.";
                }
            }
            else
                message = "No log selected to rollover";
        }
    }
}