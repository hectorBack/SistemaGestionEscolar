'use client';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { apiFetch } from '@/lib/api';
import './login.css';

export default function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [fieldErrors, setFieldErrors] = useState({ email: false, password: false });

    // Nueva mejora: Estado para detectar Mayúsculas (Caps Lock)
    const [capsLockActive, setCapsLockActive] = useState(false);

    const router = useRouter();

    const validateEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

    // Función para detectar si Bloq Mayús está activo
    const checkCapsLock = (e) => {
        if (e.getModifierState('CapsLock')) {
            setCapsLockActive(true);
        } else {
            setCapsLockActive(false);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setFieldErrors({ email: false, password: false });

        const cleanEmail = email.trim();
        let errors = { email: false, password: false };
        let hasError = false;

        if (!validateEmail(cleanEmail)) {
            setError('Formato de correo electrónico no válido');
            errors.email = true;
            hasError = true;
        }

        if (password.length < 4) {
            setError('La contraseña es demasiado corta');
            errors.password = true;
            hasError = true;
        }

        if (hasError) {
            setFieldErrors(errors);
            return;
        }

        setLoading(true);

        try {
            const data = await apiFetch('/Usuarios/login', {
                method: 'POST',
                body: JSON.stringify({ email: cleanEmail, password })
            });

            localStorage.setItem('token', data.token);
            localStorage.setItem('userRole', data.rol);
            router.replace(`/${data.rol.toLowerCase()}`);

        } catch (err) {
            setError(err.message || 'Credenciales incorrectas');
            setFieldErrors({ email: true, password: true });
        } finally {
            setLoading(false);
        }
    };

    // Mejora: El botón se deshabilita si los campos están vacíos o si está cargando
    const isFormEmpty = !email || !password;

    return (
        <div className="login-container">
            <div className="login-card">
                <header className="login-header">
                    <h1 className="text-2xl font-bold">Sistema Escolar</h1>
                    <p className="opacity-80 text-sm">Gestión Académica</p>
                </header>

                <form onSubmit={handleSubmit} className="login-form" noValidate>
                    {error && (
                        <div className="error-message">
                            <strong>⚠️</strong> {error}
                        </div>
                    )}

                    <div className="form-group">
                        <label>Correo Electrónico</label>
                        <input
                            type="email"
                            value={email}
                            autoFocus // <--- MEJORA 1: Auto-focus al cargar
                            placeholder=""
                            onChange={(e) => setEmail(e.target.value)}
                            className={fieldErrors.email ? 'input-error' : ''}
                        />
                    </div>

                    <div className="form-group">
                        <label>Contraseña</label>
                        <div className="password-wrapper">
                            <input
                                type={showPassword ? "text" : "password"}
                                value={password}
                                placeholder="••••••••"
                                onChange={(e) => setPassword(e.target.value)}
                                onKeyUp={checkCapsLock}
                                onKeyDown={checkCapsLock}
                                className={fieldErrors.password ? 'input-error' : ''}
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="toggle-password"
                                title={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
                            >
                                {showPassword ? '👁️‍🗨️' : '👁️'}
                            </button>
                        </div>

                        {capsLockActive && (
                            <p className="caps-lock-warning">
                                💡 Bloq Mayús está activo
                            </p>
                        )}
                    </div>

                    <button
                        type="submit"
                        disabled={loading || isFormEmpty} // <--- MEJORA 3: Botón inteligente
                        className="btn-login"
                        style={{
                            cursor: (loading || isFormEmpty) ? 'not-allowed' : 'pointer',
                            opacity: (loading || isFormEmpty) ? 0.6 : 1,
                            filter: loading ? 'brightness(0.8)' : 'none',
                            transition: 'all 0.3s ease'
                        }}
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