using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Auditor.Models;
using Auditor.Models.EnumModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditor.Server.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<UsuarioSistema> _userManager;
        private readonly SignInManager<UsuarioSistema> _signInManager;

        public IndexModel(
            UserManager<UsuarioSistema> userManager,
            SignInManager<UsuarioSistema> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Nombres { get; set; }

            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Apellidos { get; set; }

            [Required(ErrorMessage = "El campo {0} es requerido")]
            [Display(Name = "Fecha de Nacimiento")]
            public DateTime FechaNacimiento { get; set; } = DateTime.Now;

            [Required(ErrorMessage = "El campo {0} es requerido")]
            [Display(Name = "Tipo de Documento")]
            public TipoIdentificacion TipoIdentificacion { get; set; }

            [Required(ErrorMessage = "El campo {0} es requerido")]
            [Display(Name = "Número de Documento")]
            public string NumeroIdentificacion { get; set; }

            [Display(Name = "Teléfono Celular")]
            [RegularExpression(@"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$",
                ErrorMessage = "Ingrese un teléfono válido")]
            public string TelefonoMovil { get; set; }


            [Display(Name = "Teléfono Fijo")]
            [RegularExpression(@"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$",
                ErrorMessage = "Ingrese un teléfono válido")]
            public string TelefonoFijo { get; set; }
        }

        private async Task LoadAsync(UsuarioSistema user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel
            {
                TelefonoFijo = user.PhoneNumber,
                TipoIdentificacion = user.TipoIdentificacion,
                FechaNacimiento = user.FechaNacimiento,
                Apellidos = user.Apellidos,
                Nombres = user.Nombres,
                TelefonoMovil = user.TelefonoMovil,
                NumeroIdentificacion = user.NumeroIdentificacion
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

            if (_userManager.GetClaimsAsync(user).GetAwaiter().GetResult()
                .FirstOrDefault(x => x.Type == ClaimTypes.GivenName).Value != Input.Nombres)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.GivenName, user.Nombres)
                };

                await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.GivenName, user.Nombres));
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, Input.Nombres));
            }

            user.Nombres = Input.Nombres;
            user.Apellidos = Input.Apellidos;
            user.FechaNacimiento = Input.FechaNacimiento;
            user.TelefonoMovil = Input.TelefonoMovil;
            user.PhoneNumber = Input.TelefonoFijo;
            user.TipoIdentificacion = Input.TipoIdentificacion;
            user.NumeroIdentificacion = Input.NumeroIdentificacion;

            await _userManager.UpdateAsync(user);


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Tu perfil fue actualizado";
            return RedirectToPage();
        }
    }
}
