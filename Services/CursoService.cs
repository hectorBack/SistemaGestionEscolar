using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services
{
    public class CursoService : ICursoService
    {
        private readonly GestionEscolarDbContext _context;

        public CursoService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Actualizar(int id, CursoRequest request)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return false;

            // 1. Validar que la nueva Materia y Docente existan (por si los cambiaron)
            if (!await _context.Materias.AnyAsync(m => m.Id == request.MateriaId && m.Activo))
                throw new Exception("La materia seleccionada no existe o está inactiva.");

            if (!await _context.Docentes.AnyAsync(d => d.Id == request.DocenteId && d.Activo))
                throw new Exception("El docente seleccionado no existe o está inactivo.");

            // 2. VALIDACIÓN DE CHOQUE DE HORARIOS (Excluyendo el curso actual)
            var existeConflicto = await _context.Cursos
                .AnyAsync(c => c.DocenteId == request.DocenteId &&
                               c.CicloEscolar == request.CicloEscolar &&
                               c.Horario == request.Horario &&
                               c.Id != id && // <--- IMPORTANTE: No compararse consigo mismo
                               c.Activo);

            if (existeConflicto)
            {
                throw new Exception($"No se puede actualizar: El docente ya tiene otro curso en el ciclo {request.CicloEscolar} a las {request.Horario}.");
            }

            // 3. Gestión de Cupos
            // Si el nuevo cupo máximo es menor a lo que ya se ha ocupado, deberías lanzar error.
            // Por ahora, solo ajustamos la diferencia:
            int alumnosInscritos = curso.CupoMaximo - curso.CupoDisponible;
            if (request.CupoMaximo < alumnosInscritos)
            {
                throw new Exception($"No puedes reducir el cupo a {request.CupoMaximo} porque ya hay {alumnosInscritos} alumnos inscritos.");
            }

            curso.CupoDisponible = request.CupoMaximo - alumnosInscritos;
            curso.MateriaId = request.MateriaId;
            curso.DocenteId = request.DocenteId;
            curso.CicloEscolar = request.CicloEscolar;
            curso.Horario = request.Horario;
            curso.Aula = request.Aula;
            curso.CupoMaximo = request.CupoMaximo;

            _context.Cursos.Update(curso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<CursoResponse> CrearCurso(CursoRequest request)
        {
            // 1. Validaciones de existencia (las que ya teníamos)
            var materia = await _context.Materias.FindAsync(request.MateriaId);
            if (materia == null || !materia.Activo)
                throw new Exception("La materia seleccionada no existe o está inactiva.");

            var docente = await _context.Docentes.FindAsync(request.DocenteId);
            if (docente == null || !docente.Activo)
                throw new Exception("El docente seleccionado no existe o está inactivo.");

            // 2. VALIDACIÓN EXTRA: Choque de horarios para el docente
            // Buscamos si el docente ya tiene un curso en ese mismo ciclo y horario
            var existeConflicto = await _context.Cursos
                .AnyAsync(c => c.DocenteId == request.DocenteId &&
                               c.CicloEscolar == request.CicloEscolar &&
                               c.Horario == request.Horario &&
                               c.Activo);

            if (existeConflicto)
            {
                throw new Exception($"El docente ya tiene asignado un curso en el ciclo {request.CicloEscolar} a las {request.Horario}.");
            }

            // 3. Si todo está bien, procedemos a crear
            var nuevoCurso = new Cursos
            {
                MateriaId = request.MateriaId,
                DocenteId = request.DocenteId,
                CicloEscolar = request.CicloEscolar,
                Horario = request.Horario,
                Aula = request.Aula,
                CupoMaximo = request.CupoMaximo,
                CupoDisponible = request.CupoMaximo,
                Activo = true
            };

            _context.Cursos.Add(nuevoCurso);
            await _context.SaveChangesAsync();

            // Cargamos las relaciones para el Mapper
            await _context.Entry(nuevoCurso).Reference(c => c.Materia).LoadAsync();
            await _context.Entry(nuevoCurso).Reference(c => c.Docente).LoadAsync();

            return CursoMapper.ToResponse(nuevoCurso);
        }

        public async Task<bool> Eliminar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return false;

            curso.Activo = false;
            _context.Cursos.Update(curso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<CursoResponse?> ObtenerPorId(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Materia)
                .Include(c => c.Docente)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            return curso == null ? null : CursoMapper.ToResponse(curso);
        }

        public async Task<PagedResponse<CursoResponse>> ObtenerTodos(int pageNumber, int pageSize, string? ciclo, int? materiaId)
        {
            var query = _context.Cursos
                .Include(c => c.Materia)
                .Include(c => c.Docente)
                .Where(c => c.Activo)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(ciclo))
                query = query.Where(c => c.CicloEscolar == ciclo);

            if (materiaId.HasValue)
                query = query.Where(c => c.MateriaId == materiaId.Value);

            var totalRecords = await query.CountAsync();

            var cursosBase = await query
                .OrderByDescending(c => c.CicloEscolar)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = cursosBase.Select(c => CursoMapper.ToResponse(c));

            return new PagedResponse<CursoResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}