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
    public class MateriaService : IMateriaService
    {
        private readonly GestionEscolarDbContext _context;

        public MateriaService(GestionEscolarDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Actualizar(int id, MateriaRequest request)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return false;

            // Validar si cambiaron el código, que el nuevo no esté duplicado
            if (materia.Codigo != request.Codigo &&
                await _context.Materias.AnyAsync(m => m.Codigo == request.Codigo))
                throw new Exception("El nuevo código ya pertenece a otra materia.");

            materia.Codigo = request.Codigo.ToUpper();
            materia.Nombre = request.Nombre;
            materia.Descripcion = request.Descripcion;
            materia.Creditos = request.Creditos;

            _context.Materias.Update(materia);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<MateriasResponse> CrearMateria(MateriaRequest request)
        {
            // Validar que el código de materia sea único
            if (await _context.Materias.AnyAsync(m => m.Codigo == request.Codigo))
                throw new Exception($"Ya existe una materia registrada con el código: {request.Codigo}");

            var nuevaMateria = new Materias
            {
                Codigo = request.Codigo.ToUpper(), // Siempre en mayúsculas para orden
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Creditos = request.Creditos,
                Activo = true
            };

            _context.Materias.Add(nuevaMateria);
            await _context.SaveChangesAsync();

            return MateriaMapper.ToResponse(nuevaMateria);
        }

        public async Task<bool> Eliminar(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return false;

            // Borrado Lógico
            materia.Activo = false;

            _context.Materias.Update(materia);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<EstadisticasMateriaResponse> ObtenerEstadisticas()
        {
            var materias = await _context.Materias
                .Where(m => m.Activo)
                .ToListAsync();

            if (!materias.Any()) return new EstadisticasMateriaResponse();

            var stats = new EstadisticasMateriaResponse
            {
                TotalMaterias = materias.Count,
                TotalCreditosPlan = materias.Sum(m => m.Creditos),
                PromedioCreditos = Math.Round(materias.Average(m => m.Creditos), 2),

                // Buscamos la materia con el valor máximo de créditos
                MateriaMasCargada = MateriaMapper.ToResponse(
                    materias.OrderByDescending(m => m.Creditos).First()
                ),

                // Agrupamos para ver cuántas materias hay de 3 créditos, cuántas de 5, etc.
                DistribucionCreditos = materias
                    .GroupBy(m => m.Creditos)
                    .Select(g => new CreditosGrupo
                    {
                        Rango = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderBy(g => g.Rango)
                    .ToList()
            };

            return stats;
        }

        public async Task<MateriasResponse?> ObtenerPorCodigo(string codigo)
        {
            var materia = await _context.Materias
                .FirstOrDefaultAsync(m => m.Codigo == codigo && m.Activo);

            return materia == null ? null : MateriaMapper.ToResponse(materia);
        }

        public async Task<MateriasResponse?> ObtenerPorId(int id)
        {
            var materia = await _context.Materias
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            return materia == null ? null : MateriaMapper.ToResponse(materia);
        }

        public async Task<PagedResponse<MateriasResponse>> ObtenerTodas(int pageNumber, int pageSize, string? nombre, string? codigo)
        {
            var query = _context.Materias
                .Where(m => m.Activo)
                .AsQueryable();

            // Filtros opcionales
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(m => m.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(codigo))
                query = query.Where(m => m.Codigo.Contains(codigo));

            var totalRecords = await query.CountAsync();

            var materiasBase = await query
                .OrderBy(m => m.Nombre) // Ordenamos por nombre para que la paginación sea estable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapeo a Response
            var data = materiasBase.Select(m => MateriaMapper.ToResponse(m));

            return new PagedResponse<MateriasResponse>(data, totalRecords, pageNumber, pageSize);
        }
    }
}