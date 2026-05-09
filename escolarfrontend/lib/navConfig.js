export const menuItems = {
    Admin: [
        { name: 'Dashboard', path: '/admin', icon: '📊' },
        { name: 'Gestión Alumnos', path: '/admin/alumnos', icon: '👥' },
        { name: 'Gestión Docentes', path: '/admin/docentes', icon: '👨‍🏫' },
        { name: 'Materias y Cursos', path: '/admin/cursos', icon: '📚' },
    ],
    Docente: [
        { name: 'Mis Cursos', path: '/docente', icon: '📖' },
        { name: 'Calificar', path: '/docente/calificaciones', icon: '📝' },
        { name: 'Pasar Lista', path: '/docente/asistencia', icon: '✅' },
    ],
    Alumno: [
        { name: 'Mi Horario', path: '/alumno', icon: '📅' },
        { name: 'Inscripciones', path: '/alumno/inscripciones', icon: '✏️' },
        { name: 'Mis Calificaciones', path: '/alumno/record', icon: '🎓' },
    ],
};