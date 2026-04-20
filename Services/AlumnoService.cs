using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
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

        public async Task<bool> CambiarPassword(int alumnoId, string nuevaPassword)
        {
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null) return false;

            var usuario = await _context.Usuarios.FindAsync(alumno.UsuarioId);
            if (usuario == null) return false;

            usuario.Password = nuevaPassword; // Recuerda encriptar esto en el futuro
            _context.Usuarios.Update(usuario);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AlumnoResponse> CrearAlumno(AlumnoRequest request)
        {
            // 1. Iniciamos una transacción para asegurar que se creen ambos o ninguno
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Crear la entidad Usuario primero
                var nuevoUsuario = new Usuarios
                {
                    Email = request.Email,
                    Password = request.Password, // Nota: En producción, usa un Hash aquí
                    Rol = "Alumno"
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // 3. Crear la entidad Alumno vinculada al UsuarioId recién generado
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

                // 4. Confirmar cambios en la base de datos
                await transaction.CommitAsync();

                // 5. Mapear a Response para devolver al controlador
                return new AlumnoResponse
                {
                    Id = nuevoAlumno.Id,
                    Matricula = nuevoAlumno.Matricula,
                    NombreCompleto = $"{nuevoAlumno.Nombre} {nuevoAlumno.Apellido}",
                    Email = nuevoUsuario.Email,
                    Edad = DateTime.Today.Year - nuevoAlumno.FechaNacimiento.Year // Cálculo simple
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // O manejar un error personalizado
            }
        }

        public async Task<bool> EliminarAlumno(int id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return false;

            // Buscamos el usuario asociado para borrarlo y que la cascada haga el resto
            var usuario = await _context.Usuarios.FindAsync(alumno.UsuarioId);
            if (usuario != null) _context.Usuarios.Remove(usuario);

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
                Email = a.Usuario?.Email ?? "Sin correo"
            };
        }

        public async Task<IEnumerable<AlumnoResponse>> ObtenerTodos()
        {
            return await _context.Alumnos
                .Include(a => a.Usuario) // Si quieres traer el email del usuario relacionado
                .Select(a => new AlumnoResponse
                {
                    Id = a.Id,
                    Matricula = a.Matricula,
                    NombreCompleto = $"{a.Nombre} {a.Apellido}",
                    Email = a.Usuario != null ? a.Usuario.Email : "Sin correo"
                }).ToListAsync();
        }

        
    }
}