using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class AsistenciaRequest
    {
        [Required(ErrorMessage = "El ID de inscripción es obligatorio.")]
        public int InscripcionId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El estatus de la asistencia es obligatorio.")]
        [RegularExpression("^(Asistencia|Falta|Retardo|Justificada)$",
            ErrorMessage = "El estatus debe ser: Asistencia, Falta, Retardo o Justificada.")]
        public string Estatus { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Las observaciones no pueden exceder los 200 caracteres.")]
        public string? Observaciones { get; set; }
    }
}