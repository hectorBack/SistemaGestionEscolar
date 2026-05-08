using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public static class CursoMapper
    {
        public static CursoResponse ToResponse(Cursos curso)
        {
            return new CursoResponse
            {
                Id = curso.Id,
                MateriaId = curso.MateriaId,
                // Si la relación está cargada, mostramos el nombre, si no, un aviso
                MateriaNombre = curso.Materia?.Nombre ?? "Materia no cargada",
                DocenteId = curso.DocenteId,
                DocenteNombre = curso.Docente != null
                    ? $"{curso.Docente.Nombre} {curso.Docente.Apellido}"
                    : "Docente no cargado",
                CicloEscolar = curso.CicloEscolar,

                // Nuevos campos en la respuesta
                DiaSemana = curso.DiaSemana,
                HoraInicio = curso.HoraInicio,
                HoraFin = curso.HoraFin,

                Horario = curso.Horario ?? "Sin horario",
                Aula = curso.Aula,
                CupoMaximo = curso.CupoMaximo,
                CupoDisponible = curso.CupoDisponible,
                Activo = curso.Activo
            };
        }

    }
}