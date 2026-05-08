using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EscolarApi.Tests
{
    public class DocenteServiceTests
    {
        private (GestionEscolarDbContext context, SqliteConnection connection) GetSqliteContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new GestionEscolarDbContext(options);
            context.Database.EnsureCreated();

            return (context, connection);
        }

        [Fact]
        public async Task CrearDocente_SiEmailYaExiste_LanzaExcepcion()
        {
            var (context, connection) = GetSqliteContext();
            try
            {
                var service = new DocenteService(context);

                context.Usuarios.Add(new Usuarios
                {
                    Email = "duplicado@test.com",
                    Password = "123",
                    Rol = "Docente",
                    Activo = true
                });

                await context.SaveChangesAsync();

                var request = new DocenteRequest
                {
                    Email = "duplicado@test.com",
                    Nombre = "Hector",
                    Apellido = "Servin",
                    NumeroEmpleado = "EMP100",
                    Password = "Password123"
                };

                var ex = await Assert.ThrowsAnyAsync<Exception>(
                    () => service.CrearDocente(request)
                );

                Assert.Contains("correo electrónico ya está registrado", ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task EliminarDocente_SiTieneCursosActivos_LanzaExcepcion()
        {
            var (context, connection) = GetSqliteContext();
            try
            {
                var service = new DocenteService(context);

                // 🔥 1. Crear Usuario
                var usuario = new Usuarios
                {
                    Email = "docente@test.com",
                    Password = "123",
                    Rol = "Docente",
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                context.Usuarios.Add(usuario);
                await context.SaveChangesAsync();

                // 🔥 2. Crear Docente con Usuario
                var docente = new Docentes
                {
                    Nombre = "Héctor",
                    Apellido = "Servín",
                    NumeroEmpleado = "123",
                    Activo = true,
                    UsuarioId = usuario.Id
                };

                context.Docentes.Add(docente);
                await context.SaveChangesAsync();

                // 🔥 3. Crear Materia
                var materia = new Materias
                {
                    Nombre = "Matemáticas",
                    Codigo = "MAT-001"
                };

                context.Materias.Add(materia);
                await context.SaveChangesAsync();

                // 🔥 4. Crear Curso
                var curso = new Cursos
                {
                    DocenteId = docente.Id,
                    MateriaId = materia.Id,
                    Activo = true,
                    CicloEscolar = "2026-1",
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.Zero,
                    HoraFin = TimeSpan.Zero
                };

                context.Cursos.Add(curso);
                await context.SaveChangesAsync();

                // 🔹 Act + Assert
                var ex = await Assert.ThrowsAsync<Exception>(
                    () => service.EliminarDocente(docente.Id)
                );

                Assert.Contains("cursos activos", ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task ActualizarDocente_CambiaPassword_Correctamente()
        {
            var (context, connection) = GetSqliteContext();

            try
            {
                var service = new DocenteService(context);

                var usuario = new Usuarios
                {
                    Email = "test@test.com",
                    Password = "OLD_PASSWORD",
                    Rol = "Docente",
                    Activo = true
                };

                context.Usuarios.Add(usuario);
                await context.SaveChangesAsync();

                var docente = new Docentes
                {
                    Nombre = "Original",
                    Apellido = "Original",
                    NumeroEmpleado = "EMP99",
                    UsuarioId = usuario.Id,
                    Usuario = usuario,
                    Activo = true
                };

                context.Docentes.Add(docente);
                await context.SaveChangesAsync();

                var request = new DocenteRequest
                {
                    Nombre = "Actualizado",
                    Apellido = "Actualizado",
                    Email = "test@test.com",
                    NumeroEmpleado = "EMP99",
                    Password = "NEW_PASSWORD"
                };

                // ACT
                var resultado = await service.ActualizarDocente(docente.Id, request);

                // ASSERT
                Assert.True(resultado);

                var docenteDb = await context.Docentes
                    .Include(d => d.Usuario)
                    .FirstAsync(d => d.Id == docente.Id);

                Assert.NotEqual("OLD_PASSWORD", docenteDb.Usuario.Password);
                Assert.True(docenteDb.Usuario.Password.Length > 20);
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task ObtenerEstadisticas_RetornaConteoCorrecto()
        {
            var (context, connection) = GetSqliteContext();

            try
            {
                var service = new DocenteService(context);

                // 🔥 Crear usuarios primero
                var usuario1 = new Usuarios { Email = "a@test.com", Password = "123", Rol = "Docente", Activo = true, FechaRegistro = DateTime.Now };
                var usuario2 = new Usuarios { Email = "b@test.com", Password = "123", Rol = "Docente", Activo = true, FechaRegistro = DateTime.Now };
                var usuario3 = new Usuarios { Email = "c@test.com", Password = "123", Rol = "Docente", Activo = true, FechaRegistro = DateTime.Now };

                context.Usuarios.AddRange(usuario1, usuario2, usuario3);
                await context.SaveChangesAsync();

                // 🔥 Crear docentes con FK válida
                context.Docentes.AddRange(
                    new Docentes { Nombre = "A", Apellido = "A", NumeroEmpleado = "1", Especialidad = "Sistemas", Activo = true, UsuarioId = usuario1.Id },
                    new Docentes { Nombre = "B", Apellido = "B", NumeroEmpleado = "2", Especialidad = "Sistemas", Activo = true, UsuarioId = usuario2.Id },
                    new Docentes { Nombre = "C", Apellido = "C", NumeroEmpleado = "3", Especialidad = "Derecho", Activo = false, UsuarioId = usuario3.Id }
                );

                await context.SaveChangesAsync();

                // ACT
                var stats = await service.ObtenerEstadisticas();

                // ASSERT
                Assert.Equal(3, stats.TotalDocentes);
                Assert.Equal(2, stats.DocentesActivos);
                Assert.Equal(1, stats.DocentesInactivos);

                var sistemas = stats.ConteoPorEspecialidad
                    .First(c => c.Especialidad == "Sistemas");

                Assert.Equal(2, sistemas.Cantidad);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}