using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("promedios-cursos")]
        public async Task<IActionResult> GetPromedios()
        {
            var data = await _reporteService.ObtenerPromediosPorCurso();
            return Ok(data);
        }

        [HttpGet("cupos-agotados")]
        public async Task<IActionResult> GetCuposAgotados()
        {
            var data = await _reporteService.ObtenerCursosSinCupo();
            return Ok(data);
        }

        [HttpGet("alumnos-en-riesgo")]
        public async Task<IActionResult> GetAlumnosEnRiesgo()
        {
            var data = await _reporteService.ObtenerAlumnosEnRiesgo();
            return Ok(data);
        }

        [HttpGet("distribucion-genero")]
        public async Task<IActionResult> GetDistribucionGenero()
        {
            var data = await _reporteService.ObtenerDistribucionPorGenero();
            return Ok(data);
        }

        [HttpGet("inscripciones-ciclo")]
        public async Task<IActionResult> GetInscripcionesCiclo()
        {
            var data = await _reporteService.ObtenerInscripcionesPorCiclo();
            return Ok(data);
        }
    }
}