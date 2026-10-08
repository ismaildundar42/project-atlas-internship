/**
 * Resource URL Resolver Helper
 *
 * Resolves static uploaded files (/uploads/...) to the API backend origin.
 * Leaves absolute URLs (http://, https://) unchanged.
 */
export function resolveResourceUrl(url?: string | null): string {
  if (!url) return '';
  const trimmed = url.trim();
  if (!trimmed) return '';

  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    return trimmed;
  }

  // Relative path starting with /uploads/ or /, append API backend origin
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
  const cleanBase = apiBaseUrl.replace(/\/+$/, '');
  const cleanPath = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;

  return `${cleanBase}${cleanPath}`;
}

/**
 * External URL Normalizer Helper
 *
 * Normalizes external user-entered URLs (applicationUrl, repositoryUrl)
 * so they navigate correctly to external destinations without falling into Vite/React Router.
 */
export function normalizeExternalUrl(url?: string | null): string {
  if (!url) return '';
  const trimmed = url.trim();
  if (!trimmed) return '';

  // Prevent dangerous schemes
  if (/^(javascript|data|vbscript):/i.test(trimmed)) {
    return '';
  }

  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    return trimmed;
  }

  // Default missing protocol to https://
  return `https://${trimmed}`;
}

/**
 * Standard HTTP / Axios / ASP.NET Core Error Message Extractor
 * Handles ASP.NET Core DataAnnotations / ProblemDetails errors dictionary ({ errors: { Field: ["Msg"] } })
 * as well as custom API exception messages ({ message: "Msg" }) and raw string responses.
 */
export function extractErrorMessage(
  err: any,
  fallbackMessage: string = 'Proje kaydedilirken bir sorun oluştu. Lütfen bilgileri kontrol edip tekrar deneyin.'
): string {
  if (!err) return fallbackMessage;

  const data = err.response?.data;
  let rawMsg: string | null = null;

  if (data) {
    if (typeof data === 'string' && data.trim()) {
      rawMsg = data.trim();
    } else if (data.detail && typeof data.detail === 'string' && data.detail.trim()) {
      rawMsg = data.detail.trim();
    } else if (data.message && typeof data.message === 'string' && data.message.trim()) {
      rawMsg = data.message.trim();
    } else if (data.errors && typeof data.errors === 'object') {
      const messages: string[] = [];
      for (const key of Object.keys(data.errors)) {
        const fieldErrors = data.errors[key];
        if (Array.isArray(fieldErrors)) {
          fieldErrors.forEach((msg) => {
            if (typeof msg === 'string' && msg.trim()) messages.push(msg.trim());
          });
        } else if (typeof fieldErrors === 'string' && fieldErrors.trim()) {
          messages.push(fieldErrors.trim());
        }
      }
      if (messages.length > 0) {
        rawMsg = messages.join(' • ');
      }
    } else if (data.title && typeof data.title === 'string' && data.title !== 'One or more validation errors occurred.') {
      rawMsg = data.title.trim();
    }
  }

  if (!rawMsg && err.message && typeof err.message === 'string' && err.message.trim()) {
    rawMsg = err.message.trim();
  }

  if (!rawMsg) {
    return fallbackMessage;
  }

  // Intercept and sanitize technical C#/ASP.NET/System.Text.Json stack traces or error internals
  const isTechnicalError =
    /System\.|Nullable|DateOnly|JSON value could not be converted|BytePositionInLine|LineNumber|Exception|stack trace|Object reference|status code/i.test(
      rawMsg
    );

  if (isTechnicalError) {
    if (/DateOnly|date/i.test(rawMsg)) {
      return 'Lütfen geçerli bir tarih formatı giriniz veya tarih alanlarını boş bırakınız.';
    }
    return fallbackMessage;
  }

  // Clean up duplicate slug message if present
  if (rawMsg.includes('slug adresi başka bir proje tarafından kullanılmaktadır')) {
    return 'Bu URL adresi (slug) başka bir proje tarafından kullanılıyor. Lütfen farklı bir slug belirleyin.';
  }

  return rawMsg;
}

