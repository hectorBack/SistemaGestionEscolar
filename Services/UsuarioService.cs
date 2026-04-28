using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EscolarApi.Services.Impl
{
    public class UsuarioService : IUsuarioService
    {
        private readonly GestionEscolarDbContext _context;
        private readonly IConfiguration _config;

        public UsuarioService(GestionEscolarDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
                return null;

            // Actualizar fecha de último acceso
            usuario.UltimoAcceso = DateTime.Now;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // GENERAMOS EL TOKEN
            string tokenCreado = GenerarToken(usuario);

            return UsuarioMapper.ToResponse(usuario, tokenCreado);
        }

        public async Task<UsuarioResponse?> ObtenerPerfil(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return null;

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

        public async Task<UsuarioResponse> RegistrarAdmin(AdminRegistroRequest request)
        {
            // 1. Validar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("El correo ya está registrado.");

            // 2. Crear la entidad y hashear la clave
            var nuevoAdmin = new Usuarios
            {
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Rol = "Admin", // Forzamos el rol
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevoAdmin);
            await _context.SaveChangesAsync();

            // 1. Generamos el token para el nuevo administrador
            string token = GenerarToken(nuevoAdmin);

            return UsuarioMapper.ToResponse(nuevoAdmin, token);
        }

        private string GenerarToken(Usuarios usuario)
        {
            // Agregamos ?? string.Empty para evitar el nulo
            var jwtKey = _config.GetSection("Jwt:Key").Value ?? string.Empty;
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Email, usuario.Email));
            claims.AddClaim(new Claim(ClaimTypes.Role, usuario.Rol));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                // Agregamos una validación para la duración
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config.GetSection("Jwt:DurationInMinutes").Value ?? "60")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(tokenConfig);
        }
    }
}