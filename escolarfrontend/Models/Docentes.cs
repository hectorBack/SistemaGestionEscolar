using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Docentes
{
    public int Id { get; set; }

    public string NumeroEmpleado { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Especialidad { get; set; }

    public DateTime FechaContratacion { get; set; }

    public bool Activo { get; set; }

    public int UsuarioId { get; set; }

    public virtual ICollection<Cursos> Cursos { get; set; } = new List<Cursos>();

    public virtual Usuarios? Usuario { get; set; }
}
