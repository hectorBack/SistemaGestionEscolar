using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.models;
using EscolarApi.Services;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Tests
{
    public class MateriaServiceTest
    {
        private readonly GestionEscolarDbContext _context;
        private readonly MateriaService _service;

        public MateriaServiceTest()
        {
            // Configuración de base de datos en memoria (SQLite)
            var options = new DbContextOptionsBuilder<GestionEscolarDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new GestionEscolarDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _service = new MateriaService(_context);
        }

        [Fact]
        public async Task CrearMateria_SiCodigoYaExiste_LanzaExcepcion()
        {
            // ARRANGE
            var materiaExistente = new Materias { Codigo = "MAT101", Nombre = "Mate I", Activo = true };
            _context.Materias.Add(materiaExistente);
            await _context.SaveChangesAsync();

            var request = new MateriaRequest { Codigo = "MAT101", Nombre = "Matematicas Nueva" };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => _service.CrearMateria(request));
            Assert.Contains("Ya existe una materia registrada con el código", ex.Message);
        }

        [Fact]
        public async Task CrearMateria_SiEsExitosa_RetornaMateriaResponse()
        {
            // ARRANGE
            var request = new MateriaRequest
            {
                Codigo = "fis102", // Enviamos minúsculas
                Nombre = "Fisica I",
                Creditos = 5
            };

            // ACT
            var resultado = await _service.CrearMateria(request);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal("FIS102", resultado.Codigo); // Verificamos que el ToUpper() funcionó
            Assert.True(resultado.Id > 0);
        }

        [Fact]
        public async Task ObtenerEstadisticas_CalculaValoresCorrectos()
        {
            // ARRANGE
            _context.Materias.AddRange(new List<Materias>
            {
                new Materias { Codigo = "M1", Nombre = "M1", Creditos = 10, Activo = true },
                new Materias { Codigo = "M2", Nombre = "M2", Creditos = 4, Activo = true },
                new Materias { Codigo = "M3", Nombre = "M3", Creditos = 4, Activo = true }
            });
            await _context.SaveChangesAsync();

            // ACT
            var stats = await _service.ObtenerEstadisticas();

            // ASSERT
            Assert.Equal(3, stats.TotalMaterias);
            Assert.Equal(18, stats.TotalCreditosPlan);
            Assert.Equal(6, stats.PromedioCreditos); // (10+4+4) / 3 = 6
            Assert.Equal("M1", stats.MateriaMasCargada.Nombre);
            // Distribución: hay 2 materias de 4 créditos y 1 de 10 créditos
            Assert.Equal(2, stats.DistribucionCreditos.First(g => g.Rango == 4).Cantidad);
        }

        [Fact]
        public async Task Actualizar_SiMateriaNoExiste_RetornaFalse()
        {
            // ACT
            var resultado = await _service.Actualizar(999, new MateriaRequest { Codigo = "ERROR", Nombre = "X" });

            // ASSERT
            Assert.False(resultado);
        }

        [Fact]
        public async Task Actualizar_SiNuevoCodigoYaLoTieneOtraMateria_LanzaExcepcion()
        {
            // ARRANGE
            var m1 = new Materias { Codigo = "MAT1", Nombre = "Materia 1", Activo = true };
            var m2 = new Materias { Codigo = "MAT2", Nombre = "Materia 2", Activo = true };
            _context.Materias.AddRange(m1, m2);
            await _context.SaveChangesAsync();

            // Intentamos cambiar el código de MAT1 a MAT2
            var request = new MateriaRequest { Codigo = "MAT2", Nombre = "Intento Duplicar" };

            // ACT & ASSERT
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => _service.Actualizar(m1.Id, request));
            Assert.Contains("El nuevo código ya pertenece a otra materia", ex.Message);
        }

        [Fact]
        public async Task Eliminar_RealizaBorradoLogico()
        {
            // ARRANGE
            var materia = new Materias { Codigo = "DEL1", Nombre = "Por eliminar", Activo = true };
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            // ACT
            var resultado = await _service.Eliminar(materia.Id);

            // ASSERT
            Assert.True(resultado);
            var materiaDb = await _context.Materias.FindAsync(materia.Id);
            Assert.False(materiaDb.Activo); // El campo Activo debe ser false
        }

        [Fact]
        public async Task ObtenerTodas_PaginacionYFiltro_RetornaDatosCorrectos()
        {
            // ARRANGE
            _context.Materias.AddRange(new List<Materias>
            {
                new Materias { Codigo = "ABC", Nombre = "Calculo", Activo = true },
                new Materias { Codigo = "ABD", Nombre = "Algebra", Activo = true },
                new Materias { Codigo = "XYZ", Nombre = "Programacion", Activo = true }
            });
            await _context.SaveChangesAsync();

            // ACT - Filtramos por nombre "Al" y pedimos página 1, tamaño 10
            var paged = await _service.ObtenerTodas(1, 10, "Al", null);

            // ASSERT
            Assert.Single(paged.Data); // Solo Algebra coincide
            Assert.Equal("Algebra", paged.Data.First().Nombre);
            Assert.Equal(1, paged.TotalRecords);
        }
    }
}