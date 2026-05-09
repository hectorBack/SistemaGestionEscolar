import ThemeProvider from '@/components/ThemeProvider';
import './globals.css';

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