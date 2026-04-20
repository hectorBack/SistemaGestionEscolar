using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;

namespace EscolarApi.Services
{
    public interface IAlumnoService
    {
        Task<IEnumerable<AlumnoResponse>> ObtenerTodos();
        Task<AlumnoResponse?> ObtenerPorId(int id);
        // El Controller le pasa un Request y el Service responde con un Response
        Task<AlumnoResponse> CrearAlumno(AlumnoRequest request);
        Task<bool> ActualizarAlumno(int id, AlumnoRequest request);
        Task<bool> EliminarAlumno(int id);
        Task<object> ObtenerKardex(int alumnoId);
        Task<bool> CambiarPassword(int alumnoId, string nuevaPassword);
        Task<IEnumerable<AlumnoResponse>> ObtenerAlumnosPorCurso(int cursoId);

    }
}