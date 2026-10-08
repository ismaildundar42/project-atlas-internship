import axios from 'axios';

/**
 * Merkezi Axios API istemcisi.
 *
 * Tüm API çağrıları bu instance üzerinden yapılmalıdır.
 * Böylece:
 * - Base URL tek noktadan yönetilir
 * - Authorization header'ı ileride buraya eklenir
 * - Global error handling interceptor'lar buraya tanımlanır
 * - Request/response dönüşümleri merkezi olarak yönetilir
 */
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
  timeout: 30000, // 30 saniye
});

// ─── Request Interceptor ────────────────────────────────────────────────────
// İleride authentication eklendiğinde Authorization header'ı buraya eklenir.
// Örnek:
// apiClient.interceptors.request.use((config) => {
//   const token = getAuthToken();
//   if (token) {
//     config.headers.Authorization = `Bearer ${token}`;
//   }
//   return config;
// });

// ─── Response Interceptor ───────────────────────────────────────────────────
// Global hata yönetimi: 401 → login yönlendirmesi, 500 → hata bildirimi vb.
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Geliştirme aşamasında hataları console'a yaz.
    // Production'da bu kısım bir monitoring servisine (örn. Sentry) raporlanabilir.
    if (import.meta.env.DEV) {
      console.error('[API Error]', error.response?.status, error.response?.data ?? error.message);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
