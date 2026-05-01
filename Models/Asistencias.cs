using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.models;

namespace EscolarApi.Models
{
    public partial class Asistencias
    {
        public int Id { get; set; }
        public int InscripcionId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; } // Sugerencia: Usa un Enum o constantes
        public string? Observaciones { get; set; }

        // Propiedad de navegación
        public virtual Inscripciones Inscripcion { get; set; } = null!;

    }
}