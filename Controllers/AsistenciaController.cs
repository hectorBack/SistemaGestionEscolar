using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService;

        public AsistenciaController(IAsistenciaService asistenciaService)
        {
            _asistenciaService = asistenciaService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<IActionResult> Registrar([FromBody] AsistenciaRequest request)
        {
            try
            {
                var resultado = await _asistenciaService.RegistrarAsistencia(request);
                return Ok(new { Message = "Asistencia registrada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("masivo")]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<IActionResult> RegistroMasivo([FromBody] List<AsistenciaRequest> lista)
        {
            try
            {
                await _asistenciaService.RegistroMasivo(lista);
                return Ok(new { Message = "Pase de lista masivo completado con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}")]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<IActionResult> ObtenerPorCurso(int cursoId, [FromQuery] DateTime fecha)
        {
            var asistencias = await _asistenciaService.ObtenerAsistenciasPorCurso(cursoId, fecha);
            return Ok(asistencias);
        }

        [HttpGet("alumno/{alumnoId}")]
        [Authorize(Roles = "Admin,Docente,Alumno")]
        public async Task<IActionResult> ObtenerHistorial(int alumnoId)
        {
            // 1. Obtener la identidad del usuario actual desde el Token JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // 2. Si es un Alumno, verificar que el ID que solicita sea el SUYO
            if (userRole == "Alumno")
            {
                // Comparamos el ID del token con el ID de la URL
                if (userIdClaim != alumnoId.ToString())
                {
                    return Forbid("No tienes permiso para ver el historial de otro alumno.");
                }
            }

            // 3. Si es Admin o Docente, o es el Alumno correcto, procedemos
            var historial = await _asistenciaService.ObtenerHistorialAlumno(alumnoId);
            return Ok(historial);
        }
    }
}