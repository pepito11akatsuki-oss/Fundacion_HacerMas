using OpenXmlPowerTools;
using System.ComponentModel.DataAnnotations;

namespace FundacionHacerMasWeb.Models
{
    public class NotasClinicasViewModel
    {
        public int IdCita { get; set; }
        public string? IdPacienteExpediente { get; set; }
        public string? NombrePaciente { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? MotivoConsulta { get; set; }

        // --- SIGNOS VITALES ---
        [Display(Name = "Peso (kg)")]
        public string? Peso { get; set; }

        [Display(Name = "Talla (cm)")]
        public string? Talla { get; set; }

        [Display(Name = "Presión Arterial (mmHg)")]
        public string? PresionArterial { get; set; }

        [Display(Name = "Temperatura (°C)")]
        public string? Temperatura { get; set; }

        // --- EVALUACIÓN MÉDICA ---
        [Required(ErrorMessage = "Debe ingresar un diagnóstico.")]
        public string Diagnostico { get; set; }

        [Required(ErrorMessage = "Debe ingresar el tratamiento o receta.")]
        public string Tratamiento { get; set; }

        public string? ObservacionesAdicionales { get; set; }
    }
}
