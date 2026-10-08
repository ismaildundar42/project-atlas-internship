import { useState, type ImgHTMLAttributes } from 'react';
import { FolderKanban } from 'lucide-react';

interface ImageWithFallbackProps extends ImgHTMLAttributes<HTMLImageElement> {
  fallbackText?: string;
}

export function ImageWithFallback({
  src,
  alt,
  fallbackText,
  className = '',
  ...props
}: ImageWithFallbackProps) {
  const [hasError, setHasError] = useState(!src);

  // Safely detect generic external Unsplash mock URLs to prefer intentional corporate fallback
  const isGenericMockUrl = Boolean(src && src.includes('images.unsplash.com'));
  const shouldShowFallback = hasError || !src || isGenericMockUrl;

  if (shouldShowFallback) {
    return (
      <div className="project-card__fallback-media" role="img" aria-label={alt || 'Demir Export Ar-Ge Projesi'}>
        <div className="project-card__fallback-icon-wrapper">
          <FolderKanban size={32} strokeWidth={1.5} className="project-card__fallback-icon" />
        </div>
        <span className="project-card__fallback-label">
          {fallbackText || 'Demir Export Ar-Ge'}
        </span>
      </div>
    );
  }

  return (
    <img
      src={src}
      alt={alt}
      className={className}
      onError={() => setHasError(true)}
      {...props}
    />
  );
}

export default ImageWithFallback;
