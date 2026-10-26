using System;
using System.ComponentModel.DataAnnotations;

namespace FundacionHacerMasWeb.Models
{
    public class RegistroCitaViewModel
    {
        // Datos Personales del Paciente
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El contacto de emergencia es obligatorio.")]
        public string ContactoEmergencia { get; set; }

        [Required(ErrorMessage = "El teléfono de emergencia es obligatorio.")]
        public string TelefonoEmergencia { get; set; }

        // Datos de la Cita
        [Required(ErrorMessage = "Debe seleccionar un médico.")]
        public int IdMedico { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "El motivo de consulta es obligatorio.")]
        public string MotivoConsulta { get; set; }
    }
}
