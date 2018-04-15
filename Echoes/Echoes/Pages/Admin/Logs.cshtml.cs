using System;
using System.Collections.Generic;
using Cottontail.FileSystem.Logging;
using Echoes.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Echoes.Web.Pages.Logs
{
    public class LogsModel : PageModel
    {
        public UserManager<ApplicationUser> _userManager { get; set; }

        [BindProperty]
        public IEnumerable<string> ChannelNames { get; set; }

        [BindProperty]
        public string SelectedLogContent { get; set; }

        [BindProperty]
        public string SelectedLog { get; set; }

        public LogsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public void OnGet()
        {
            ChannelNames = LoggingUtility.GetCurrentLogNames();

            if (!String.IsNullOrWhiteSpace(SelectedLog))
                SelectedLogContent = LoggingUtility.GetCurrentLogContent(SelectedLog);
        }

        public void OnPost()
        {
            var message = String.Empty;

            if (!String.IsNullOrWhiteSpace(SelectedLog))
            {
                if (!LoggingUtility.RolloverLog(SelectedLog))
                    message = "Error rolling over log.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RolloverLog[" + SelectedLog + "]", _userManager.GetUserName(User));
                    message = "Rollover Successful.";
                }
            }
            else
                message = "No log selected to rollover";
        }
    }
}