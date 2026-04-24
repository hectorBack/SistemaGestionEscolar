using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services.Impl
{
    public class UsuarioService : IUsuarioService
    {
        private readonly GestionEscolarDbContext _context;

        public UsuarioService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarEmail(int id, string nuevoEmail)
        {
            // Validar que el nuevo email no esté ocupado
            if (await _context.Usuarios.AnyAsync(u => u.Email == nuevoEmail && u.Id != id))
                throw new Exception("El correo electrónico ya está en uso por otro usuario.");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Email = nuevoEmail;
            _context.Usuarios.Update(usuario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarEstado(int id, bool activo)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Activo = activo;
            _context.Usuarios.Update(usuario);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<UsuarioResponse?> Login(LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo);

            if (usuario == null) return null;

            // Verificar el Hash de la contraseña
            bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password);

            if (!passwordValida) return null;

            // Actualizar fecha de último acceso
            usuario.UltimoAcceso = DateTime.Now;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return UsuarioMapper.ToResponse(usuario);
        }

        public async Task<UsuarioResponse?> ObtenerPorId(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return null;

            return UsuarioMapper.ToResponse(u);
        }

        public async Task<PagedResponse<UsuarioResponse>> ObtenerTodos(int pageNumber, int pageSize, string? rol, bool? activo)
        {
            var query = _context.Usuarios.AsQueryable();

            // Filtros opcionales
            if (!string.IsNullOrEmpty(rol))
                query = query.Where(u => u.Rol == rol);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo.Value);

            var totalRecords = await query.CountAsync();

            var usuarios = await query
                .OrderByDescending(u => u.FechaRegistro)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = usuarios.Select(u => UsuarioMapper.ToResponse(u));

            return new PagedResponse<UsuarioResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}