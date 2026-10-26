using System;
using System.ComponentModel.DataAnnotations;

namespace FundacionHacerMasWeb.Models
{
    public class UsuarioViewModel
    {
        public string? CedulaProfesional { get; set; }
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El usuario/correo es obligatorio.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]        public string PasswordHash { get; set; }

        public bool Activo { get; set; }

        // Campos Dinámicos (Exclusivos para Médicos)
        public string? Consultorio { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
    }
}
