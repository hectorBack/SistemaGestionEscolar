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
            var docente = await _context.Docentes
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (docente == null) return false;

            // Validar que el nuevo email no lo tenga otro usuario
            if (docente.Usuario.Email != request.Email &&
                await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("El nuevo correo ya está en uso por otro usuario.");

            // Actualizar datos del Docente
            docente.Nombre = request.Nombre;
            docente.Apellido = request.Apellido;
            docente.Especialidad = request.Especialidad;
            docente.NumeroEmpleado = request.NumeroEmpleado;

            // Actualizar Usuario asociado
            docente.Usuario.Email = request.Email;

            // Solo actualizar password si se proporciona uno nuevo
            if (!string.IsNullOrEmpty(request.Password))
                docente.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            _context.Docentes.Update(docente);
            return await _context.SaveChangesAsync() > 0;
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
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null) return false;

            // Borrado Lógico
            docente.Activo = false;

            _context.Docentes.Update(docente);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<EstadisticasDocenteResponse> ObtenerEstadisticas()
        {
            // Obtenemos solo los datos necesarios de la base de datos
            var docentes = await _context.Docentes.ToListAsync();

            if (!docentes.Any())
            {
                return new EstadisticasDocenteResponse();
            }

            var stats = new EstadisticasDocenteResponse
            {
                TotalDocentes = docentes.Count,
                DocentesActivos = docentes.Count(d => d.Activo),
                DocentesInactivos = docentes.Count(d => !d.Activo),

                // Agrupamos por especialidad y contamos cuántos hay en cada una
                ConteoPorEspecialidad = docentes
                    .GroupBy(d => d.Especialidad ?? "Otras")
                    .Select(g => new EspecialidadCount
                    {
                        Especialidad = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(c => c.Cantidad)
                    .ToList()
            };

            return stats;
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