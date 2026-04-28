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
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DocenteController : ControllerBase
    {
        private readonly IDocenteService _docenteService;

        public DocenteController(IDocenteService docenteService)
        {
            _docenteService = docenteService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<DocenteResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? numeroEmpleado = null)
        {
            var response = await _docenteService.ObtenerTodos(pageNumber, pageSize, nombre, numeroEmpleado);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocenteResponse>> GetById(int id)
        {
            var docente = await _docenteService.ObtenerPorId(id);
            if (docente == null) return NotFound(new { Message = "Docente no encontrado o inactivo." });

            return Ok(docente);
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasDocenteResponse>> GetStats()
        {
            var stats = await _docenteService.ObtenerEstadisticas();
            return Ok(stats);
        }

        [HttpPost]
        public async Task<ActionResult<DocenteResponse>> Create([FromBody] DocenteRequest request)
        {
            // Las validaciones de DataAnnotations (como [Required]) se ejecutan automáticamente
            var nuevoDocente = await _docenteService.CrearDocente(request);

            return CreatedAtAction(nameof(GetById), new { id = nuevoDocente.Id }, nuevoDocente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DocenteRequest request)
        {
            var actualizado = await _docenteService.ActualizarDocente(id, request);
            if (!actualizado) return NotFound(new { Message = "No se pudo actualizar. Docente no encontrado." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _docenteService.EliminarDocente(id);
            if (!eliminado) return NotFound(new { Message = "Docente no encontrado." });

            return NoContent();
        }
    }
}