'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import { menuItems } from '@/lib/navConfig';
import { useTheme } from '@/components/ThemeProvider';
import './Sidebar.css';

export default function Sidebar({ isCollapsed, setIsCollapsed }) {
    const [role, setRole] = useState(null);
    const { theme, toggleTheme } = useTheme();
    const router = useRouter();
    const pathname = usePathname();

    useEffect(() => {
        const savedRole = localStorage.getItem('userRole');
        if (!savedRole) {
            router.push('/login');
        } else {
            setRole(savedRole);
        }
    }, [router]);

    const handleLogout = () => {
        localStorage.removeItem('userRole');
        // Si manejas tokens de tu backend en Java, bórralo aquí:
        // localStorage.removeItem('token'); 
        router.push('/login');
    };

    if (!role) return null;
    const currentMenu = menuItems[role] || [];

    return (
        <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
            {/* Botón Toggle para expandir/contraer */}
            <button
                className="toggle-btn"
                onClick={() => setIsCollapsed(!isCollapsed)}
                aria-label="Toggle Sidebar"
            >
                {isCollapsed ? '→' : '←'}
            </button>

            {/* Cabecera: Logo y Rol */}
            <div className="sidebar-header">
                {!isCollapsed && <h2 className="sidebar-logo">Escolar Pro</h2>}
                <p className="sidebar-role">{isCollapsed ? role[0] : role}</p>
            </div>

            {/* Navegación dinámica según el rol */}
            <nav className="sidebar-nav">
                {currentMenu.map((item) => {
                    const isActive = pathname === item.path;
                    return (
                        <Link
                            key={item.path}
                            href={item.path}
                            className={`nav-item ${isActive ? 'active' : ''}`}
                            title={isCollapsed ? item.name : ''}
                        >
                            <span className="nav-icon">{item.icon}</span>
                            {!isCollapsed && <span>{item.name}</span>}
                        </Link>
                    );
                })}
            </nav>

            {/* Footer: Configuración y Logout */}
            <div className="sidebar-footer">
                {/* Selector de Tema */}
                <button onClick={toggleTheme} className="footer-btn" title="Cambiar tema">
                    <span className="nav-icon">{theme === 'light' ? '🌙' : '☀️'}</span>
                    {!isCollapsed && <span>Modo {theme === 'light' ? 'Oscuro' : 'Claro'}</span>}
                </button>

                {/* Botón Cerrar Sesión */}
                <button onClick={handleLogout} className="footer-btn btn-logout" title="Cerrar sesión">
                    <span className="nav-icon">🚪</span>
                    {!isCollapsed && <span>Cerrar Sesión</span>}
                </button>
            </div>
        </aside>
    );
}