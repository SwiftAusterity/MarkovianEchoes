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
        private FileLogger _logger;

        public UserManager<ApplicationUser> _userManager { get; set; }

        [BindProperty]
        public IEnumerable<string> ChannelNames { get; set; }

        [BindProperty]
        public string SelectedLogContent { get; set; }

        [BindProperty]
        public string SelectedLog { get; set; }

        public LogsModel(UserManager<ApplicationUser> userManager, FileLogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public void OnGet(string selectedLog = "")
        {
            ChannelNames = _logger.GetCurrentLogNames();

            if (!String.IsNullOrWhiteSpace(selectedLog))
                SelectedLogContent = _logger.GetCurrentLogContent(selectedLog);
        }

        public void OnPost()
        {
            ChannelNames = _logger.GetCurrentLogNames();

            var message = String.Empty;

            if (!String.IsNullOrWhiteSpace(SelectedLog))
            {
                if (!_logger.RolloverLog(SelectedLog))
                    message = "Error rolling over log.";
                else
                {
                    _logger.LogAdminCommandUsage("*WEB* - RolloverLog[" + SelectedLog + "]", _userManager.GetUserName(User));
                    message = "Rollover Successful.";
                }
            }
            else
                message = "No log selected to rollover";
        }
    }
}