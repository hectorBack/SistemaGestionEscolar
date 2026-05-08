using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.Validations;

namespace EscolarApi.DTOs
{
    public class AlumnoRequest
    {
        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "La matrícula debe tener exactamente 10 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "La matrícula solo puede contener letras y números.")]
        public string Matricula { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression("^(Masculino|Femenino|Otro)$", ErrorMessage = "El género debe ser 'Masculino', 'Femenino' u 'Otro'.")]
        public string Genero { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [MinMaxAge(3, 120, ErrorMessage = "La edad debe estar entre 3 y 120 años.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = null!;
    }
}