import React, { useEffect, useRef } from 'react';
import { resolveResourceUrl } from '../../utils/urlUtils';
import type { ProjectMediaItem } from '../../types/project';

interface ImageLightboxProps {
  images: ProjectMediaItem[];
  currentIndex: number;
  isOpen: boolean;
  onClose: () => void;
  onNavigate: (index: number) => void;
  triggerElementRef?: React.RefObject<HTMLElement | null>;
}

export const ImageLightbox: React.FC<ImageLightboxProps> = ({
  images,
  currentIndex,
  isOpen,
  onClose,
  onNavigate,
  triggerElementRef,
}) => {
  const closeBtnRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    // Focus close button on open for accessibility
    closeBtnRef.current?.focus();

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      } else if (e.key === 'ArrowLeft') {
        const prevIndex = (currentIndex - 1 + images.length) % images.length;
        onNavigate(prevIndex);
      } else if (e.key === 'ArrowRight') {
        const nextIndex = (currentIndex + 1) % images.length;
        onNavigate(nextIndex);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      // Return focus to trigger element when closed
      triggerElementRef?.current?.focus();
    };
  }, [isOpen, currentIndex, images.length, onClose, onNavigate, triggerElementRef]);

  if (!isOpen || images.length === 0) return null;

  const currentItem = images[currentIndex] || images[0];
  const fullUrl = resolveResourceUrl(currentItem.fileUrl);
  const titleText = currentItem.caption || currentItem.altText || currentItem.fileName;

  const handlePrev = (e: React.MouseEvent) => {
    e.stopPropagation();
    const prevIndex = (currentIndex - 1 + images.length) % images.length;
    onNavigate(prevIndex);
  };

  const handleNext = (e: React.MouseEvent) => {
    e.stopPropagation();
    const nextIndex = (currentIndex + 1) % images.length;
    onNavigate(nextIndex);
  };

  return (
    <div
      className="lightbox-overlay"
      role="dialog"
      aria-modal="true"
      aria-label="Görsel Galerisi"
      onClick={onClose}
    >
      <div className="lightbox-container" onClick={(e) => e.stopPropagation()}>
        {/* Header bar */}
        <div className="lightbox-header">
          <span className="lightbox-counter">
            {currentIndex + 1} / {images.length}
          </span>
          <button
            ref={closeBtnRef}
            type="button"
            className="lightbox-close-btn"
            onClick={onClose}
            aria-label="Kapat"
          >
            ✕
          </button>
        </div>

        {/* Main image content area */}
        <div className="lightbox-content">
          {images.length > 1 && (
            <button
              type="button"
              className="lightbox-nav-btn lightbox-prev"
              onClick={handlePrev}
              aria-label="Önceki Görsel"
            >
              ‹
            </button>
          )}

          <div className="lightbox-image-wrapper">
            <img
              src={fullUrl}
              alt={currentItem.altText || currentItem.fileName || 'Proje Görseli'}
              className="lightbox-image"
            />
          </div>

          {images.length > 1 && (
            <button
              type="button"
              className="lightbox-nav-btn lightbox-next"
              onClick={handleNext}
              aria-label="Sonraki Görsel"
            >
              ›
            </button>
          )}
        </div>

        {/* Caption/Title Footer */}
        {titleText && (
          <div className="lightbox-footer">
            <p className="lightbox-caption">{titleText}</p>
          </div>
        )}
      </div>
    </div>
  );
};
