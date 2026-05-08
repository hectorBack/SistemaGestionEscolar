-- 1. Creación de la Base de Datos
CREATE DATABASE GestionEscolarDB;
GO

USE GestionEscolarDB;
GO

-- 2. Tabla de Usuarios (Autenticación)
CREATE TABLE Usuarios (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL, -- Aquí guardarás el hash, no texto plano
    Rol NVARCHAR(20) NOT NULL CHECK (Rol IN ('Admin', 'Docente', 'Alumno')),
    FechaRegistro DATETIME DEFAULT GETDATE()
);

-- Agregamos campos de control de estado y auditoría
ALTER TABLE Usuarios ADD Activo BIT NOT NULL DEFAULT 1;
ALTER TABLE Usuarios ADD UltimoAcceso DATETIME NULL;

-- Opcional: Si quieres manejar intentos fallidos para bloquear cuentas
ALTER TABLE Usuarios ADD IntentosFallidos INT NOT NULL DEFAULT 0;

-- 3. Tabla de Materias
CREATE TABLE Materias (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Creditos INT NOT NULL,
    Descripcion NVARCHAR(MAX)
);

-- 1. Agregar el campo Codigo (primero como nulable para no romper datos existentes)
ALTER TABLE Materias ADD Codigo NVARCHAR(10);

-- 2. (Opcional) Si ya tienes datos, podrías llenarlos aquí. 
-- Si está vacía, procedemos a ponerle el UNIQUE y NOT NULL
ALTER TABLE Materias ALTER COLUMN Codigo NVARCHAR(10) NOT NULL;
ALTER TABLE Materias ADD CONSTRAINT UQ_Materias_Codigo UNIQUE (Codigo);

-- 3. Agregar el campo Activo para borrado lógico
ALTER TABLE Materias ADD Activo BIT NOT NULL DEFAULT 1;

-- 1. Agregar la columna del prerrequisito
ALTER TABLE Materias 
ADD MateriaPrerrequisitoId INT NULL;

-- 2. Configurar la relación de llave foránea hacia la misma tabla
ALTER TABLE Materias 
ADD CONSTRAINT FK_Materias_Prerrequisito 
FOREIGN KEY (MateriaPrerrequisitoId) REFERENCES Materias(ID);

-- 1. Insertamos una inscripción aprobada para Programación Web (ID 1)
-- (Usamos el CursoId 2 que es de Programación Web)
INSERT INTO Inscripciones (AlumnoId, CursoId, FechaInscripcion, Estatus, CalificacionFinal, Activo)
VALUES (2, 2, '2025-12-15', 'Finalizado', 90, 1);

-- 4. Tabla de Alumnos
CREATE TABLE Alumnos (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Matricula NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    UsuarioId INT UNIQUE, -- Relación 1:1 con Usuarios
    CONSTRAINT FK_Alumnos_Usuarios FOREIGN KEY (UsuarioId) 
        REFERENCES Usuarios(ID) ON DELETE CASCADE
);

ALTER TABLE Alumnos
ADD Activo BIT NOT NULL DEFAULT 1;

ALTER TABLE Alumnos
ADD Genero NVARCHAR(20) NULL; -- Puede ser 'Masculino', 'Femenino', 'Otro'

-- 5. Tabla de Docentes
CREATE TABLE Docentes (
    ID INT PRIMARY KEY IDENTITY(1,1),
    NumeroEmpleado NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Especialidad NVARCHAR(100),
    UsuarioId INT UNIQUE, -- Relación 1:1 con Usuarios
    CONSTRAINT FK_Docentes_Usuarios FOREIGN KEY (UsuarioId) 
        REFERENCES Usuarios(ID) ON DELETE CASCADE
);

-- 1. Agregamos los campos faltantes
ALTER TABLE Docentes ADD Apellido NVARCHAR(100) NOT NULL DEFAULT '';
ALTER TABLE Docentes ADD FechaContratacion DATE NOT NULL DEFAULT GETDATE();
ALTER TABLE Docentes ADD Activo BIT NOT NULL DEFAULT 1;

-- 2. Ajustamos el tamaño del Nombre si es necesario (tú tenías 100, está bien)
-- 3. Quitamos el DEFAULT si quieres que sean obligatorios manualmente a partir de ahora

-- 6. Tabla de Cursos (Relación Materia - Docente)
CREATE TABLE Cursos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MateriaId INT NOT NULL,
    DocenteId INT NOT NULL,
    CicloEscolar NVARCHAR(20) NOT NULL, -- Ejemplo: "2024-1"
    Horario NVARCHAR(100),
    CONSTRAINT FK_Cursos_Materias FOREIGN KEY (MateriaId) REFERENCES Materias(ID),
    CONSTRAINT FK_Cursos_Docentes FOREIGN KEY (DocenteId) REFERENCES Docentes(ID)
);

ALTER TABLE Cursos ADD CupoMaximo INT NOT NULL DEFAULT 30;
ALTER TABLE Cursos ADD CupoDisponible INT NOT NULL DEFAULT 30;
ALTER TABLE Cursos ADD Aula NVARCHAR(50);
ALTER TABLE Cursos ADD Activo BIT NOT NULL DEFAULT 1;

-- 7. Tabla de Inscripciones (Relación Alumno - Curso)
CREATE TABLE Inscripciones (
    ID INT PRIMARY KEY IDENTITY(1,1),
    AlumnoId INT NOT NULL,
    CursoId INT NOT NULL,
    FechaInscripcion DATETIME DEFAULT GETDATE(),
    CalificacionFinal DECIMAL(4,2) NULL, -- Permite nulos hasta que se asigne nota
    CONSTRAINT FK_Inscripciones_Alumnos FOREIGN KEY (AlumnoId) REFERENCES Alumnos(ID),
    CONSTRAINT FK_Inscripciones_Cursos FOREIGN KEY (CursoId) REFERENCES Cursos(ID)
);

ALTER TABLE Inscripciones ADD Estatus NVARCHAR(20) NOT NULL DEFAULT 'Activo';
ALTER TABLE Inscripciones ADD Activo BIT NOT NULL DEFAULT 1;

-- Si quieres asegurar que FechaInscripcion sea NOT NULL como en la propuesta:
ALTER TABLE Inscripciones ALTER COLUMN FechaInscripcion DATETIME NOT NULL;

CREATE TABLE Asistencias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InscripcionId INT NOT NULL, -- Relaciona al Alumno y al Curso de un solo golpe
    Fecha DATE NOT NULL DEFAULT GETDATE(),
    Estatus VARCHAR(20) NOT NULL, -- 'Asistencia', 'Falta', 'Retardo', 'Justificada'
    Observaciones NVARCHAR(200) NULL,
    
    -- Llave foránea hacia tu tabla de Inscripciones
    CONSTRAINT FK_Asistencias_Inscripciones FOREIGN KEY (InscripcionId) 
    REFERENCES Inscripciones(Id)
);