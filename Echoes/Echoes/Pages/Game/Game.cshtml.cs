﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Echoes.Web.Pages.Game
{
    [AllowAnonymous]
    public class GameModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}