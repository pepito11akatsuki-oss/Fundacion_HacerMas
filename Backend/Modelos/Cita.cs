using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FundacionHacerMasWeb.Models
{
    [Table("Citas")]
    public class Cita
    {
        [Key]
        [Column("id_cita")]
        public int IdCita { get; set; }

        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Column("id_medico")]
        public int IdMedico { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("motivo_consulta")]
        public string MotivoConsulta { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        [Column("intentos_reagendamiento")]
        public int IntentosReagendamiento { get; set; }

        // --- LOS PUENTES DE CONEXIÓN QUE FALTABAN ---
        [ForeignKey("IdPaciente")]
        public virtual Paciente? Paciente { get; set; }

        [ForeignKey("IdMedico")]
        public virtual Medico? Medico { get; set; }
        // --------------------------------------------
    }
}
