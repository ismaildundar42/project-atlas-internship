import { QueryClient } from '@tanstack/react-query';

/**
 * TanStack Query istemcisi.
 *
 * Uygulama genelinde tek bir QueryClient instance'ı kullanılır.
 * Cache davranışı, retry mantığı ve hata yönetimi burada merkezi olarak yapılandırılır.
 *
 * Bu instance doğrudan import edilerek kullanılabileceği gibi
 * QueryClientProvider üzerinden React context'e de sağlanır.
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Pencere odağa döndüğünde otomatik refetch — production'da açık bırakılabilir.
      refetchOnWindowFocus: false,

      // Hata durumunda en fazla 1 kez dene (development için uygun, production'da artırılabilir).
      retry: 1,

      // Başarılı sorgu verisi 5 dakika boyunca "taze" kabul edilir.
      staleTime: 5 * 60 * 1000,

      // Kullanılmayan cache 10 dakika sonra temizlenir.
      gcTime: 10 * 60 * 1000,
    },
    mutations: {
      // Mutation hataları için global hata handler'ı ileriki aşamada eklenebilir.
      retry: 0,
    },
  },
});

export default queryClient;
