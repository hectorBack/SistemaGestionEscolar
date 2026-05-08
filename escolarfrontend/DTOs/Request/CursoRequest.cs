using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class CursoRequest
    {
        [Required(ErrorMessage = "La materia es obligatoria.")]
        public int MateriaId { get; set; }

        [Required(ErrorMessage = "El docente es obligatorio.")]
        public int DocenteId { get; set; }

        [Required(ErrorMessage = "El ciclo escolar es obligatorio.")]
        [RegularExpression(@"^\d{4}-\d{1}$", ErrorMessage = "El formato del ciclo debe ser YYYY-N (ej: 2024-1).")]
        public string CicloEscolar { get; set; } = null!;

        [Required(ErrorMessage = "El día es obligatorio.")]
        public string DiaSemana { get; set; } = null!; // Ej: "Lunes"

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; } // Formato "08:00:00"

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "El horario es obligatorio.")]
        [StringLength(100)]
        public string Horario { get; set; } = null!;

        [StringLength(50)]
        public string? Aula { get; set; }

        [Required(ErrorMessage = "El cupo máximo es obligatorio.")]
        [Range(1, 100, ErrorMessage = "El cupo máximo debe estar entre 1 y 100 alumnos.")]
        public int CupoMaximo { get; set; }
    }
}