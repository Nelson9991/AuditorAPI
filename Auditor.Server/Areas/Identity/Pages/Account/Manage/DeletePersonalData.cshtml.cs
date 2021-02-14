using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Auditor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Auditor.Server.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<UsuarioSistema> _userManager;
        private readonly SignInManager<UsuarioSistema> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<UsuarioSistema> userManager,
            SignInManager<UsuarioSistema> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID'{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Se produjo un error inesperado al eliminar el usuario con ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("Usuario con ID '{UserId}' se borraron.", userId);

            return Redirect("~/");
        }
    }
}
