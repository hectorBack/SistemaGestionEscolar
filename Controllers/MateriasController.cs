using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MateriasController : ControllerBase
    {
        private readonly IMateriaService _materiaService;

        public MateriasController(IMateriaService materiaService)
        {
            _materiaService = materiaService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<MateriasResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? codigo = null)
        {
            var response = await _materiaService.ObtenerTodas(pageNumber, pageSize, nombre, codigo);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MateriasResponse>> GetById(int id)
        {
            var materia = await _materiaService.ObtenerPorId(id);
            if (materia == null) return NotFound(new { Message = "Materia no encontrada." });

            return Ok(materia);
        }

        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<MateriasResponse>> GetByCodigo(string codigo)
        {
            var materia = await _materiaService.ObtenerPorCodigo(codigo);
            if (materia == null) return NotFound(new { Message = $"No existe materia con el código {codigo}." });

            return Ok(materia);
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasMateriaResponse>> GetStats()
        {
            var stats = await _materiaService.ObtenerEstadisticas();
            return Ok(stats);
        }

        [HttpPost]
        public async Task<ActionResult<MateriasResponse>> Create([FromBody] MateriaRequest request)
        {
            var nuevaMateria = await _materiaService.CrearMateria(request);
            return CreatedAtAction(nameof(GetById), new { id = nuevaMateria.Id }, nuevaMateria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaRequest request)
        {
            var actualizado = await _materiaService.Actualizar(id, request);
            if (!actualizado) return NotFound(new { Message = "Materia no encontrada para actualizar." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _materiaService.Eliminar(id);
            if (!eliminado) return NotFound(new { Message = "Materia no encontrada." });

            return NoContent();
        }
    }
}