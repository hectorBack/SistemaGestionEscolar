using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface ICursoService
    {
        Task<PagedResponse<CursoResponse>> ObtenerTodos(int pageNumber, int pageSize, string? ciclo, int? materiaId);
        Task<CursoResponse?> ObtenerPorId(int id);
        Task<CursoResponse> CrearCurso(CursoRequest request);
        Task<bool> Actualizar(int id, CursoRequest request);
        Task<bool> Eliminar(int id); // Borrado lógico
    }
}