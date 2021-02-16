using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Auditor.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Auditor.Models;
using Auditor.Models.EnumModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Auditor.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UsuarioSistema> _signInManager;
        private readonly UserManager<UsuarioSistema> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<UsuarioSistema> userManager,
            SignInManager<UsuarioSistema> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [EmailAddress(ErrorMessage = "Ingrese un email válido")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "El campo {0} es requerido")]
            [StringLength(100, ErrorMessage = "La {0} debe tener un minimo de {2} y un maximo de {1} caractéres",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmación Contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no son iguales")]
            public string ConfirmPassword { get; set; }

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

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new UsuarioSistema
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Nombres = Input.Nombres,
                    Apellidos = Input.Apellidos,
                    TipoIdentificacion = Input.TipoIdentificacion,
                    NumeroIdentificacion = Input.NumeroIdentificacion,
                    FechaActualizacion = Input.FechaNacimiento,
                    TelefonoMovil = Input.TelefonoMovil,
                    PhoneNumber = Input.TelefonoFijo,
                    FechaCreacion = DateTime.Now,
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(Sd.UsuarioSistemaRol))
                    {
                        _roleManager.CreateAsync(new IdentityRole(Sd.UsuarioSistemaRol)).GetAwaiter().GetResult();
                        _roleManager.CreateAsync(new IdentityRole(Sd.UsuarioEmpleadoEmpresaRol)).GetAwaiter()
                            .GetResult();
                        _roleManager.CreateAsync(new IdentityRole(Sd.UsuarioEmpresaRol)).GetAwaiter().GetResult();
                    }

                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.Nombres));

                    await _userManager.AddToRoleAsync(user, Sd.UsuarioSistemaRol);

                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirma tu email",
                        $"Por favor confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click aqui</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
