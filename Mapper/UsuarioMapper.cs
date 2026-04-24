using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public static class UsuarioMapper
    {
        public static UsuarioResponse ToResponse(Usuarios usuario, string? token = null)
        {
            return new UsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Rol = usuario.Rol,
                // El token es opcional, solo se llena en el Login
                Token = token ?? string.Empty
            };
        }

        // Si en el futuro necesitas un mapeo detallado para el Admin
        public static object ToDetailedResponse(Usuarios usuario)
        {
            return new
            {
                usuario.Id,
                usuario.Email,
                usuario.Rol,
                usuario.Activo,
                usuario.FechaRegistro,
                usuario.UltimoAcceso,
                // Podemos saber si es Alumno o Docente por la relación
                TipoPerfil = usuario.Alumnos != null ? "Perfil Alumno" :
                             usuario.Docentes != null ? "Perfil Docente" : "Admin"
            };
        }
    }
}