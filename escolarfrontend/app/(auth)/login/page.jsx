'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiFetch } from '@/lib/api';
import './login.css'; // <--- IMPORTANTE: Importa tu CSS aquí

export default function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const data = await apiFetch('/Usuarios/login', {
                method: 'POST',
                body: JSON.stringify({ email, password })
            });

            localStorage.setItem('token', data.token);
            localStorage.setItem('userRole', data.rol);

            const targetPath = `/${data.rol.toLowerCase()}`;
            router.replace(targetPath);

        } catch (err) {
            setError(err.message || 'Credenciales inválidas');
        } finally {
            setLoading(false);
        }
    };

    return (
        /* Usamos las clases de tu login.css */
        <div className="login-container">
            <div className="login-card">

                <header className="login-header">
                    <h1 className="text-2xl font-bold">Sistema Escolar</h1>
                    <p className="opacity-80 text-sm">Gestión Académica</p>
                </header>

                <form onSubmit={handleSubmit} className="login-form">
                    {error && (
                        <div style={{
                            backgroundColor: '#fee2e2',
                            color: '#dc2626',
                            padding: '0.75rem',
                            borderRadius: '8px',
                            marginBottom: '1rem',
                            fontSize: '0.875rem',
                            border: '1px solid #fecaca'
                        }}>
                            <strong>Error:</strong> {error}
                        </div>
                    )}

                    <div className="form-group">
                        <label>Correo Electrónico</label>
                        <input
                            type="email"
                            required
                            placeholder="ejemplo@correo.com"
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    <div className="form-group">
                        <label>Contraseña</label>
                        <input
                            type="password"
                            required
                            placeholder="••••••••"
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="btn-login"
                    >
                        {loading ? 'Verificando...' : 'Iniciar Sesión'}
                    </button>

                    <div style={{ textAlign: 'center', marginTop: '1.5rem' }}>
                        <a href="#" style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>
                            ¿Olvidaste tu contraseña?
                        </a>
                    </div>
                </form>
            </div>
        </div>
    );
}