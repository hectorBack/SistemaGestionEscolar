using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Inscripciones
{
    public int Id { get; set; }

    public int AlumnoId { get; set; }

    public int CursoId { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public decimal? CalificacionFinal { get; set; }

    public string Estatus { get; set; } = "Activo";

    public bool Activo { get; set; }

    public virtual Alumnos Alumno { get; set; } = null!;

    public virtual Cursos Curso { get; set; } = null!;
}
