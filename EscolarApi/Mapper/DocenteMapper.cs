using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public static class DocenteMapper
    {
        public static DocenteResponse ToResponse(Docentes docente)
        {
            return new DocenteResponse
            {
                Id = docente.Id,
                NumeroEmpleado = docente.NumeroEmpleado,
                NombreCompleto = $"{docente.Nombre} {docente.Apellido}",
                Especialidad = docente.Especialidad,
                Email = docente.Usuario?.Email ?? "Sin correo",
                FechaContratacion = docente.FechaContratacion,
                Activo = docente.Activo
            };
        }

    }
}