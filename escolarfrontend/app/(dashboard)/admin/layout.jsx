// app/admin/layout.jsx
'use client';
import Sidebar from '@/components/Sidebar';
import { usePathname } from 'next/navigation';

export default function AdminLayout({ children }) {
    const pathname = usePathname();

    // Si la ruta es el login, solo mostramos el contenido (el formulario)
    if (pathname === '/admin/login') {
        return <>{children}</>;
    }

    // Para el resto de las páginas de admin, mostramos el Sidebar
    return (
        <div className="flex">
            <Sidebar />
            <main className="flex-1 ml-64 p-8 bg-gray-50 min-h-screen">
                {children}
            </main>
        </div>
    );
}