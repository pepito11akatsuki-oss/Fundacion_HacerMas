using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FundacionHacerMasWeb.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombres")]
        public string Nombres { get; set; }

        [Column("apellidos")]
        public string Apellidos { get; set; }

        [Column("correo")]
        public string Correo { get; set; } // Aquí guardamos "jose1234"

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        // Navegación para traer el nombre del rol automáticamente
        // El signo de interrogación evita que C# bloquee el guardado
        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }
    }
}
