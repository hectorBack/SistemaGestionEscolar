using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class AlumnoEnRiesgoResponse
    {
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; } = null!;
        public string MateriaNombre { get; set; } = null!;
        public string DocenteNombre { get; set; } = null!;
        public decimal CalificacionActual { get; set; }
        public string CicloEscolar { get; set; } = null!;
    }
}