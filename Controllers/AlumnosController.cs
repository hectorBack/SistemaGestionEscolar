using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
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
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _alumnoService.ObtenerTodos());
        }

        //Metodo para obtener un Alumno por {id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var alumno = await _alumnoService.ObtenerPorId(id);
            if (alumno == null) return NotFound();
            return Ok(alumno);
        }

        
    }
}