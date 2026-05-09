import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

export function useAuth(allowedRoles = []) {
    const router = useRouter();
    const [user, setUser] = useState(null);

    useEffect(() => {
        const token = localStorage.getItem('token');
        const role = localStorage.getItem('userRole');

        if (!token) {
            router.push('/login');
            return;
        }

        if (allowedRoles.length > 0 && !allowedRoles.includes(role)) {
            router.push('/login'); // O a una página de "No autorizado"
            return;
        }

        setUser({ token, role });
    }, [router]);

    return user;
}