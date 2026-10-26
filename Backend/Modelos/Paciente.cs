using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FundacionHacerMasWeb.Models
{
    [Table("Pacientes")]
    public class Paciente
    {
        [Key]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        // Nuevos campos agregados
        [Column("fecha_nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Column("contacto_emergencia")]
        public string ContactoEmergencia { get; set; }

        [Column("telefono_emergencia")]
        public string TelefonoEmergencia { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
