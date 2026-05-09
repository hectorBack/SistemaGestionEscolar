'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import { menuItems } from '@/lib/navConfig';
import { useTheme } from '@/components/ThemeProvider';
import './Sidebar.css';

export default function Sidebar({ isCollapsed, setIsCollapsed }) { // Recibe props para sincronizar con el layout
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

    if (!role) return null;
    const currentMenu = menuItems[role] || [];

    return (
        <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
            {/* Botón para esconder/mostrar */}
            <button
                className="toggle-btn"
                onClick={() => setIsCollapsed(!isCollapsed)}
            >
                {isCollapsed ? '→' : '←'}
            </button>

            <div className="sidebar-header">
                {!isCollapsed && <h2 className="sidebar-logo">Escolar Pro</h2>}
                <p className="sidebar-role">{isCollapsed ? role[0] : role}</p>
            </div>

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

            <div className="sidebar-footer">
                <button onClick={toggleTheme} className="footer-btn">
                    <span>{theme === 'light' ? '🌙' : '☀️'}</span>
                    {!isCollapsed && <span>Modo {theme === 'light' ? 'Oscuro' : 'Claro'}</span>}
                </button>
            </div>
        </aside>
    );
}