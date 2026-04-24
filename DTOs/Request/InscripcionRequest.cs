using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class InscripcionRequest
    {
        [Required(ErrorMessage = "El ID del alumno es obligatorio.")]
        public int AlumnoId { get; set; }

        [Required(ErrorMessage = "El ID del curso es obligatorio.")]
        public int CursoId { get; set; }
    }
}