using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class MateriaRequest
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10, ErrorMessage = "El código no debe exceder los 10 caracteres.")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Range(1, 20, ErrorMessage = "Los créditos deben estar entre 1 y 20.")]
        public int Creditos { get; set; }
    }
}