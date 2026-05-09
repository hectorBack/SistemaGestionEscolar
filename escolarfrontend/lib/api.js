const API_BASE_URL = 'https://localhost:5063/api';

export const apiFetch = async (endpoint, options = {}) => {
    // En Next.js Client Components usamos localStorage
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers,
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Error en la comunicación con el servidor');
    }

    return response.json();
};