using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services
{
    public class DocenteService : IDocenteService
    {
        private readonly GestionEscolarDbContext _context;

        public DocenteService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarDocente(int id, DocenteRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var docente = await _context.Docentes
                    .Include(d => d.Usuario)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (docente == null) return false;

                // Validar correo único
                if (docente.Usuario.Email != request.Email &&
                    await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                    throw new Exception("El nuevo correo ya está en uso por otro usuario.");

                // Actualizar Docente
                docente.Nombre = request.Nombre;
                docente.Apellido = request.Apellido;
                docente.Especialidad = request.Especialidad;
                docente.NumeroEmpleado = request.NumeroEmpleado;

                // Actualizar Usuario
                docente.Usuario.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Password))
                    docente.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                _context.Docentes.Update(docente);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<DocenteResponse> CrearDocente(DocenteRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("El correo electrónico ya está registrado.");

            if (await _context.Docentes.AnyAsync(d => d.NumeroEmpleado == request.NumeroEmpleado))
                throw new Exception("El número de empleado ya existe.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoUsuario = new Usuarios
                {
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Rol = "Docente"
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                var nuevoDocente = new Docentes
                {
                    NumeroEmpleado = request.NumeroEmpleado,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Especialidad = request.Especialidad,
                    FechaContratacion = request.FechaContratacion,
                    UsuarioId = nuevoUsuario.Id,
                    Activo = true
                };

                _context.Docentes.Add(nuevoDocente);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // USANDO EL MAPPER: Ya no necesitas el método privado MapearADocenteResponse
                return DocenteMapper.ToResponse(nuevoDocente);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //Eliminar Docente con Borrado Logico
        public async Task<bool> EliminarDocente(int id)
        {
            var docente = await _context.Docentes.Include(d => d.Usuario).FirstOrDefaultAsync(d => d.Id == id);
            if (docente == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Borrado Lógico del Docente
                docente.Activo = false;

                // ¡Mejora! También desactivamos al Usuario para que no pueda loguearse
                if (docente.Usuario != null)
                {
                    docente.Usuario.Activo = false; // Asumiendo que tu tabla Usuarios tiene este campo
                }

                _context.Docentes.Update(docente);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EstadisticasDocenteResponse> ObtenerEstadisticas()
        {
            // Dejamos que la base de datos cuente, no el servidor de C#
            var total = await _context.Docentes.CountAsync();
            if (total == 0) return new EstadisticasDocenteResponse();

            var activos = await _context.Docentes.CountAsync(d => d.Activo);

            var conteoEspecialidad = await _context.Docentes
                .GroupBy(d => d.Especialidad ?? "Otras")
                .Select(g => new EspecialidadCount
                {
                    Especialidad = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(c => c.Cantidad)
                .ToListAsync();

            return new EstadisticasDocenteResponse
            {
                TotalDocentes = total,
                DocentesActivos = activos,
                DocentesInactivos = total - activos,
                ConteoPorEspecialidad = conteoEspecialidad
            };
        }

        public async Task<DocenteResponse> ObtenerPorId(int id)
        {
            var docente = await _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id && d.Activo);

            return docente == null ? null : DocenteMapper.ToResponse(docente);
        }

        public async Task<PagedResponse<DocenteResponse>> ObtenerTodos(int pageNumber, int pageSize, string? nombre, string? numeroEmpleado)
        {
            var query = _context.Docentes
                .Include(d => d.Usuario)
                .Where(d => d.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(d => (d.Nombre + " " + d.Apellido).Contains(nombre));

            if (!string.IsNullOrEmpty(numeroEmpleado))
                query = query.Where(d => d.NumeroEmpleado.Contains(numeroEmpleado));

            var totalRecords = await query.CountAsync();

            var docentesBase = await query
                .OrderBy(d => d.Apellido)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // USANDO EL MAPPER: Convertimos la lista de modelos a lista de respuestas
            var data = docentesBase.Select(d => DocenteMapper.ToResponse(d));

            return new PagedResponse<DocenteResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}