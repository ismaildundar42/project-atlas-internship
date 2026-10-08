import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import queryClient from './app/queryClient';
import router from './routes/router';
import { AuthProvider } from './context/AuthContext';
import './i18n';
import './styles/global.css';

/**
 * Uygulama giriş noktası.
 *
 * Provider sıralaması:
 * 1. StrictMode — development'ta çift render ile yan etkileri tespit eder
 * 2. QueryClientProvider — TanStack Query bağlamı
 * 3. AuthProvider — ASP.NET Core Identity oturum bağlamı
 * 4. RouterProvider — React Router bağlamı
 */
const rootElement = document.getElementById('root');

if (!rootElement) {
  throw new Error(
    'Root element bulunamadı. index.html dosyasında id="root" olan bir element olmalıdır.'
  );
}

createRoot(rootElement).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>
);
