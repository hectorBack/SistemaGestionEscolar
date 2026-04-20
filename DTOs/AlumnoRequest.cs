using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs
{
    public class AlumnoRequest
    {
        [Required(ErrorMessage = "La matrícula es obligatoria")]
        public string Matricula { get; set; } = null!;

        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Apellido { get; set; } = null!;

        public DateTime FechaNacimiento { get; set; }

        [EmailAddress]
        public string Email { get; set; } = null!;

        [MinLength(8)]
        public string Password { get; set; } = null!; // Necesario para crear su cuenta de Usuario
    }
}