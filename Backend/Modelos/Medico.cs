using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FundacionHacerMasWeb.Models
{
    [Table("Medicos")]
    public class Medico
    {
        [Key]
        [Column("id_medico")]
        public int IdMedico { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("especialidad")]
        public string Especialidad { get; set; }

        [Column("consultorio")]
        public string Consultorio { get; set; }

        // --- ESTOS SON LOS CAMPOS QUE FALTABAN ---
        [Column("hora_entrada")]
        public TimeSpan HoraEntrada { get; set; }

        [Column("hora_salida")]
        public TimeSpan HoraSalida { get; set; }
        // -----------------------------------------
        [Column("cedula_profesional")]
        public string CedulaProfesional { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
