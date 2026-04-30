using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Materias
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int Creditos { get; set; }

    public bool Activo { get; set; }

    public string? Descripcion { get; set; }

    public int? MateriaPrerrequisitoId { get; set; }

    // Propiedad de navegación para el prerrequisito
    public virtual Materias? MateriaPrerrequisito { get; set; }

    public virtual ICollection<Cursos> Cursos { get; set; } = new List<Cursos>();
}
