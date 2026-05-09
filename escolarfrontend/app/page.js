// app/page.jsx
'use client';
import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function RootPage() {
  const router = useRouter();

  useEffect(() => {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('userRole');

    if (!token) {
      // URL limpia gracias al Route Group (auth)
      router.push('/login');
    } else {
      // URL limpia gracias al Route Group (dashboard)
      router.push(`/${role.toLowerCase()}`);
    }
  }, [router]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-white">
      <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-blue-600"></div>
    </div>
  );
}