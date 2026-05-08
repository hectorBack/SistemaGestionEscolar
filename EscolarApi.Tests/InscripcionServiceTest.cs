using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Tests
{
    public class InscripcionServiceTest : IDisposable
    {
        private readonly GestionEscolarDbContext _context;
        private readonly SqliteConnection _connection;
        private readonly InscripcionService _service;

        public InscripcionServiceTest()
        {
            // Configuración de SQLite in-memory
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new GestionEscolarDbContext(options);
            _context.Database.EnsureCreated();

            _service = new InscripcionService(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }

        [Fact]
        public async Task Inscribir_SiNoHayCupo_LanzaExcepcion()
        {
            // ARRANGE
            // 1. Crear el Alumno
            var alumno = new Alumnos { Nombre = "Juan", Apellido = "Perez", Matricula = "A01", Activo = true };
            _context.Alumnos.Add(alumno);

            // 2. Crear el Usuario y Docente (Necesario para que el Curso sea válido en SQLite)
            var usuarioDocente = new Usuarios
            {
                Email = "profe@test.com",
                Password = "123",
                Rol = "Docente",
                Activo = true
            };
            _context.Usuarios.Add(usuarioDocente);
            await _context.SaveChangesAsync();

            var docente = new Docentes
            {
                Nombre = "Profesor",
                Apellido = "Prueba",
                NumeroEmpleado = "P001",
                UsuarioId = usuarioDocente.Id,
                Activo = true
            };
            _context.Docentes.Add(docente);

            // 3. Crear la Materia
            var materia = new Materias { Nombre = "Programación", Codigo = "PRG1" };
            _context.Materias.Add(materia);

            await _context.SaveChangesAsync();

            // 4. Crear el Curso SIN CUPO vinculando Materia y Docente
            var cursoSinCupo = new Cursos
            {
                MateriaId = materia.Id,
                DocenteId = docente.Id, // 🔥 Agregado para evitar el error de FOREIGN KEY
                CupoDisponible = 0,
                Activo = true,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = TimeSpan.FromHours(8),
                HoraFin = TimeSpan.FromHours(10)
            };
            _context.Cursos.Add(cursoSinCupo);
            await _context.SaveChangesAsync();

            var request = new InscripcionRequest { AlumnoId = alumno.Id, CursoId = cursoSinCupo.Id };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Inscribir(request));
            Assert.Contains("no hay cupos disponibles", ex.Message);
        }

        [Fact]
        public async Task Inscribir_SiHayChoqueHorario_LanzaExcepcion()
        {
            // ARRANGE
            // 1. Crear el Alumno con Apellido (para evitar el NOT NULL constraint)
            var alumno = new Alumnos
            {
                Nombre = "Juan",
                Apellido = "Perez", // 🔥 Agregado para cumplir con la restricción
                Matricula = "A01",
                Activo = true
            };

            // 2. Crear un Docente genérico para los cursos (para evitar el FOREIGN KEY error)
            var usuarioDocente = new Usuarios { Email = "profe_choque@test.com", Password = "123", Rol = "Docente", Activo = true };
            _context.Usuarios.Add(usuarioDocente);
            await _context.SaveChangesAsync();

            var docente = new Docentes { Nombre = "Profesor", Apellido = "X", NumeroEmpleado = "PX1", UsuarioId = usuarioDocente.Id, Activo = true };
            _context.Docentes.Add(docente);

            var m1 = new Materias { Nombre = "M1", Codigo = "C1" };
            var m2 = new Materias { Nombre = "M2", Codigo = "C2" };

            _context.Alumnos.Add(alumno);
            _context.Materias.AddRange(m1, m2);
            await _context.SaveChangesAsync();

            // 3. Crear los cursos con DocenteId asignado
            // Curso 1: Lunes 8-10
            var curso1 = new Cursos
            {
                MateriaId = m1.Id,
                DocenteId = docente.Id, // 🔥 Agregado
                Activo = true,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = TimeSpan.FromHours(8),
                HoraFin = TimeSpan.FromHours(10),
                CupoDisponible = 10
            };

            // Curso 2: Lunes 9-11 (🔥 Choca con el anterior)
            var curso2 = new Cursos
            {
                MateriaId = m2.Id,
                DocenteId = docente.Id, // 🔥 Agregado
                Activo = true,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = TimeSpan.FromHours(9),
                HoraFin = TimeSpan.FromHours(11),
                CupoDisponible = 10
            };

            _context.Cursos.AddRange(curso1, curso2);
            await _context.SaveChangesAsync();

            // 4. Inscribimos al primero manualmente
            _context.Inscripciones.Add(new Inscripciones
            {
                AlumnoId = alumno.Id,
                CursoId = curso1.Id,
                Estatus = "Activo",
                Activo = true,
                FechaInscripcion = DateTime.Now // Asegúrate de llenar campos requeridos de Inscripciones si los hay
            });
            await _context.SaveChangesAsync();

            var request = new InscripcionRequest { AlumnoId = alumno.Id, CursoId = curso2.Id };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => _service.Inscribir(request));
            Assert.Contains("conflicto de horario", ex.Message);
        }

        [Fact]
        public async Task AsignarCalificacion_SiDocenteNoImparteCurso_LanzaExcepcion()
        {
            // ARRANGE
            // 1. Crear Usuarios (Tablas maestras)
            var usuarioDocenteB = new Usuarios { Email = "docenteB@test.com", Rol = "Docente", Password = "123", Activo = true };
            var usuarioDocenteA = new Usuarios { Email = "docenteA@test.com", Rol = "Docente", Password = "123", Activo = true };
            _context.Usuarios.AddRange(usuarioDocenteA, usuarioDocenteB);

            // 2. Crear Materia (¡ESTO SUELE SER EL CULPABLE!)
            var materia = new Materias { Nombre = "Materia Prueba", Codigo = "PROB101" };
            _context.Materias.Add(materia);

            // 3. Crear Alumno
            var alumno = new Alumnos { Nombre = "Estudiante", Apellido = "Prueba", Matricula = "ST123", Activo = true };
            _context.Alumnos.Add(alumno);

            await _context.SaveChangesAsync(); // Guardamos los maestros para obtener sus IDs

            // 4. Crear Docentes vinculados a sus Usuarios
            var docenteA = new Docentes { Nombre = "Docente A", Apellido = "Apellido A", NumeroEmpleado = "A", Activo = true, UsuarioId = usuarioDocenteA.Id };
            var docenteB = new Docentes { Nombre = "Docente B", Apellido = "Apellido B", NumeroEmpleado = "B", Activo = true, UsuarioId = usuarioDocenteB.Id };
            _context.Docentes.AddRange(docenteA, docenteB);
            await _context.SaveChangesAsync();

            // 5. Crear el Curso vinculado al Docente A y a la Materia
            var curso = new Cursos
            {
                MateriaId = materia.Id, // 🔥 Aseguramos que la materia exista
                DocenteId = docenteA.Id,
                Activo = true,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = TimeSpan.Zero,
                HoraFin = TimeSpan.Zero
            };
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            // 6. Crear la Inscripción vinculada al Alumno y al Curso reales
            var inscripcion = new Inscripciones { AlumnoId = alumno.Id, CursoId = curso.Id, Estatus = "Activo", Activo = true };
            _context.Inscripciones.Add(inscripcion);
            await _context.SaveChangesAsync();

            // ACT & ASSERT
            var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
                _service.AsignarCalificacion(inscripcion.Id, 85, usuarioDocenteB.Id, "Docente"));

            Assert.Contains("No tienes permiso", ex.Message);
        }
    }
}