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
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return false;

            alumno.Nombre = request.Nombre;
            alumno.Apellido = request.Apellido;
            alumno.Genero = request.Genero;
            alumno.FechaNacimiento = request.FechaNacimiento;
            alumno.Matricula = request.Matricula;

            _context.Alumnos.Update(alumno);
            return await _context.SaveChangesAsync() > 0;
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
            // Usamos Include para traer al usuario relacionado de una vez
            var alumno = await _context.Alumnos
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return false;

            // 1. Desactivar al Alumno
            alumno.Activo = false;

            // 2. Desactivar su cuenta de acceso (Importante)
            if (alumno.Usuario != null)
            {
                alumno.Usuario.Activo = false;
            }

            _context.Alumnos.Update(alumno);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AlumnoResponse>> ObtenerAlumnosPorCurso(int cursoId)
        {
            var inscritos = await _context.Inscripciones
                .Include(i => i.Alumno)
                    .ThenInclude(a => a.Usuario)
                .Where(i => i.CursoId == cursoId)
                .ToListAsync();

            // USANDO EL MAPPER
            return inscritos.Select(i => AlumnoMapper.ToResponse(i.Alumno));
        }

        //Obtener Estadisticas de Alumnos
        public async Task<EstadisticasAlumnoResponse> ObtenerEstadisticas()
        {
            var alumnos = await _context.Alumnos.ToListAsync();
            if (!alumnos.Any()) return new EstadisticasAlumnoResponse();

            // Nota: Aquí se usa el método de cálculo de edad que ahora 
            // debería ser accesible o estar en una clase de utilidad (Helper)
            var edades = alumnos.Select(a => CalcularEdadInterno(a.FechaNacimiento));

            return new EstadisticasAlumnoResponse
            {
                TotalAlumnos = alumnos.Count,
                AlumnosActivos = alumnos.Count(a => a.Activo),
                AlumnosInactivos = alumnos.Count(a => !a.Activo),
                PromedioEdad = Math.Round(edades.Average(), 2)
            };
        }

        // Método auxiliar para estadísticas
        private int CalcularEdadInterno(DateTime fecha)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fecha.Year;
            if (fecha.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        public async Task<object> ObtenerKardex(int alumnoId)
        {
            return await _context.Inscripciones
                 .Where(i => i.AlumnoId == alumnoId)
                 .Select(i => new
                 {
                     Materia = i.Curso.Materia.Nombre,
                     Ciclo = i.Curso.CicloEscolar,
                     Calificacion = i.CalificacionFinal ?? 0,
                     Profesor = i.Curso.Docente.Nombre
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
    }
}