using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auditor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditor.Server.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<UsuarioSistema> _signInManager;

        public LogoutModel(SignInManager<UsuarioSistema> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            return Redirect("~/");
        }
    }
}
