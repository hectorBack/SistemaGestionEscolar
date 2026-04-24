using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class DistribucionGeneroResponse
    {
        public string Genero { get; set; } = null!;
        public int Total { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class InscripcionesCicloResponse
    {
        public string CicloEscolar { get; set; } = null!;
        public int TotalInscritos { get; set; }
    }
}