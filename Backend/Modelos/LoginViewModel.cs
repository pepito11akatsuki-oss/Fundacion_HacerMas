using System.ComponentModel.DataAnnotations;

namespace FundacionHacerMasWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El ID de acceso es obligatorio.")]
        [Display(Name = "Identificador de Acceso")]
        [RegularExpression(@"^[EPep]-\d+$", ErrorMessage = "El formato debe ser E-000001 (Empleado) o P-000001 (Paciente).")]
        public string IdAcceso { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
