using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.models;
using EscolarApi.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EscolarApi.Tests
{
    public class UsuarioServiceTest
    {
        private readonly GestionEscolarDbContext _context;
        private readonly UsuarioService _service;
        private readonly Mock<IConfiguration> _mockConfig;

        public UsuarioServiceTest()
        {
            // Configuración de la base de datos en memoria (SQLite)
            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new GestionEscolarDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            // Mock de IConfiguration para el JWT
            _mockConfig = new Mock<IConfiguration>();

            // Configuramos valores por defecto que el servicio buscará
            _mockConfig.Setup(c => c.GetSection("Jwt:Key").Value).Returns("EstaEsUnaLlaveSuperSecretaDe32Caracteres");
            _mockConfig.Setup(c => c.GetSection("Jwt:DurationInMinutes").Value).Returns("60");

            _service = new UsuarioService(_context, _mockConfig.Object);
        }

        [Fact]
        public async Task Login_CredencialesCorrectas_RetornaUsuarioYToken()
        {
            // ARRANGE
            var passwordPlano = "Password123";
            var passwordHasheado = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

            var usuario = new Usuarios
            {
                Email = "test@sistema.com",
                Password = passwordHasheado,
                Rol = "Admin",
                Activo = true,
                FechaRegistro = DateTime.Now
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = "test@sistema.com",
                Password = passwordPlano
            };

            // ACT
            var resultado = await _service.Login(loginRequest);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(usuario.Email, resultado.Email);
            Assert.NotNull(resultado.Token); // Verifica que se generó el JWT
        }

        [Fact]
        public async Task Login_PasswordIncorrecto_RetornaNull()
        {
            // ARRANGE
            var usuario = new Usuarios
            {
                Email = "error@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("correcta"),
                Rol = "Docente",
                Activo = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequest { Email = "error@test.com", Password = "falsa" };

            // ACT
            var resultado = await _service.Login(loginRequest);

            // ASSERT
            Assert.Null(resultado);
        }

        [Fact]
        public async Task RegistrarAdmin_EmailDuplicado_LanzaExcepcion()
        {
            // ARRANGE
            var email = "admin@test.com";
            _context.Usuarios.Add(new Usuarios { Email = email, Password = "1", Rol = "Admin", Activo = true });
            await _context.SaveChangesAsync();

            var request = new AdminRegistroRequest { Email = email, Password = "nueva" };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegistrarAdmin(request));
            Assert.Equal("El correo ya está registrado.", ex.Message);
        }
    }
}