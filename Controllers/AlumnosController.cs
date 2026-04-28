using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
using EscolarApi.DTOs.Response;
using EscolarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [Authorize(Roles = "Admin,Docente")]
    [ApiController]
    [Route("api/[controller]")]
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnoService _alumnoService;

        public AlumnosController(IAlumnoService alumnoService)
        {
            _alumnoService = alumnoService;
        }

        //Metodo para obtener todos los estudiantes
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AlumnoResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? matricula = null)
        {
            var response = await _alumnoService.ObtenerTodos(pageNumber, pageSize, nombre, matricula);
            return Ok(response);
        }

        //Metodo para obtener un Alumno por {id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AlumnoResponse>> GetById(int id)
        {
            var alumno = await _alumnoService.ObtenerPorId(id);
            if (alumno == null) return NotFound($"No se encontró el alumno con ID {id}");
            return Ok(alumno);
        }

        // 3. Crear un nuevo alumno (y su usuario)
        [HttpPost]
        public async Task<ActionResult<AlumnoResponse>> Create([FromBody] AlumnoRequest request)
        {
            // Si el Service lanza una excepción, el Middleware la atrapará automáticamente
            var nuevoAlumno = await _alumnoService.CrearAlumno(request);
            return CreatedAtAction(nameof(GetById), new { id = nuevoAlumno.Id }, nuevoAlumno);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AlumnoRequest request)
        {
            var actualizado = await _alumnoService.ActualizarAlumno(id, request);
            if (!actualizado) return NotFound();
            return NoContent(); // 204: La operación fue exitosa pero no devuelve contenido
        }

        // 5. Eliminar un alumno (y su usuario por cascada)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _alumnoService.EliminarAlumno(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        // 6. Cambiar la contraseña del alumno
        [HttpPatch("{id}/cambiar-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] string nuevaPassword)
        {
            var resultado = await _alumnoService.CambiarPassword(id, nuevaPassword);
            if (!resultado) return BadRequest("No se pudo actualizar la contraseña.");
            return Ok(new { message = "Contraseña actualizada correctamente" });
        }

        // 7. Ver el Kardex de un alumno
        [HttpGet("{id}/kardex")]
        public async Task<IActionResult> GetKardex(int id)
        {
            var kardex = await _alumnoService.ObtenerKardex(id);
            return Ok(kardex);
        }

        // 8. Listar alumnos inscritos en un curso específico
        [HttpGet("curso/{cursoId}")]
        public async Task<ActionResult<IEnumerable<AlumnoResponse>>> GetByCurso(int cursoId)
        {
            var alumnos = await _alumnoService.ObtenerAlumnosPorCurso(cursoId);
            return Ok(alumnos);
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasAlumnoResponse>> GetEstadisticas()
        {
            var stats = await _alumnoService.ObtenerEstadisticas();
            return Ok(stats);
        }
    }
}