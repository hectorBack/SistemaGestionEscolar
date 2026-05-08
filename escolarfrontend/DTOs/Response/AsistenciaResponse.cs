using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class AsistenciaResponse
    {
        public int Id { get; set; }
        public int InscripcionId { get; set; }

        // Datos del Alumno (para no tener que hacer otra consulta)
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;

        // Datos del Curso
        public string MateriaNombre { get; set; } = string.Empty;

        // Datos de la Asistencia
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}