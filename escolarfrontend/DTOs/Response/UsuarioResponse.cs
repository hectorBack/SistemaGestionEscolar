using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public string? Token { get; set; } // Aquí irá el JWT después
    }
}