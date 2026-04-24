using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IReporteService
    {
        Task<IEnumerable<PromedioCursoResponse>> ObtenerPromediosPorCurso();
        Task<IEnumerable<CursoCupoAgotadoResponse>> ObtenerCursosSinCupo();
        Task<IEnumerable<AlumnoEnRiesgoResponse>> ObtenerAlumnosEnRiesgo();
        Task<IEnumerable<DistribucionGeneroResponse>> ObtenerDistribucionPorGenero();
        Task<IEnumerable<InscripcionesCicloResponse>> ObtenerInscripcionesPorCiclo();
    }
}