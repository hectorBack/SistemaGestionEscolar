using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.DTOs.Response
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public string? Details { get; set; } // Opcional: para debug
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString() => System.Text.Json.JsonSerializer.Serialize(this);
    }
}