using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Usuarios
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public virtual Alumnos? Alumnos { get; set; }

    public virtual Docentes? Docentes { get; set; }
}
