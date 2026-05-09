// middleware.js
import { NextResponse } from 'next/server';

export function middleware(request) {
    // Esto es del lado del servidor, aquí no tenemos acceso a localStorage
    // Pero podemos verificar si existe una cookie de sesión si decides usarlas
    // Por ahora, manejaremos la protección pesada en los componentes con useEffect
    return NextResponse.next();
}