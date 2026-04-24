using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;
using EscolarApi.models;

namespace EscolarApi.Mapper
{
    public static class MateriaMapper
    {
        public static MateriasResponse ToResponse(Materias materia)
        {
            return new MateriasResponse
            {
                Id = materia.Id,
                Codigo = materia.Codigo,
                Nombre = materia.Nombre,
                Descripcion = materia.Descripcion,
                Creditos = materia.Creditos,
                Activo = materia.Activo
            };
        }

    }
}