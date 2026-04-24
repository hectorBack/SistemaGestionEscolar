using System;
using System.Collections.Generic;

namespace EscolarApi.models;

public partial class Alumnos
{
    public int Id { get; set; }

    public string Matricula { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string? Genero { get; set; }

    public bool Activo { get; set; }

    public int? UsuarioId { get; set; }

    public virtual ICollection<Inscripciones> Inscripciones { get; set; } = new List<Inscripciones>();

    public virtual Usuarios? Usuario { get; set; }
}
