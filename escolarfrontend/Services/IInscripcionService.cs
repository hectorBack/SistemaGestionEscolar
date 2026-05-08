using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IInscripcionService
    {
        // Paginación con filtros por Alumno o Curso
        Task<PagedResponse<InscripcionResponse>> ObtenerTodas(int pageNumber, int pageSize, int? alumnoId, int? cursoId);

        Task<InscripcionResponse?> ObtenerPorId(int id);

        // El método estrella: Inscribir
        Task<InscripcionResponse> Inscribir(InscripcionRequest request);

        // Para dar de baja (esto debería devolver el cupo al curso)
        Task<bool> DarDeBaja(int id);

        // Para asigar calificación al final del curso
        Task<bool> AsignarCalificacion(int id, decimal calificacion, int requestUserId, string requestRole);
    }
}