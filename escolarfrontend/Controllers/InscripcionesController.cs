using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InscripcionesController : ControllerBase
    {
        private readonly IInscripcionService _inscripcionService;

        public InscripcionesController(IInscripcionService inscripcionService)
        {
            _inscripcionService = inscripcionService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<ActionResult<PagedResponse<InscripcionResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? alumnoId = null,
            [FromQuery] int? cursoId = null)
        {
            var response = await _inscripcionService.ObtenerTodas(pageNumber, pageSize, alumnoId, cursoId);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InscripcionResponse>> GetById(int id)
        {
            var inscripcion = await _inscripcionService.ObtenerPorId(id);
            if (inscripcion == null) return NotFound(new { Message = "Inscripción no encontrada." });

            return Ok(inscripcion);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Alumno")]
        public async Task<ActionResult<InscripcionResponse>> Create([FromBody] InscripcionRequest request)
        {
            // El Service maneja la validación de cupo y el choque de inscripciones duplicadas
            var nuevaInscripcion = await _inscripcionService.Inscribir(request);
            return CreatedAtAction(nameof(GetById), new { id = nuevaInscripcion.Id }, nuevaInscripcion);
        }

        [HttpPatch("{id}/baja")]
        [Authorize(Roles = "Admin,Alumno")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var resultado = await _inscripcionService.DarDeBaja(id);
            if (!resultado) return NotFound(new { Message = "Inscripción no encontrada o ya está dada de baja." });

            return NoContent();
        }

        [HttpPatch("{id}/calificar")]
        [Authorize(Roles = "Admin,Docente")]
        public async Task<IActionResult> Calificar(int id, [FromQuery] decimal calificacion)
        {
            // Extraer ID y Rol del Token
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            try
            {
                var resultado = await _inscripcionService.AsignarCalificacion(id, calificacion, userId, userRole);
                return Ok(new { Message = "Calificación asignada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}