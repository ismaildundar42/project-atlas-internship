/// <reference types="vite/client" />

/**
 * Vite tarafından expose edilen environment variable'ların TypeScript tip tanımları.
 * Yalnızca VITE_ prefix'li değişkenler bundle'a dahil edilir.
 * Bu interface, auto-complete ve tip güvenliği sağlar.
 */
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_APP_NAME: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
