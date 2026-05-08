using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services
{
    public class AlumnoService : IAlumnoService
    {
        private readonly GestionEscolarDbContext _context;

        public AlumnoService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarAlumno(int id, AlumnoRequest request)
        {
            // Iniciamos transacción para proteger ambas tablas
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscamos al alumno incluyendo su Usuario
                var alumno = await _context.Alumnos
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (alumno == null) return false;

                // Validar que la nueva matrícula no la tenga otro alumno
                if (alumno.Matricula != request.Matricula &&
                    await _context.Alumnos.AnyAsync(a => a.Matricula == request.Matricula))
                    throw new Exception("La matrícula ya está asignada a otro alumno.");

                // 2. Validar que el nuevo correo no esté en uso por nadie más
                if (alumno.Usuario.Email != request.Email &&
                    await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                    throw new Exception("El nuevo correo electrónico ya está en uso por otro usuario.");

                // 3. Actualizar datos del Alumno
                alumno.Nombre = request.Nombre;
                alumno.Apellido = request.Apellido;
                alumno.Genero = request.Genero;
                alumno.FechaNacimiento = request.FechaNacimiento;
                alumno.Matricula = request.Matricula;

                // 4. Actualizar datos del Usuario asociado
                alumno.Usuario.Email = request.Email;

                // Solo actualizar password si se proporciona uno nuevo en el request
                if (!string.IsNullOrEmpty(request.Password))
                {
                    alumno.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                _context.Alumnos.Update(alumno);
                await _context.SaveChangesAsync();

                // Confirmamos los cambios en ambas tablas
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                // Si algo sale mal, no se guarda nada
                await transaction.RollbackAsync();
                throw;
            }
        }

        public int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            // Si el mes actual es menor al de nacimiento, 
            // o es el mismo mes pero el día actual es menor, aún no cumple años.
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }

        public async Task<bool> CambiarPassword(int alumnoId, string nuevaPassword)
        {
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null) return false;

            var usuario = await _context.Usuarios.FindAsync(alumno.UsuarioId);
            if (usuario == null) return false;

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
            _context.Usuarios.Update(usuario);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AlumnoResponse> CrearAlumno(AlumnoRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("El correo electrónico ya se encuentra registrado.");

            if (await _context.Alumnos.AnyAsync(a => a.Matricula == request.Matricula))
                throw new Exception("La matrícula ya pertenece a otro alumno.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoUsuario = new Usuarios
                {
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Rol = "Alumno"
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                var nuevoAlumno = new Alumnos
                {
                    Matricula = request.Matricula,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Genero = request.Genero,
                    FechaNacimiento = request.FechaNacimiento,
                    UsuarioId = nuevoUsuario.Id,
                    Activo = true
                };

                _context.Alumnos.Add(nuevoAlumno);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // USANDO EL MAPPER
                return AlumnoMapper.ToResponse(nuevoAlumno);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> EliminarAlumno(int id)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return false;

            // 1. Validar que no deje grupos a medias
            var tieneInscripciones = await _context.Inscripciones
                .AnyAsync(i => i.AlumnoId == id && i.Activo);

            if (tieneInscripciones)
                throw new Exception("No se puede eliminar un alumno con inscripciones activas.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                alumno.Activo = false;

                if (alumno.Usuario != null)
                {
                    alumno.Usuario.Activo = false;
                }

                // No hace falta el .Update(alumno), EF lo hace por ti
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al procesar la baja del alumno: {ex.Message}");
            }
        }

        public async Task<IEnumerable<AlumnoResponse>> ObtenerAlumnosPorCurso(int cursoId)
        {
            var alumnos = await _context.Inscripciones
                .AsNoTracking()
                .Where(i => i.CursoId == cursoId)
                .Select(i => i.Alumno)
                .Include(a => a.Usuario) // Necesario para el Mapper si pide Email
                .ToListAsync();

            return alumnos.Select(a => AlumnoMapper.ToResponse(a));
        }

        //Obtener Estadisticas de Alumnos
        public async Task<EstadisticasAlumnoResponse> ObtenerEstadisticas()
        {
            var query = _context.Alumnos.AsQueryable();

            var total = await query.CountAsync();
            if (total == 0) return new EstadisticasAlumnoResponse();

            var activos = await query.CountAsync(a => a.Activo);

            // Para el promedio de edad, calculamos el promedio de los años de diferencia en SQL
            var hoy = DateTime.Today;
            var promedioEdad = await query
                .Where(a => a.Activo)
                .Select(a => hoy.Year - a.FechaNacimiento.Year)
                .AverageAsync();

            return new EstadisticasAlumnoResponse
            {
                TotalAlumnos = total,
                AlumnosActivos = activos,
                AlumnosInactivos = total - activos,
                PromedioEdad = Math.Round(promedioEdad, 2)
            };
        }

        public async Task<object> ObtenerKardex(int alumnoId)
        {
            return await _context.Inscripciones
                 .AsNoTracking()
                 .Where(i => i.AlumnoId == alumnoId)
                 .Select(i => new
                 {
                     Materia = i.Curso.Materia.Nombre ?? "Sin nombre",
                     Ciclo = i.Curso.CicloEscolar,
                     Calificacion = i.CalificacionFinal ?? 0,
                     // Usamos concatenación simple para el nombre del profesor
                     Profesor = i.Curso.Docente.Nombre + " " + i.Curso.Docente.Apellido
                 }).ToListAsync();
        }

        public async Task<AlumnoResponse?> ObtenerPorId(int id)
        {
            var a = await _context.Alumnos
                .Include(a => a.Usuario)
                // Agregamos el filtro de Activo para ser consistentes
                .FirstOrDefaultAsync(x => x.Id == id && x.Activo);

            if (a == null) return null;

            return AlumnoMapper.ToResponse(a);
        }

        public async Task<PagedResponse<AlumnoResponse>> ObtenerTodos(int pageNumber, int pageSize, string? nombre, string? matricula)
        {
            // 1. Usamos AsNoTracking para velocidad
            var query = _context.Alumnos
                .Include(a => a.Usuario)
                .Where(a => a.Activo)
                .AsNoTracking()
                .AsQueryable();

            // 2. Búsqueda optimizada (sin concatenar en el servidor)
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(a => a.Nombre.Contains(nombre) || a.Apellido.Contains(nombre));
            }

            if (!string.IsNullOrEmpty(matricula))
            {
                query = query.Where(a => a.Matricula.Contains(matricula));
            }

            var totalRecords = await query.CountAsync();

            var alumnosBase = await query
                .OrderBy(a => a.Apellido)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = alumnosBase.Select(a => AlumnoMapper.ToResponse(a));

            return new PagedResponse<AlumnoResponse>(data, totalRecords, pageNumber, pageSize);
        }

        public async Task<bool> RestaurarAlumno(int id)
        {
            // Usamos IgnoreQueryFilters para poder encontrar al alumno desactivado
            var alumno = await _context.Alumnos
                .IgnoreQueryFilters()
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return false;

            // Si ya está activo, no hay nada que restaurar
            if (alumno.Activo) return true;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Reactivar perfil
                alumno.Activo = true;

                // 2. Reactivar cuenta de usuario
                if (alumno.Usuario != null)
                {
                    alumno.Usuario.Activo = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al restaurar alumno: {ex.Message}");
            }
        }
    }
}