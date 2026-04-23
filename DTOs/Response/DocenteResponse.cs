using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class DocenteResponse
    {
        public int Id { get; set; }
        public string NumeroEmpleado { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string? Especialidad { get; set; }
        public string Email { get; set; } = null!;
        public DateTime FechaContratacion { get; set; }
        public bool Activo { get; set; }

        // Propiedad calculada: Antigüedad en la institución
        public int Antiguedad
        {
            get
            {
                var hoy = DateTime.Today;
                var anos = hoy.Year - FechaContratacion.Year;
                if (FechaContratacion.Date > hoy.AddYears(-anos)) anos--;
                return anos;
            }
        }

    }
}