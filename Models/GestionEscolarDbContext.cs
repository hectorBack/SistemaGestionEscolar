using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EscolarApi.models;

public partial class GestionEscolarDbContext : DbContext
{
    public GestionEscolarDbContext()
    {
    }

    public GestionEscolarDbContext(DbContextOptions<GestionEscolarDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alumnos> Alumnos { get; set; }

    public virtual DbSet<Cursos> Cursos { get; set; }

    public virtual DbSet<Docentes> Docentes { get; set; }

    public virtual DbSet<Inscripciones> Inscripciones { get; set; }

    public virtual DbSet<Materias> Materias { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:BD_ESCOLAR");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumnos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alumnos__3214EC27A7D64E1F");

            entity.HasIndex(e => e.Matricula, "UQ__Alumnos__0FB9FB4FF65BA6B5").IsUnique();

            entity.HasIndex(e => e.UsuarioId, "UQ__Alumnos__2B3DE7B9DE0F2344").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Apellido).HasMaxLength(50);
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.Matricula).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(50);

            entity.HasOne(d => d.Usuario).WithOne(p => p.Alumnos)
                .HasForeignKey<Alumnos>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Alumnos_Usuarios");
        });

        modelBuilder.Entity<Cursos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cursos__3214EC07D2D1D4C7");

            entity.Property(e => e.CicloEscolar).HasMaxLength(20);
            entity.Property(e => e.Horario).HasMaxLength(100);

            entity.HasOne(d => d.Docente).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.DocenteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cursos_Docentes");

            entity.HasOne(d => d.Materia).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cursos_Materias");
        });

        modelBuilder.Entity<Docentes>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Docentes__3214EC27A76B2292");

            entity.HasIndex(e => e.UsuarioId, "UQ__Docentes__2B3DE7B9ADF394B0").IsUnique();

            entity.HasIndex(e => e.NumeroEmpleado, "UQ__Docentes__44F848FD8847E7FB").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Especialidad).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NumeroEmpleado).HasMaxLength(20);

            entity.HasOne(d => d.Usuario).WithOne(p => p.Docentes)
                .HasForeignKey<Docentes>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Docentes_Usuarios");
        });

        modelBuilder.Entity<Inscripciones>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inscripc__3214EC274CD6B1C3");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CalificacionFinal).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.FechaInscripcion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Alumno).WithMany(p => p.Inscripciones)
                .HasForeignKey(d => d.AlumnoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inscripciones_Alumnos");

            entity.HasOne(d => d.Curso).WithMany(p => p.Inscripciones)
                .HasForeignKey(d => d.CursoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inscripciones_Cursos");
        });

        modelBuilder.Entity<Materias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Materias__3214EC27CB3CF7B5");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.MateriaPrerrequisito)
            .WithMany() // Una materia puede ser prerrequisito de muchas otras
            .HasForeignKey(d => d.MateriaPrerrequisitoId)
            .OnDelete(DeleteBehavior.Restrict); // Evita que al borrar una materia se borren las dependientes
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC271B75B542");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D1053495A7E32F").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Rol).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
