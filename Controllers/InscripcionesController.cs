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
    [Authorize(Roles = "Admin,Docente,Alumno")]
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
        public async Task<ActionResult<InscripcionResponse>> Create([FromBody] InscripcionRequest request)
        {
            // El Service maneja la validación de cupo y el choque de inscripciones duplicadas
            var nuevaInscripcion = await _inscripcionService.Inscribir(request);
            return CreatedAtAction(nameof(GetById), new { id = nuevaInscripcion.Id }, nuevaInscripcion);
        }

        [HttpPatch("{id}/baja")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var resultado = await _inscripcionService.DarDeBaja(id);
            if (!resultado) return NotFound(new { Message = "Inscripción no encontrada o ya está dada de baja." });

            return NoContent();
        }

        [HttpPatch("{id}/calificar")]
        public async Task<IActionResult> Calificar(int id, [FromQuery] decimal calificacion)
        {
            if (calificacion < 0 || calificacion > 100)
                return BadRequest(new { Message = "La calificación debe estar entre 0 y 100." });

            var resultado = await _inscripcionService.AsignarCalificacion(id, calificacion);
            if (!resultado) return NotFound(new { Message = "No se pudo asignar la calificación." });

            return Ok(new { Message = "Calificación asignada correctamente." });
        }
    }
}