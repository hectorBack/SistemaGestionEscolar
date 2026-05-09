'use client'; // Importante: ahora el layout maneja estado
import { useState } from 'react';
import Sidebar from '@/components/Sidebar';

export default function DashboardLayout({ children }) {
    const [isCollapsed, setIsCollapsed] = useState(false);

    return (
        <div className="flex">
            <Sidebar isCollapsed={isCollapsed} setIsCollapsed={setIsCollapsed} />

            <main
                className="flex-1 min-h-screen transition-all duration-300"
                style={{
                    marginLeft: isCollapsed ? '5rem' : '16rem', // Sincronizado con CSS
                    backgroundColor: 'var(--bg-primary)'
                }}
            >
                <div className="p-8">
                    {children}
                </div>
            </main>
        </div>
    );
}