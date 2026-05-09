'use client';
import { useEffect, useState } from 'react';
import { apiFetch } from '@/lib/api';

export default function InscripcionPage() {
    const [cursos, setCursos] = useState([]);

    useEffect(() => {
        // Cargar cursos disponibles desde el backend de .NET
        apiFetch('/Cursos').then(setCursos).catch(console.error);
    }, []);

    const inscribirse = async (cursoId) => {
        try {
            await apiFetch('/Inscripciones', {
                method: 'POST',
                body: JSON.stringify({ cursoId })
            });
            alert('¡Inscrito con éxito!');
        } catch (err) {
            alert(err.message); // Aquí saldría el "No hay cupo" que testeamos
        }
    };

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-4">Cursos Disponibles</h1>
            <div className="grid gap-4">
                {cursos.map(curso => (
                    <div key={curso.id} className="border p-4 rounded flex justify-between shadow-sm">
                        <div>
                            <p className="font-bold text-blue-800">{curso.materiaNombre}</p>
                            <p className="text-sm text-gray-600">{curso.diaSemana} | {curso.horaInicio} - {curso.horaFin}</p>
                            <p className="text-xs">Cupo: {curso.cupoDisponible}</p>
                        </div>
                        <button
                            onClick={() => inscribirse(curso.id)}
                            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
                        >
                            Inscribirme
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
}