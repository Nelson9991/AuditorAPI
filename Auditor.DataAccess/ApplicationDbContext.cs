using Auditor.Models;
using Auditor.Models.EnumModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Auditor.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region UsuarioSistema

            builder.Entity<UsuarioSistema>()
                .HasIndex(x => x.NumeroIdentificacion)
                .IsUnique();

            builder.Entity<UsuarioSistema>()
                .Property(x => x.TipoIdentificacion)
                .HasConversion<string>();

            builder.Entity<UsuarioSistema>()
                .Property(x => x.Estado)
                .HasConversion<string>()
                .HasDefaultValue(EstadoRegistro.Activo);

            #endregion

            base.OnModelCreating(builder);
        }

        public DbSet<UsuarioSistema> UsuarioSistema { get; set; }
    }
}