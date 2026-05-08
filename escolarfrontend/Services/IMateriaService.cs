using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IMateriaService
    {
        Task<PagedResponse<MateriasResponse>> ObtenerTodas(int pageNumber, int pageSize, string? nombre, string? codigo);
        Task<MateriasResponse?> ObtenerPorId(int id);
        Task<MateriasResponse?> ObtenerPorCodigo(string codigo);
        Task<MateriasResponse> CrearMateria(MateriaRequest request);
        Task<bool> Actualizar(int id, MateriaRequest request);
        Task<bool> Eliminar(int id); // Borrado lógico
        Task<EstadisticasMateriaResponse> ObtenerEstadisticas();
    }
}