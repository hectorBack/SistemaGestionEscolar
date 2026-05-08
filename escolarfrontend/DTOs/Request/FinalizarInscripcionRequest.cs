using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class FinalizarInscripcionRequest
    {
        public int InscripcionId { get; set; }
        public decimal Calificacion { get; set; }
    }
}