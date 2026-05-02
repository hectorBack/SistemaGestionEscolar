using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IDocenteService
    {
        Task<PagedResponse<DocenteResponse>> ObtenerTodos(int pageNumber, int pageSize, string? nombre, string? numeroEmpleado);
        Task<DocenteResponse?> ObtenerPorId(int id);
        Task<DocenteResponse> CrearDocente(DocenteRequest request);
        Task<bool> ActualizarDocente(int id, DocenteRequest request);
        Task<bool> EliminarDocente(int id); // Borrado lógico
        Task<EstadisticasDocenteResponse> ObtenerEstadisticas();

        Task<bool> RestaurarDocente(int id);
    }
}