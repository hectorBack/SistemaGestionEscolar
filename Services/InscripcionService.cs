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
    public class InscripcionService : IInscripcionService
    {

        private readonly GestionEscolarDbContext _context;

        public InscripcionService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AsignarCalificacion(int id, decimal calificacion)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);
            if (inscripcion == null) return false;

            inscripcion.CalificacionFinal = calificacion;
            // Si tiene calificación, podríamos marcar el estatus como "Finalizado"
            inscripcion.Estatus = "Finalizado";

            _context.Inscripciones.Update(inscripcion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var inscripcion = await _context.Inscripciones.FindAsync(id);
                if (inscripcion == null || inscripcion.Estatus == "Baja") return false;

                // Cambiar estatus
                inscripcion.Estatus = "Baja";

                // DEVOLVER CUPO AL CURSO
                var curso = await _context.Cursos.FindAsync(inscripcion.CursoId);
                if (curso != null)
                {
                    curso.CupoDisponible += 1;
                    _context.Cursos.Update(curso);
                }

                _context.Inscripciones.Update(inscripcion);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<InscripcionResponse> Inscribir(InscripcionRequest request)
        {
            // Iniciamos una transacción para asegurar la integridad
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validar si el alumno existe y está activo
                var alumno = await _context.Alumnos.FindAsync(request.AlumnoId);
                if (alumno == null || !alumno.Activo)
                    throw new Exception("El alumno no existe o está inactivo.");

                // 2. Validar si el curso existe, está activo y TIENE CUPO
                var curso = await _context.Cursos
                    .Include(c => c.Materia)
                    .Include(c => c.Docente)
                    .FirstOrDefaultAsync(c => c.Id == request.CursoId && c.Activo);

                if (curso == null) throw new Exception("El curso no existe.");
                if (curso.CupoDisponible <= 0) throw new Exception("Ya no hay cupos disponibles para este curso.");

                // 3. Validar que el alumno no esté ya inscrito en este mismo curso
                var yaInscrito = await _context.Inscripciones
                    .AnyAsync(i => i.AlumnoId == request.AlumnoId && i.CursoId == request.CursoId && i.Activo && i.Estatus == "Activo");

                if (yaInscrito) throw new Exception("El alumno ya se encuentra inscrito en este curso.");

                // 4. Crear la inscripción
                var nuevaInscripcion = new Inscripciones
                {
                    AlumnoId = request.AlumnoId,
                    CursoId = request.CursoId,
                    FechaInscripcion = DateTime.Now,
                    Estatus = "Activo",
                    Activo = true
                };

                // 5. RESTAR CUPO AL CURSO
                curso.CupoDisponible -= 1;

                _context.Inscripciones.Add(nuevaInscripcion);
                _context.Cursos.Update(curso);

                await _context.SaveChangesAsync();

                // Confirmamos la operación
                await transaction.CommitAsync();

                // Cargamos el alumno para el Mapper (el curso ya lo tenemos por el Include de arriba)
                await _context.Entry(nuevaInscripcion).Reference(i => i.Alumno).LoadAsync();

                return InscripcionMapper.ToResponse(nuevaInscripcion);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<InscripcionResponse?> ObtenerPorId(int id)
        {
            var ins = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Curso).ThenInclude(c => c.Materia)
                .Include(i => i.Curso).ThenInclude(c => c.Docente)
                .FirstOrDefaultAsync(i => i.Id == id);

            return ins == null ? null : InscripcionMapper.ToResponse(ins);
        }

        public async Task<PagedResponse<InscripcionResponse>> ObtenerTodas(int pageNumber, int pageSize, int? alumnoId, int? cursoId)
        {
            var query = _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Curso).ThenInclude(c => c.Materia)
                .Include(i => i.Curso).ThenInclude(c => c.Docente)
                .Where(i => i.Activo)
                .AsQueryable();

            if (alumnoId.HasValue) query = query.Where(i => i.AlumnoId == alumnoId);
            if (cursoId.HasValue) query = query.Where(i => i.CursoId == cursoId);

            var totalRecords = await query.CountAsync();
            var pagedData = await query
                .OrderByDescending(i => i.FechaInscripcion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = pagedData.Select(i => InscripcionMapper.ToResponse(i));

            return new PagedResponse<InscripcionResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}