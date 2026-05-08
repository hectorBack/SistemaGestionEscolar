using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EscolarApi.Tests
{
    public class CursoServiceTests
    {
        private GestionEscolarDbContext GetDbContext()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            return new GestionEscolarDbContext(options);
        }

        [Fact]
        public async Task CrearCurso_SiHayTraslapeDeHorario_LanzaExcepcion()
        {
            var context = GetDbContext();
            var service = new CursoService(context);

            // AGREGAMOS 'Codigo' que es requerido
            context.Materias.Add(new Materias { Id = 1, Nombre = "Matemáticas", Codigo = "MAT101", Activo = true });
            context.Docentes.Add(new Docentes { Id = 1, Nombre = "Juan", Apellido = "Pérez", NumeroEmpleado = "EMP001", Activo = true });

            context.Cursos.Add(new Cursos
            {
                Id = 1,
                DocenteId = 1,
                MateriaId = 1,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(10, 0, 0),
                Activo = true
            });
            await context.SaveChangesAsync();

            var requestConflicto = new CursoRequest
            {
                MateriaId = 1,
                DocenteId = 1,
                CicloEscolar = "2026-1",
                DiaSemana = "Lunes",
                HoraInicio = new TimeSpan(9, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0),
                CupoMaximo = 30
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.CrearCurso(requestConflicto));
            Assert.Contains("El docente ya tiene un curso que se traslapa", ex.Message);
        }

        [Fact]
        public async Task Actualizar_SiSeIntentaReducirCupoMenorAInscritos_LanzaExcepcion()
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new CursoService(context);

            context.Materias.Add(new Materias { Id = 1, Nombre = "Física", Codigo = "FIS101", Activo = true });
            context.Docentes.Add(new Docentes { Id = 1, Nombre = "Ana", Apellido = "García", NumeroEmpleado = "EMP001", Activo = true });

            // Curso con 20 cupo máximo y solo 5 disponibles (significa que hay 15 inscritos)
            var curso = new Cursos
            {
                Id = 5,
                MateriaId = 1,
                DocenteId = 1,
                CupoMaximo = 20,
                CupoDisponible = 5,
                Activo = true,
                CicloEscolar = "2026-1",
                DiaSemana = "Martes",
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(12, 0, 0)
            };
            context.Cursos.Add(curso);
            await context.SaveChangesAsync();

            // Intentamos bajar el cupo máximo a 10 (pero ya hay 15 inscritos)
            var request = new CursoRequest
            {
                MateriaId = 1,
                DocenteId = 1,
                CupoMaximo = 10,
                CicloEscolar = "2026-1",
                DiaSemana = "Martes",
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(12, 0, 0)
            };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() => service.Actualizar(5, request));
            Assert.Contains("No puedes reducir el cupo", ex.Message);
        }

        [Fact]
        public async Task ObtenerTodos_ConFiltroDeCiclo_RetornaSoloCursosDeEseCiclo()
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new CursoService(context);

            context.Materias.Add(new Materias { Id = 1, Nombre = "Materia A", Codigo = "FIS101", Activo = true });
            context.Docentes.Add(new Docentes { Id = 1, Nombre = "Docente A", Apellido = "X", NumeroEmpleado = "EMP001", Activo = true });

            context.Cursos.AddRange(
                new Cursos { Id = 1, CicloEscolar = "2025-1", Activo = true, MateriaId = 1, DocenteId = 1, DiaSemana = "Lunes" },
                new Cursos { Id = 2, CicloEscolar = "2026-1", Activo = true, MateriaId = 1, DocenteId = 1, DiaSemana = "Martes" }
            );
            await context.SaveChangesAsync();

            // ACT
            var resultado = await service.ObtenerTodos(1, 10, "2026-1", null);

            // ASSERT
            Assert.Single(resultado.Data);
            Assert.Equal("2026-1", resultado.Data.First().CicloEscolar);
        }
    }
}