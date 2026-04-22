using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class EstadisticasAlumnoResponse
    {
        public int TotalAlumnos { get; set; }
        public int AlumnosActivos { get; set; }
        public int AlumnosInactivos { get; set; }
        public double PromedioEdad { get; set; }
    }
}