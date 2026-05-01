using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.Models;

namespace EscolarApi.Mapper
{
    public static class AsistenciaMapper
    {
        public static AsistenciaResponse ToResponse(Asistencias asistencia)
        {
            if (asistencia == null) return null!;

            return new AsistenciaResponse
            {
                Id = asistencia.Id,
                InscripcionId = asistencia.InscripcionId,

                // Accedemos a los datos del alumno a través de la inscripción
                AlumnoNombre = asistencia.Inscripcion?.Alumno != null
                    ? $"{asistencia.Inscripcion.Alumno.Nombre} {asistencia.Inscripcion.Alumno.Apellido}"
                    : "N/A",

                Matricula = asistencia.Inscripcion?.Alumno?.Matricula ?? "N/A",

                // Accedemos al nombre de la materia
                MateriaNombre = asistencia.Inscripcion?.Curso?.Materia?.Nombre ?? "N/A",

                Fecha = asistencia.Fecha,
                Estatus = asistencia.Estatus,
                Observaciones = asistencia.Observaciones
            };
        }
    }
}