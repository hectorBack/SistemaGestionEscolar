using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class CursoResponse
    {
        public int Id { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; } = null!;
        public int DocenteId { get; set; }
        public string DocenteNombre { get; set; } = null!;
        public string CicloEscolar { get; set; } = null!;
        public string Horario { get; set; } = null!;
        public string? Aula { get; set; }
        public int CupoMaximo { get; set; }
        public int CupoDisponible { get; set; }
        public bool Activo { get; set; }
    }
}