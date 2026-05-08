using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public class InscripcionMapper
    {
        public static InscripcionResponse ToResponse(Inscripciones ins)
        {
            return new InscripcionResponse
            {
                Id = ins.Id,
                AlumnoId = ins.AlumnoId,
                AlumnoNombre = ins.Alumno != null ? $"{ins.Alumno.Nombre} {ins.Alumno.Apellido}" : "Alumno no cargado",
                CursoId = ins.CursoId,
                CursoNombre = ins.Curso?.Materia != null
                    ? $"{ins.Curso.Materia.Nombre} ({ins.Curso.Docente?.Nombre})"
                    : "Curso no cargado",

                DiaSemana = ins.Curso?.DiaSemana,
                Aula = ins.Curso?.Aula,
                HorarioCompleto = ins.Curso != null
                ? $"{ins.Curso.DiaSemana} {ins.Curso.HoraInicio:hh\\:mm} - {ins.Curso.HoraFin:hh\\:mm}"
                : "Sin horario",

                FechaInscripcion = ins.FechaInscripcion,
                Estatus = ins.Estatus,
                CalificacionFinal = ins.CalificacionFinal,
                Activo = ins.Activo
            };
        }
    }
}