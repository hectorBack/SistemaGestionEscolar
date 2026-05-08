using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Tests
{
    public class ReporteServiceTest
    {
        private readonly GestionEscolarDbContext _context;
        private readonly ReporteService _service;

        public ReporteServiceTest()
        {
            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new GestionEscolarDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _service = new ReporteService(_context);
        }

        [Fact]
        public async Task ObtenerAlumnosEnRiesgo_RetornaSoloAlumnosConMenosDeSiete()
        {
            // ARRANGE
            var usuario = new Usuarios { Email = "doc@test.com", Rol = "Docente", Password = "123", Activo = true };
            _context.Usuarios.Add(usuario);

            var materia = new Materias { Nombre = "Programación", Codigo = "PRG1" };
            _context.Materias.Add(materia);

            var alumnoRiesgo = new Alumnos { Nombre = "Juan", Apellido = "Malo", Matricula = "A1", Activo = true };
            var alumnoAprobado = new Alumnos { Nombre = "Ana", Apellido = "Buena", Matricula = "A2", Activo = true };
            _context.Alumnos.AddRange(alumnoRiesgo, alumnoAprobado);
            await _context.SaveChangesAsync();

            var docente = new Docentes { Nombre = "Profe", Apellido = "X", NumeroEmpleado = "1", UsuarioId = usuario.Id, Activo = true };
            _context.Docentes.Add(docente);
            await _context.SaveChangesAsync();

            // 🔥 Agregamos DiaSemana, HoraInicio y HoraFin para cumplir con las restricciones
            var curso = new Cursos
            {
                MateriaId = materia.Id,
                DocenteId = docente.Id,
                CicloEscolar = "2024-1",
                DiaSemana = "Lunes",          // <--- Agregado
                HoraInicio = TimeSpan.Zero,   // <--- Agregado (o una hora válida)
                HoraFin = TimeSpan.Zero,      // <--- Agregado
                Activo = true
            };
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            // Calificación 5.0 (Riesgo) y 9.0 (Aprobado)
            _context.Inscripciones.AddRange(
                new Inscripciones { AlumnoId = alumnoRiesgo.Id, CursoId = curso.Id, CalificacionFinal = 5.0m, Activo = true, Estatus = "Activo" },
                new Inscripciones { AlumnoId = alumnoAprobado.Id, CursoId = curso.Id, CalificacionFinal = 9.0m, Activo = true, Estatus = "Activo" }
            );
            await _context.SaveChangesAsync();

            // ACT
            var resultado = await _service.ObtenerAlumnosEnRiesgo();

            // ASSERT
            Assert.Single(resultado);
            Assert.Equal("Juan Malo", resultado.First().AlumnoNombre);
            Assert.Equal(5.0m, resultado.First().CalificacionActual);
        }

        [Fact]
        public async Task ObtenerCursosSinCupo_RetornaCursosConCupoCero()
        {
            // ARRANGE
            // 1. Crear Usuario y Docente real para evitar errores de FK
            var usuario = new Usuarios { Email = "profe_cupo@test.com", Rol = "Docente", Password = "123", Activo = true };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var docente = new Docentes { Nombre = "Docente", Apellido = "Prueba", NumeroEmpleado = "DP01", UsuarioId = usuario.Id, Activo = true };
            _context.Docentes.Add(docente);

            // 2. Crear Materia
            var materia = new Materias { Nombre = "Base de Datos", Codigo = "BD1" };
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            // 3. Crear Cursos con TODOS los campos obligatorios (DiaSemana, Horas)
            _context.Cursos.AddRange(
                new Cursos
                {
                    MateriaId = materia.Id,
                    DocenteId = docente.Id,
                    CupoDisponible = 0,
                    CupoMaximo = 30,
                    CicloEscolar = "2024-1",
                    DiaSemana = "Lunes",          // 🔥 Obligatorio
                    HoraInicio = TimeSpan.Zero,   // 🔥 Obligatorio
                    HoraFin = TimeSpan.Zero,      // 🔥 Obligatorio
                    Activo = true
                },
                new Cursos
                {
                    MateriaId = materia.Id,
                    DocenteId = docente.Id,
                    CupoDisponible = 5,
                    CupoMaximo = 30,
                    CicloEscolar = "2024-1",
                    DiaSemana = "Martes",         // 🔥 Obligatorio
                    HoraInicio = TimeSpan.Zero,   // 🔥 Obligatorio
                    HoraFin = TimeSpan.Zero,      // 🔥 Obligatorio
                    Activo = true
                }
            );
            await _context.SaveChangesAsync();

            // ACT
            var resultado = await _service.ObtenerCursosSinCupo();

            // ASSERT
            Assert.Single(resultado);
            // Verificamos que el curso que regresó es efectivamente el que tiene cupo 0
            Assert.Equal(0, _context.Cursos.First(c => c.Id == resultado.First().CursoId).CupoDisponible);
        }

        [Fact]
        public async Task ObtenerDistribucionPorGenero_CalculaPorcentajesCorrectamente()
        {
            // ARRANGE
            _context.Alumnos.AddRange(
                new Alumnos { Nombre = "A", Apellido = "1", Matricula = "M1", Genero = "Masculino", Activo = true },
                new Alumnos { Nombre = "B", Apellido = "2", Matricula = "M2", Genero = "Masculino", Activo = true },
                new Alumnos { Nombre = "C", Apellido = "3", Matricula = "M3", Genero = "Femenino", Activo = true }
            );
            await _context.SaveChangesAsync();

            // ACT
            var resultado = await _service.ObtenerDistribucionPorGenero();

            // ASSERT
            var masculino = resultado.First(r => r.Genero == "Masculino");
            var femenino = resultado.First(r => r.Genero == "Femenino");

            Assert.Equal(2, masculino.Total);

            // 🔥 CORRECCIÓN: 66.666... redondeado a 2 decimales es 66.67
            Assert.Equal(66.67m, Math.Round(masculino.Porcentaje, 2));

            // 🔥 CORRECCIÓN: 33.333... redondeado a 2 decimales es 33.33
            Assert.Equal(33.33m, Math.Round(femenino.Porcentaje, 2));
        }

        [Fact]
        public async Task ObtenerPromediosPorCurso_RetornaPromedioCorrecto()
        {
            // ARRANGE
            // 1. Crear Usuario y Docente
            var usuario = new Usuarios { Email = "u@u.com", Rol = "Docente", Password = "1", Activo = true };
            _context.Usuarios.Add(usuario);

            var materia = new Materias { Nombre = "Mate", Codigo = "M1" };
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            var docente = new Docentes { Nombre = "D", Apellido = "D", NumeroEmpleado = "D1", UsuarioId = usuario.Id, Activo = true };
            _context.Docentes.Add(docente);
            await _context.SaveChangesAsync();

            // 2. Crear Alumnos (Importante: deben existir en la DB para las Inscripciones)
            var alumno1 = new Alumnos { Nombre = "A1", Apellido = "L1", Matricula = "MAT1", Activo = true };
            var alumno2 = new Alumnos { Nombre = "A2", Apellido = "L2", Matricula = "MAT2", Activo = true };
            _context.Alumnos.AddRange(alumno1, alumno2);
            await _context.SaveChangesAsync();

            // 3. Crear Curso con todos los campos obligatorios
            var curso = new Cursos
            {
                MateriaId = materia.Id,
                DocenteId = docente.Id,
                CicloEscolar = "2024",
                DiaSemana = "Lunes",          // 🔥 Agregado: Evita el NOT NULL constraint failed
                HoraInicio = TimeSpan.Zero,   // 🔥 Agregado
                HoraFin = TimeSpan.Zero,      // 🔥 Agregado
                Activo = true
            };
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            // 4. Crear Inscripciones con alumnos reales
            _context.Inscripciones.AddRange(
                new Inscripciones { AlumnoId = alumno1.Id, CursoId = curso.Id, CalificacionFinal = 10m, Activo = true, Estatus = "Activo" },
                new Inscripciones { AlumnoId = alumno2.Id, CursoId = curso.Id, CalificacionFinal = 8m, Activo = true, Estatus = "Activo" }
            );
            await _context.SaveChangesAsync();

            // ACT
            var resultado = await _service.ObtenerPromediosPorCurso();

            // ASSERT
            Assert.Single(resultado);
            // Usamos 'm' para comparar decimales y asegurar precisión
            Assert.Equal(9.0m, (decimal)resultado.First().PromedioGeneral);
            Assert.Equal(2, resultado.First().TotalAlumnosCalificados);
        }
    }
}