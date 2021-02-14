using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Auditor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Auditor.Server.Areas.Identity.Pages.Account.Manage
{
    public partial class EmailModel : PageModel
    {
        private readonly UserManager<UsuarioSistema> _userManager;
        private readonly SignInManager<UsuarioSistema> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<UsuarioSistema> userManager,
            SignInManager<UsuarioSistema> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Username { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Nuevo correo para inicio de sesión")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(UsuarioSistema user)
        {
            var email = await _userManager.GetUserNameAsync(user);

            Input = new InputModel
            {
                NewEmail = email,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            await _userManager.SetUserNameAsync(user, Input.NewEmail);
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Se actualizó su correo";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            return RedirectToPage();
        }
    }
}
