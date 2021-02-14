using System;
using System.ComponentModel.DataAnnotations;
using Auditor.Models.EnumModels;
using Microsoft.AspNetCore.Identity;

namespace Auditor.Models
{
    public class UsuarioSistema : IdentityUser
    {
        [Required] public string Nombres { get; set; }
        [Required] public string Apellidos { get; set; }
        public DateTime FechaNacimiento { get; set; } = DateTime.Now;
        [Required] public TipoIdentificacion TipoIdentificacion { get; set; }
        [Required] public string NumeroIdentificacion { get; set; }
        public string TelefonoMovil { get; set; }
        public string Foto { get; set; }
        [Required] public EstadoRegistro Estado { get; set; }
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string ActualizadoPor { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}