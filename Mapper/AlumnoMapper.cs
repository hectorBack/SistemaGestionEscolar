using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public static class AlumnoMapper
    {
        public static AlumnoResponse ToResponse(Alumnos alumno)
        {
            return new AlumnoResponse
            {
                Id = alumno.Id,
                Matricula = alumno.Matricula,
                NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}",
                Email = alumno.Usuario?.Email ?? "Sin correo",
                Genero = alumno.Genero ?? "No especificado",
                FechaNacimiento = alumno.FechaNacimiento,
                Activo = alumno.Activo,
                Edad = CalcularEdad(alumno.FechaNacimiento)
            };
        }

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }

}