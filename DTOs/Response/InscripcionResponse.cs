using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class InscripcionResponse
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; } = null!;
        public int CursoId { get; set; }
        public string CursoNombre { get; set; } = null!; // Materia + Docente
        public DateTime FechaInscripcion { get; set; }
        public string Estatus { get; set; } = null!;
        public decimal? CalificacionFinal { get; set; }
        public bool Activo { get; set; }
    }
}