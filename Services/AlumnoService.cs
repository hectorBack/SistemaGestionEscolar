using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
using EscolarApi.DTOs.Response;
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
            // 1. Validar si el email ya existe en la tabla de Usuarios
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (emailExiste)
            {
                // Lanzamos una excepción personalizada o manejamos el error
                throw new Exception("El correo electrónico ya se encuentra registrado.");
            }

            var matriculaExiste = await _context.Alumnos.AnyAsync(a => a.Matricula == request.Matricula);
            if (matriculaExiste)
            {
                throw new Exception("La matrícula ya pertenece a otro alumno.");
            }

            if (request.FechaNacimiento > DateTime.Now.AddYears(-3))
            {
                throw new Exception("La fecha de nacimiento no es válida para un estudiante (mínimo 3 años).");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // HASHEAR la contraseña antes de crear el objeto Usuario
                // El 'Salt' se genera automáticamente dentro del Hash
                string passwordHasheada = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 2. Crear la entidad Usuario
                var nuevoUsuario = new Usuarios
                {
                    Email = request.Email,
                    Password = passwordHasheada,
                    Rol = "Alumno"
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // 3. Crear la entidad Alumno vinculada
                var nuevoAlumno = new Alumnos
                {
                    Matricula = request.Matricula,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    FechaNacimiento = request.FechaNacimiento,
                    UsuarioId = nuevoUsuario.Id
                };

                _context.Alumnos.Add(nuevoAlumno);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AlumnoResponse
                {
                    Id = nuevoAlumno.Id,
                    Matricula = nuevoAlumno.Matricula,
                    NombreCompleto = $"{nuevoAlumno.Nombre} {nuevoAlumno.Apellido}",
                    Email = nuevoUsuario.Email,
                    Edad = CalcularEdad(nuevoAlumno.FechaNacimiento)
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> EliminarAlumno(int id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return false;

            // Buscamos el usuario asociado para borrarlo y que la cascada haga el resto
            //var usuario = await _context.Usuarios.FindAsync(alumno.UsuarioId);
            //if (usuario != null) _context.Usuarios.Remove(usuario);
            alumno.Activo = false;

            _context.Alumnos.Update(alumno);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AlumnoResponse>> ObtenerAlumnosPorCurso(int cursoId)
        {
            return await _context.Inscripciones
        .Where(i => i.CursoId == cursoId)
        .Select(i => new AlumnoResponse
        {
            Id = i.Alumno.Id,
            Matricula = i.Alumno.Matricula,
            NombreCompleto = $"{i.Alumno.Nombre} {i.Alumno.Apellido}",
            Email = i.Alumno.Usuario.Email
        }).ToListAsync();
        }

        //Obtener Estadisticas de Alumnos
        public async Task<EstadisticasAlumnoResponse> ObtenerEstadisticas()
        {
            var alumnos = await _context.Alumnos.ToListAsync();

            if (!alumnos.Any())
            {
                return new EstadisticasAlumnoResponse();
            }

            // Calculamos las edades en memoria
            var edades = alumnos.Select(a => CalcularEdad(a.FechaNacimiento));

            return new EstadisticasAlumnoResponse
            {
                TotalAlumnos = alumnos.Count,
                AlumnosActivos = alumnos.Count(a => a.Activo),
                AlumnosInactivos = alumnos.Count(a => !a.Activo),
                PromedioEdad = Math.Round(edades.Average(), 2)
            };
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return null;

            return new AlumnoResponse
            {
                Id = a.Id,
                Matricula = a.Matricula,
                NombreCompleto = $"{a.Nombre} {a.Apellido}",
                Email = a.Usuario?.Email ?? "Sin correo",
                Edad = CalcularEdad(a.FechaNacimiento)
            };
        }

        public async Task<PagedResponse<AlumnoResponse>> ObtenerTodos(int pageNumber, int pageSize, string? nombre, string? matricula)
        {
            var query = _context.Alumnos
                .Include(a => a.Usuario)
                .Where(a => a.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(a => (a.Nombre + " " + a.Apellido).Contains(nombre));

            if (!string.IsNullOrEmpty(matricula))
                query = query.Where(a => a.Matricula.Contains(matricula));

            var totalRecords = await query.CountAsync();

            // 1. Primero obtenemos los datos de la BD sin el cálculo
            var alumnosBase = await query
                .OrderBy(a => a.Apellido) // <--- ESTO CORRIGE EL SEGUNDO ERROR (el warning)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Aquí la consulta ya se ejecutó en SQL

            // 2. Ahora que están en memoria, mapeamos y calculamos la edad con C#
            var data = alumnosBase.Select(a => new AlumnoResponse
            {
                Id = a.Id,
                Matricula = a.Matricula,
                NombreCompleto = $"{a.Nombre} {a.Apellido}",
                Email = a.Usuario?.Email ?? "Sin correo",
                FechaNacimiento = a.FechaNacimiento,
                Edad = CalcularEdad(a.FechaNacimiento) // Ahora sí funciona porque ya no es SQL
            });

            return new PagedResponse<AlumnoResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}