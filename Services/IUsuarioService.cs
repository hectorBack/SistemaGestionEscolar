using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Services
{
    public interface IUsuarioService
    {
        // Autenticación básica (antes de implementar JWT)
        Task<UsuarioResponse?> Login(LoginRequest request);

        // Administración con Paginación
        Task<PagedResponse<UsuarioResponse>> ObtenerTodos(int pageNumber, int pageSize, string? rol, bool? activo);

        Task<UsuarioResponse?> ObtenerPorId(int id);
        Task<bool> CambiarEstado(int id, bool activo); // Activar/Desactivar
        Task<bool> ActualizarEmail(int id, string nuevoEmail);
        Task<UsuarioResponse> RegistrarAdmin(AdminRegistroRequest request);
    }
}