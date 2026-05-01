using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IAsistenciaService
    {
        Task<bool> RegistrarAsistencia(AsistenciaRequest request);
        Task<bool> RegistroMasivo(List<AsistenciaRequest> listaAsistencias);
        Task<IEnumerable<AsistenciaResponse>> ObtenerAsistenciasPorCurso(int cursoId, DateTime fecha);
        Task<IEnumerable<AsistenciaResponse>> ObtenerHistorialAlumno(int alumnoId);
    }
}