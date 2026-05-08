using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Mapper;
using EscolarApi.models;
using EscolarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly GestionEscolarDbContext _context;

        public AsistenciaService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AsistenciaResponse>> ObtenerAsistenciasPorCurso(int cursoId, DateTime fecha)
        {
            var asistencias = await _context.Asistencias
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Alumno)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Curso)
                        .ThenInclude(c => c.Materia)
                .AsNoTracking()
                .Where(a => a.Inscripcion.CursoId == cursoId && a.Fecha.Date == fecha.Date)
                .ToListAsync();

            return asistencias.Select(AsistenciaMapper.ToResponse);
        }

        public async Task<IEnumerable<AsistenciaResponse>> ObtenerHistorialAlumno(int alumnoId)
        {
            var asistencias = await _context.Asistencias
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Alumno)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.Curso)
                        .ThenInclude(c => c.Materia)
                .AsNoTracking()
                .Where(a => a.Inscripcion.AlumnoId == alumnoId)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            return asistencias.Select(AsistenciaMapper.ToResponse);
        }

        public async Task<bool> RegistrarAsistencia(AsistenciaRequest request)
        {
            var existeInscripcion = await _context.Inscripciones
                .AnyAsync(i => i.Id == request.InscripcionId && i.Activo);

            if (!existeInscripcion)
                throw new Exception("La inscripción no existe o no está activa.");

            // Evitar duplicados por fecha
            var yaExiste = await _context.Asistencias
                .AnyAsync(a => a.InscripcionId == request.InscripcionId && a.Fecha.Date == request.Fecha.Date);

            if (yaExiste)
                throw new Exception("Ya se registró asistencia para este alumno en la fecha actual.");

            var nuevaAsistencia = new Asistencias
            {
                InscripcionId = request.InscripcionId,
                Fecha = request.Fecha.Date,
                Estatus = request.Estatus,
                Observaciones = request.Observaciones
            };

            _context.Asistencias.Add(nuevaAsistencia);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RegistroMasivo(List<AsistenciaRequest> listaAsistencias)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in listaAsistencias)
                {
                    await RegistrarAsistencia(item);
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}