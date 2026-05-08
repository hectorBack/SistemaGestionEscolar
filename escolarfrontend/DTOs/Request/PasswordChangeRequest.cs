using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Request
{
    public class PasswordChangeRequest
    {
        public string NuevaPassword { get; set; } = null!;
    }
}