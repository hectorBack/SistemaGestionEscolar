using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class PromedioCursoResponse
    {
        public string MateriaNombre { get; set; } = null!;
        public string DocenteNombre { get; set; } = null!;
        public decimal PromedioGeneral { get; set; }
        public int TotalAlumnosCalificados { get; set; }
    }

    public class CursoCupoAgotadoResponse
    {
        public int CursoId { get; set; }
        public string MateriaNombre { get; set; } = null!;
        public string CicloEscolar { get; set; } = null!;
        public int CupoMaximo { get; set; }
    }
}