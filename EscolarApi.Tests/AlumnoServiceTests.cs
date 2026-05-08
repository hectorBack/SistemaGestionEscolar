using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EscolarApi.Tests
{
    public class AlumnoServiceTests
    {
        // Método auxiliar para configurar el Contexto en memoria
        private GestionEscolarDbContext GetDbContext()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                // Agrega esta línea para ignorar el error de transacciones
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new GestionEscolarDbContext(options);
        }

        [Fact]
        public async Task CrearAlumno_SiDatosSonValidos_RetornaAlumnoResponse()
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new AlumnoService(context);
            var request = new AlumnoRequest
            {
                Nombre = "Héctor",
                Apellido = "Servín",
                Email = "hector@test.com",
                Password = "password123",
                Matricula = "2026001",
                Genero = "M",
                FechaNacimiento = new DateTime(2000, 1, 1)
            };

            // ACT
            var resultado = await service.CrearAlumno(request);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal("2026001", resultado.Matricula);
            Assert.Equal("Héctor Servín", resultado.NombreCompleto);

            // Verificar que se guardó en la DB
            var alumnoEnDb = await context.Alumnos.FirstOrDefaultAsync(a => a.Matricula == "2026001");
            Assert.NotNull(alumnoEnDb);
        }

        [Fact]
        public async Task CrearAlumno_SiMatriculaYaExiste_LanzaExcepcion()
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new AlumnoService(context);
            var matriculaRepetida = "DUP123";

            // Agregamos el alumno previo llenando todos los campos requeridos
            context.Alumnos.Add(new Alumnos
            {
                Matricula = matriculaRepetida,
                Nombre = "Existente",
                Apellido = "Prueba",   // <--- Agregado para evitar el error
                Activo = true
            });
            await context.SaveChangesAsync();

            // El request también debe ser coherente con lo que espera tu Service
            var request = new AlumnoRequest
            {
                Matricula = matriculaRepetida,
                Nombre = "Nuevo",      // Agregado por seguridad
                Apellido = "Test",    // Agregado por seguridad
                Email = "nuevo@test.com",
                FechaNacimiento = new DateTime(2005, 5, 15) // Suele ser requerida en servicios escolares
            };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CrearAlumno(request));
            Assert.Equal("La matrícula ya pertenece a otro alumno.", ex.Message);
        }

        [Theory]
        [InlineData("2000-05-02", 26)] // Cumple años hoy (basado en mayo 2026)
        [InlineData("2000-05-03", 25)] // Cumple años mañana
        [InlineData("2005-01-01", 21)] // Ya cumplió
        public void CalcularEdad_DebeRetornarEdadCorrecta(string fechaStr, int edadEsperada)
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new AlumnoService(context);
            DateTime fechaNacimiento = DateTime.Parse(fechaStr);

            // ACT
            var edadObtenida = service.CalcularEdad(fechaNacimiento);

            // ASSERT
            Assert.Equal(edadEsperada, edadObtenida);
        }

        [Fact]
        public async Task EliminarAlumno_ConInscripcionesActivas_LanzaExcepcion()
        {
            // ARRANGE
            var context = GetDbContext();
            var service = new AlumnoService(context);

            // Llenamos todas las propiedades que tu modelo marca como requeridas
            var alumno = new Alumnos
            {
                Id = 1,
                Nombre = "Héctor",
                Apellido = "Servín",   // Agregado para evitar error de nulabilidad
                Matricula = "2026001", // Agregado para evitar error de nulabilidad
                Activo = true
            };

            var inscripcion = new Inscripciones
            {
                Id = 1,
                AlumnoId = 1,
                Activo = true,
                Estatus = "Inscrito" // Asegúrate de llenar campos obligatorios también aquí
            };

            context.Alumnos.Add(alumno);
            context.Inscripciones.Add(inscripcion);
            await context.SaveChangesAsync();

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() => service.EliminarAlumno(1));
            Assert.Contains("No se puede eliminar un alumno con inscripciones activas", ex.Message);
        }
    }
}