'use client';
import { useEffect, useState } from 'react';
import './admin.css'; // Importamos el diseño compartido

export default function AdminDashboard() {
    const [stats, setStats] = useState({ alumnos: 250, docentes: 45, materias: 12 });

    useEffect(() => {
        // Aquí conectarás con tu API de .NET más adelante
        console.log("Admin Dashboard cargado con estilos unificados");
    }, []);

    return (
        <div className="admin-container">
            <h1 className="admin-title">
                Panel de Administración
            </h1>

            {/* Tarjetas de Resumen usando el sistema de admin.css */}
            <div className="stats-grid">
                <div className="stat-card">
                    <h3 className="stat-label">Total Alumnos</h3>
                    <p className="stat-value">{stats.alumnos}</p>
                </div>

                <div className="stat-card">
                    <h3 className="stat-label">Docentes Activos</h3>
                    <p className="stat-value">{stats.docentes}</p>
                </div>

                <div className="stat-card">
                    <h3 className="stat-label">Materias</h3>
                    <p className="stat-value">{stats.materias}</p>
                </div>
            </div>

            {/* Espacio para futuras tablas o gráficos */}
            <div className="stat-card" style={{ marginTop: '2rem', height: '200px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <p style={{ color: 'var(--text-muted)' }}>Próximamente: Gráficos de rendimiento escolar</p>
            </div>
        </div>
    );
}