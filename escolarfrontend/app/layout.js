import './globals.css'; // 1. Primero el CSS global
import ThemeProvider from '@/components/ThemeProvider'; // 2. Luego el Provider

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <body className="antialiased">
        <ThemeProvider>
          {children}
        </ThemeProvider>
      </body>
    </html>
  );
}