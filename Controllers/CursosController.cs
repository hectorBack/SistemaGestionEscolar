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
    public class CursosController : ControllerBase
    {
        private readonly ICursoService _cursoService;

        public CursosController(ICursoService cursoService)
        {
            _cursoService = cursoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CursoResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? ciclo = null,
            [FromQuery] int? materiaId = null)
        {
            var response = await _cursoService.ObtenerTodos(pageNumber, pageSize, ciclo, materiaId);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CursoResponse>> GetById(int id)
        {
            var curso = await _cursoService.ObtenerPorId(id);
            if (curso == null) return NotFound(new { Message = "Curso no encontrado." });

            return Ok(curso);
        }

        [HttpPost]
        public async Task<ActionResult<CursoResponse>> Create([FromBody] CursoRequest request)
        {
            // Gracias a nuestro Middleware Global, si el Service lanza una Exception 
            // por choque de horarios, el cliente recibirá un error 400/500 automáticamente.
            var nuevoCurso = await _cursoService.CrearCurso(request);
            return CreatedAtAction(nameof(GetById), new { id = nuevoCurso.Id }, nuevoCurso);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CursoRequest request)
        {
            var actualizado = await _cursoService.Actualizar(id, request);
            if (!actualizado) return NotFound(new { Message = "Curso no encontrado para actualizar." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _cursoService.Eliminar(id);
            if (!eliminado) return NotFound(new { Message = "Curso no encontrado." });

            return NoContent();
        }
    }
}