using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs
{
    public class AlumnoResponse
    {
        public int Id { get; set; }
        public string Matricula { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public int Edad { get; set; }
    }
}