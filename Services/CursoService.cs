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

            // 1. Validar Materia y Docente
            if (!await _context.Materias.AnyAsync(m => m.Id == request.MateriaId && m.Activo))
                throw new Exception("La materia seleccionada no existe o está inactiva.");

            if (!await _context.Docentes.AnyAsync(d => d.Id == request.DocenteId && d.Activo))
                throw new Exception("El docente seleccionado no existe o está inactivo.");

            // 2. VALIDACIÓN DE TRASLAPE (Excluyendo el actual)
            var existeConflicto = await _context.Cursos
                .AnyAsync(c => c.DocenteId == request.DocenteId &&
                               c.CicloEscolar == request.CicloEscolar &&
                               c.DiaSemana == request.DiaSemana &&
                               c.Id != id &&
                               c.Activo &&
                               request.HoraInicio < c.HoraFin &&
                               request.HoraFin > c.HoraInicio);

            if (existeConflicto)
            {
                throw new Exception("Conflicto de horario: El docente ya tiene otra clase en ese lapso.");
            }

            // 3. Gestión de Cupos
            int alumnosInscritos = curso.CupoMaximo - curso.CupoDisponible;
            if (request.CupoMaximo < alumnosInscritos)
            {
                throw new Exception($"No puedes reducir el cupo a {request.CupoMaximo} porque ya hay {alumnosInscritos} alumnos inscritos.");
            }

            // 4. Actualizar campos
            curso.CupoDisponible = request.CupoMaximo - alumnosInscritos;
            curso.MateriaId = request.MateriaId;
            curso.DocenteId = request.DocenteId;
            curso.CicloEscolar = request.CicloEscolar;
            curso.DiaSemana = request.DiaSemana;
            curso.HoraInicio = request.HoraInicio;
            curso.HoraFin = request.HoraFin;
            curso.Horario = $"{request.DiaSemana} {request.HoraInicio:hh\\:mm}-{request.HoraFin:hh\\:mm}";
            curso.Aula = request.Aula;
            curso.CupoMaximo = request.CupoMaximo;

            _context.Cursos.Update(curso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<CursoResponse> CrearCurso(CursoRequest request)
        {
            // 1. Validaciones de existencia
            var materia = await _context.Materias.FindAsync(request.MateriaId);
            if (materia == null || !materia.Activo)
                throw new Exception("La materia seleccionada no existe o está inactiva.");

            var docente = await _context.Docentes.FindAsync(request.DocenteId);
            if (docente == null || !docente.Activo)
                throw new Exception("El docente seleccionado no existe o está inactivo.");

            // 2. VALIDACIÓN DE TRASLAPE REAL
            var existeConflicto = await _context.Cursos
                .AnyAsync(c => c.DocenteId == request.DocenteId &&
                               c.CicloEscolar == request.CicloEscolar &&
                               c.DiaSemana == request.DiaSemana &&
                               c.Activo &&
                               request.HoraInicio < c.HoraFin && // Lógica de traslape
                               request.HoraFin > c.HoraInicio);

            if (existeConflicto)
            {
                throw new Exception($"El docente ya tiene un curso que se traslapa en el ciclo {request.CicloEscolar} el día {request.DiaSemana} entre {request.HoraInicio} y {request.HoraFin}.");
            }

            // 3. Mapeo y Creación
            var nuevoCurso = new Cursos
            {
                MateriaId = request.MateriaId,
                DocenteId = request.DocenteId,
                CicloEscolar = request.CicloEscolar,
                DiaSemana = request.DiaSemana,
                HoraInicio = request.HoraInicio,
                HoraFin = request.HoraFin,
                // Construimos el horario para compatibilidad
                Horario = $"{request.DiaSemana} {request.HoraInicio:hh\\:mm}-{request.HoraFin:hh\\:mm}",
                Aula = request.Aula,
                CupoMaximo = request.CupoMaximo,
                CupoDisponible = request.CupoMaximo,
                Activo = true
            };

            _context.Cursos.Add(nuevoCurso);
            await _context.SaveChangesAsync();

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