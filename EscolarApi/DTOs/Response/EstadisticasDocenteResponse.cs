using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class EstadisticasDocenteResponse
    {
        public int TotalDocentes { get; set; }
        public int DocentesActivos { get; set; }
        public int DocentesInactivos { get; set; }
        public List<EspecialidadCount> ConteoPorEspecialidad { get; set; } = new();
    }

    public class EspecialidadCount
    {
        public string Especialidad { get; set; } = "Sin Especialidad";
        public int Cantidad { get; set; }
    }
}