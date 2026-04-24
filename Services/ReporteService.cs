using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services
{
    public class ReporteService : IReporteService
    {
        private readonly GestionEscolarDbContext _context;

        public ReporteService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlumnoEnRiesgoResponse>> ObtenerAlumnosEnRiesgo()
        {
            return await _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Curso).ThenInclude(c => c.Materia)
                .Include(i => i.Curso).ThenInclude(c => c.Docente)
                .Where(i => i.Activo &&
                            i.CalificacionFinal.HasValue &&
                            i.CalificacionFinal < 7.0m) // <--- Filtro de riesgo
                .Select(i => new AlumnoEnRiesgoResponse
                {
                    AlumnoId = i.AlumnoId,
                    AlumnoNombre = $"{i.Alumno.Nombre} {i.Alumno.Apellido}",
                    MateriaNombre = i.Curso.Materia.Nombre,
                    DocenteNombre = $"{i.Curso.Docente.Nombre} {i.Curso.Docente.Apellido}",
                    CalificacionActual = i.CalificacionFinal.Value,
                    CicloEscolar = i.Curso.CicloEscolar
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CursoCupoAgotadoResponse>> ObtenerCursosSinCupo()
        {
            return await _context.Cursos
                .Where(c => c.CupoDisponible == 0 && c.Activo)
                .Select(c => new CursoCupoAgotadoResponse
                {
                    CursoId = c.Id,
                    MateriaNombre = c.Materia.Nombre,
                    CicloEscolar = c.CicloEscolar,
                    CupoMaximo = c.CupoMaximo
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DistribucionGeneroResponse>> ObtenerDistribucionPorGenero()
        {
            var totalAlumnos = await _context.Alumnos.CountAsync(a => a.Activo);

            return await _context.Alumnos
                .Where(a => a.Activo)
                .GroupBy(a => a.Genero)
                .Select(g => new DistribucionGeneroResponse
                {
                    Genero = g.Key ?? "No especificado",
                    Total = g.Count(),
                    Porcentaje = totalAlumnos > 0 ? (decimal)g.Count() * 100 / totalAlumnos : 0
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<InscripcionesCicloResponse>> ObtenerInscripcionesPorCiclo()
        {
            return await _context.Inscripciones
                .Where(i => i.Activo)
                .GroupBy(i => i.Curso.CicloEscolar)
                .Select(g => new InscripcionesCicloResponse
                {
                    CicloEscolar = g.Key,
                    TotalInscritos = g.Count()
                })
                .OrderByDescending(g => g.CicloEscolar)
                .ToListAsync();
        }

        public async Task<IEnumerable<PromedioCursoResponse>> ObtenerPromediosPorCurso()
        {
            return await _context.Inscripciones
                .Where(i => i.CalificacionFinal.HasValue && i.Activo)
                .GroupBy(i => i.CursoId)
                .Select(g => new PromedioCursoResponse
                {
                    MateriaNombre = g.First().Curso.Materia.Nombre,
                    DocenteNombre = $"{g.First().Curso.Docente.Nombre} {g.First().Curso.Docente.Apellido}",
                    PromedioGeneral = g.Average(i => i.CalificacionFinal ?? 0),
                    TotalAlumnosCalificados = g.Count()
                })
                .ToListAsync();
        }
    }
}