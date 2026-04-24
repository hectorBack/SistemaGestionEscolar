using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class EstadisticasMateriaResponse
    {
        public int TotalMaterias { get; set; }
        public int TotalCreditosPlan { get; set; } // Suma de todos los créditos
        public double PromedioCreditos { get; set; }
        public MateriasResponse? MateriaMasCargada { get; set; } // La que tiene más créditos
        public List<CreditosGrupo> DistribucionCreditos { get; set; } = new();
    }

    public class CreditosGrupo
    {
        public int Rango { get; set; } // Ejemplo: Materias de 5 créditos
        public int Cantidad { get; set; }
    }
}