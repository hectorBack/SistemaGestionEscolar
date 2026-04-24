using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Cursos
{
    public int Id { get; set; }

    public int MateriaId { get; set; }

    public int DocenteId { get; set; }

    public string CicloEscolar { get; set; } = null!;

    public string? Horario { get; set; }

    public string? Aula { get; set; }

    public int CupoMaximo { get; set; }

    public int CupoDisponible { get; set; }

    public bool Activo { get; set; }

    public virtual Docentes Docente { get; set; } = null!;

    public virtual ICollection<Inscripciones> Inscripciones { get; set; } = new List<Inscripciones>();

    public virtual Materias Materia { get; set; } = null!;
}
