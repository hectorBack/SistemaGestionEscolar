using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.Validations;

namespace EscolarApi.DTOs.Request
{
    public class DocenteRequest
    {
        [Required(ErrorMessage = "El número de empleado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El número de empleado no puede exceder los 20 caracteres.")]
        public string NumeroEmpleado { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string Apellido { get; set; } = null!;

        [StringLength(100, ErrorMessage = "La especialidad no puede exceder los 100 caracteres.")]
        public string? Especialidad { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es obligatoria.")]
        [PastDate] // <--- Aquí aplicamos nuestra nueva validación
        public DateTime FechaContratacion { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = null!;

        // El password es obligatorio al crear, pero podrías hacerlo opcional 
        // si usas este mismo DTO para editar (Update)
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = null!;
    }
}