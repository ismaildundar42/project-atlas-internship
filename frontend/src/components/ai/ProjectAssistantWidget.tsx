import React, { useState, useRef, useEffect, type FormEvent, type KeyboardEvent } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Bot,
  Send,
  Minus,
  Maximize2,
  Minimize2,
  X,
  Sparkles,
  ExternalLink,
  ShieldCheck,
  AlertCircle,
  Clock,
  Layers,
  MapPin,
  Trash2,
  GripVertical,
} from 'lucide-react';
import { useAssistant } from '../../context/AssistantContext';
import SafeMarkdown from './SafeMarkdown';
import Spinner from '../ui/Spinner';
import Badge from '../ui/Badge';
import type { ProjectAssistantCitation } from '../../types/assistant';

const STORAGE_KEY = 'demir-export-assistant-position';
const HEADER_SAFE_ZONE = 75; // Top safe margin in pixels
const BOTTOM_SAFE_ZONE = 70; // Bottom safe margin in pixels

function getInitialPosition(): number {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const parsed = parseFloat(saved);
      if (!isNaN(parsed) && parsed > 0 && parsed <= 1) {
        return Math.max(
          HEADER_SAFE_ZONE,
          Math.min(window.innerHeight - BOTTOM_SAFE_ZONE, Math.round(parsed * window.innerHeight))
        );
      }
    }
  } catch {
    // fallback
  }
  // Default: Bottom-right pinned (en sağ alta dayalı)
  return Math.max(
    HEADER_SAFE_ZONE,
    window.innerHeight - BOTTOM_SAFE_ZONE
  );
}

export const ProjectAssistantWidget: React.FC = () => {
  const { t } = useTranslation(['assistant', 'common']);
  const {
    displayState,
    openCompact,
    openExpanded,
    collapseAssistant,
    messages,
    isLoading,
    sendMessage,
    clearConversation,
    suggestedPrompts,
  } = useAssistant();

  const [inputVal, setInputVal] = useState('');
  const [localError, setLocalError] = useState<string | null>(null);
  // 0 = analyzing, 1 = finding, 2 = reviewing, 3 = preparing response
  const [loadingPhase, setLoadingPhase] = useState<0 | 1 | 2 | 3>(0);

  // Draggable position state (vertical pixels from top)
  const [launcherTop, setLauncherTop] = useState<number>(getInitialPosition);
  const [isDragging, setIsDragging] = useState(false);
  const dragRef = useRef<{ startY: number; startTop: number; hasMoved: boolean }>({
    startY: 0,
    startTop: 0,
    hasMoved: false,
  });

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const launcherRef = useRef<HTMLButtonElement>(null);

  // Auto scroll to bottom
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  // Re-clamp on window resize
  useEffect(() => {
    const handleResize = () => {
      setLauncherTop((prev) => {
        const maxTop = window.innerHeight - BOTTOM_SAFE_ZONE;
        return Math.max(HEADER_SAFE_ZONE, Math.min(maxTop, prev));
      });
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    if (displayState !== 'collapsed') {
      scrollToBottom();
      setTimeout(() => inputRef.current?.focus(), 150);
    }
  }, [displayState, messages, isLoading]);

  // Progressive loading phase: step 1 -> step 2 (2.5s) -> step 3 (5.0s) -> step 4 (7.5s)
  useEffect(() => {
    if (!isLoading) {
      setLoadingPhase(0);
      return;
    }
    setLoadingPhase(0);
    const t1 = setTimeout(() => setLoadingPhase(1), 2500);
    const t2 = setTimeout(() => setLoadingPhase(2), 5000);
    const t3 = setTimeout(() => setLoadingPhase(3), 7500);
    return () => {
      clearTimeout(t1);
      clearTimeout(t2);
      clearTimeout(t3);
    };
  }, [isLoading]);

  // Handle ESC key to minimize or collapse
  useEffect(() => {
    const handleKeyDown = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (displayState === 'expanded') {
          openCompact();
        } else if (displayState === 'compact') {
          collapseAssistant();
          launcherRef.current?.focus();
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [displayState, openCompact, collapseAssistant]);

  const handlePointerDown = (e: React.PointerEvent<HTMLButtonElement>) => {
    if (e.button !== 0) return; // Only primary mouse button or touch
    dragRef.current = {
      startY: e.clientY,
      startTop: launcherTop,
      hasMoved: false,
    };
    (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
  };

  const handlePointerMove = (e: React.PointerEvent<HTMLButtonElement>) => {
    if (!e.currentTarget.hasPointerCapture(e.pointerId)) return;
    const deltaY = e.clientY - dragRef.current.startY;
    if (Math.abs(deltaY) > 5) {
      dragRef.current.hasMoved = true;
      setIsDragging(true);
      const maxTop = window.innerHeight - BOTTOM_SAFE_ZONE;
      const newTop = Math.max(HEADER_SAFE_ZONE, Math.min(maxTop, dragRef.current.startTop + deltaY));
      setLauncherTop(newTop);
    }
  };

  const handlePointerUp = (e: React.PointerEvent<HTMLButtonElement>) => {
    if (e.currentTarget.hasPointerCapture(e.pointerId)) {
      e.currentTarget.releasePointerCapture(e.pointerId);
    }
    if (dragRef.current.hasMoved) {
      setIsDragging(false);
      // Persist vertical ratio relative to viewport height
      const ratio = launcherTop / window.innerHeight;
      try {
        localStorage.setItem(STORAGE_KEY, ratio.toFixed(4));
      } catch {
        // ignore
      }
    } else {
      setIsDragging(false);
      openCompact();
    }
  };

  const handleSend = async (textToSend?: string) => {
    const query = (textToSend ?? inputVal).trim();
    if (!query) {
      setLocalError(t('assistant.validation.questionRequired', 'Lütfen bir soru yazın.'));
      return;
    }

    if (query.length < 3) {
      setLocalError(t('assistant.validation.questionMinLength', 'Soru en az 3 karakter olmalıdır.'));
      return;
    }

    if (query.length > 1000) {
      setLocalError(t('assistant.validation.questionMaxLength', 'Soru en fazla 1000 karakter olabilir.'));
      return;
    }

    setLocalError(null);
    setInputVal('');
    await sendMessage(query);
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (!isLoading) {
        handleSend();
      }
    }
  };

  // ─── 1. COLLAPSED STATE (Draggable Edge Launcher) ───────────────────────────
  if (displayState === 'collapsed') {
    return (
      <div
        style={{
          position: 'fixed',
          top: `${launcherTop}px`,
          right: '24px',
          zIndex: 999,
          touchAction: 'none',
          userSelect: 'none',
        }}
      >
        <button
          ref={launcherRef}
          type="button"
          onPointerDown={handlePointerDown}
          onPointerMove={handlePointerMove}
          onPointerUp={handlePointerUp}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              openCompact();
            }
          }}
          aria-label={t('assistant.openAssistant', 'Proje Asistanını aç')}
          title={t('assistant.title', 'Proje Asistanı (Konumunu ayarlamak için yukarı/aşağı sürükleyebilirsiniz)')}
          className="de-assistant-launcher"
          style={{
            cursor: isDragging ? 'grabbing' : 'grab',
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            padding: '8px 14px 8px 8px',
            borderRadius: '24px',
            backgroundColor: 'var(--color-brand-navy)',
            color: '#ffffff',
            border: '1px solid rgba(255, 255, 255, 0.2)',
            boxShadow: isDragging
              ? '0 8px 24px rgba(0, 0, 0, 0.35)'
              : '0 4px 16px rgba(0, 0, 0, 0.22), 0 2px 4px rgba(0, 0, 0, 0.12)',
            transition: isDragging ? 'none' : 'box-shadow 0.2s ease, transform 0.2s ease',
            transform: isDragging ? 'scale(1.04)' : 'none',
          }}
        >
          <GripVertical size={14} style={{ color: 'rgba(255, 255, 255, 0.65)', flexShrink: 0 }} />
          <div className="de-assistant-launcher__icon-box">
            <Bot size={16} aria-hidden="true" />
          </div>
          <span className="assistant-launcher-label" style={{ fontWeight: 600, fontSize: '13px' }}>
            {t('assistant.title', 'Proje Asistanı')}
          </span>
        </button>
      </div>
    );
  }

  // ─── 2. COMPACT & 3. EXPANDED STATES ────────────────────────────────────────
  const isExpanded = displayState === 'expanded';

  return (
    <>
      {/* Expanded Modal Backdrop */}
      {isExpanded && (
        <div
          onClick={openCompact}
          aria-hidden="true"
          className="de-assistant-backdrop"
        />
      )}

      {/* Main Container */}
      <div
        role="dialog"
        aria-modal={isExpanded}
        aria-label={t('assistant.title', 'Proje Asistanı')}
        className={`de-assistant-panel ${
          isExpanded ? 'de-assistant-panel--expanded' : 'de-assistant-panel--compact'
        }`}
      >
        {/* ─── Header ─── */}
        <div className="de-assistant-header">
          <div className="de-assistant-header__title-group">
            <div className="de-assistant-header__icon-box">
              <Bot size={isExpanded ? 20 : 18} aria-hidden="true" />
            </div>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <h3 className="de-assistant-header__title">
                  {t('assistant.title', 'Proje Asistanı')}
                </h3>
                {isExpanded && (
                  <Badge variant="navy" size="sm">
                    {t('assistant.subtitleShort', 'Proje Kütüphanesi Bilgi Asistanı')}
                  </Badge>
                )}
              </div>
              {!isExpanded && (
                <span className="de-assistant-header__subtitle">
                  {t('assistant.subtitleShort', 'Proje Kütüphanesi Bilgi Asistanı')}
                </span>
              )}
            </div>
          </div>

          {/* Header Actions */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
            {messages.length > 0 && (
              <button
                type="button"
                onClick={clearConversation}
                title={t('assistant.clearSession', 'Görüşmeyi Temizle')}
                aria-label={t('assistant.clearSession', 'Görüşmeyi Temizle')}
                className="de-assistant-header__btn"
              >
                <Trash2 size={16} aria-hidden="true" />
              </button>
            )}

            {/* Expand / Compact Toggle Button */}
            {isExpanded ? (
              <button
                type="button"
                onClick={openCompact}
                title={t('assistant.compact', 'Kompakt Görünüme Dön')}
                aria-label={t('assistant.compact', 'Kompakt Görünüme Dön')}
                className="de-assistant-header__btn"
              >
                <Minimize2 size={16} aria-hidden="true" />
              </button>
            ) : (
              <button
                type="button"
                onClick={openExpanded}
                title={t('assistant.expand', 'Genişlet')}
                aria-label={t('assistant.expand', 'Genişlet')}
                className="de-assistant-header__btn"
              >
                <Maximize2 size={16} aria-hidden="true" />
              </button>
            )}

            {/* Minimize / Close Button */}
            <button
              type="button"
              onClick={collapseAssistant}
              title={t('assistant.minimize', 'Simge Durumuna Küçült')}
              aria-label={t('assistant.minimize', 'Simge Durumuna Küçült')}
              className="de-assistant-header__btn"
            >
              {isExpanded ? <X size={18} aria-hidden="true" /> : <Minus size={16} aria-hidden="true" />}
            </button>
          </div>
        </div>

        {/* ─── Conversation / Messages Area ─── */}
        <div
          tabIndex={0}
          aria-label={t('assistant.conversationHistory', 'Sohbet Geçmişi')}
          className="de-assistant-body"
        >
          <div
            style={{
              maxWidth: isExpanded ? '900px' : '100%',
              width: '100%',
              margin: isExpanded ? '0 auto' : undefined,
              display: 'flex',
              flexDirection: 'column',
              gap: isExpanded ? '20px' : '14px',
            }}
          >
            {/* Empty State */}
            {messages.length === 0 ? (
              <div className="de-assistant-empty">
                <div className="de-assistant-empty__icon">
                  <Bot size={isExpanded ? 28 : 22} aria-hidden="true" />
                </div>

                <h4 className="de-assistant-empty__title">
                  {t('assistant.welcomeTitle', 'Nasıl yardımcı olabilirim?')}
                </h4>

                <p className="de-assistant-empty__desc">
                  {t(
                    'assistant.welcomeSubtitle',
                    "Proje Kütüphanesi'ndeki projeler, teknolojiler ve uygulamalar hakkında soru sorun."
                  )}
                </p>

                {/* Suggested Prompts */}
                <div
                  style={{
                    display: 'flex',
                    flexDirection: isExpanded ? 'row' : 'column',
                    flexWrap: isExpanded ? 'wrap' : 'nowrap',
                    gap: isExpanded ? '10px' : '8px',
                    width: '100%',
                    justifyContent: isExpanded ? 'center' : 'stretch',
                  }}
                >
                  {!isExpanded && (
                    <div
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: '4px',
                        fontSize: '11px',
                        fontWeight: 600,
                        color: 'var(--color-text-muted)',
                        textTransform: 'uppercase',
                        letterSpacing: '0.04em',
                        marginBottom: '2px',
                      }}
                    >
                      <Sparkles size={12} aria-hidden="true" />
                      <span>{t('assistant.examplePromptsTitle', 'Örnek Sorular')}</span>
                    </div>
                  )}

                  {suggestedPrompts.map((prompt, idx) => (
                    <button
                      key={idx}
                      type="button"
                      onClick={() => handleSend(prompt)}
                      className="de-assistant-prompt-btn"
                      style={isExpanded ? { width: 'auto', flex: '1 1 calc(50% - 10px)' } : undefined}
                    >
                      {prompt}
                    </button>
                  ))}
                </div>
              </div>
            ) : (
              messages.map((msg) => (
                <div
                  key={msg.id}
                  style={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: msg.type === 'user' ? 'flex-end' : 'flex-start',
                    gap: '4px',
                  }}
                >
                  {/* User Bubble */}
                  {msg.type === 'user' && (
                    <div className="de-assistant-msg-user">
                      {msg.content}
                    </div>
                  )}

                  {/* Assistant Bubble */}
                  {msg.type === 'assistant' && (
                    <div
                      className={`de-assistant-msg-bot ${
                        msg.isError ? 'de-assistant-msg-bot--error' : ''
                      }`}
                    >
                      {/* Assistant Header inside bubble */}
                      <div className="de-assistant-msg-bot__header">
                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                          <Bot size={15} color="var(--color-brand-red-hover, #f43f5e)" aria-hidden="true" />
                          <span className="de-assistant-bot-name">
                            {t('assistant.title', 'Proje Asistanı')}
                          </span>
                        </div>
                        {msg.metadata && msg.metadata.totalDurationMs > 0 && (
                          <div
                            style={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: '4px',
                              fontSize: '11px',
                              color: 'var(--color-text-muted)',
                            }}
                          >
                            <Clock size={11} aria-hidden="true" />
                            <span>{(msg.metadata.totalDurationMs / 1000).toFixed(1)} sn</span>
                          </div>
                        )}
                      </div>

                      {/* Content */}
                      <div>
                        <SafeMarkdown content={msg.content} />
                      </div>

                      {/* Citations / Source Projects */}
                      {msg.citations && msg.citations.length > 0 && (
                        <div
                          style={{
                            marginTop: isExpanded ? '16px' : '12px',
                            paddingTop: isExpanded ? '12px' : '10px',
                            borderTop: '1px solid var(--color-border-subtle)',
                          }}
                        >
                          <div
                            style={{
                              display: 'flex',
                              alignItems: 'center',
                              gap: '6px',
                              marginBottom: '8px',
                            }}
                          >
                            <Layers size={13} color="var(--color-brand-navy-light)" aria-hidden="true" />
                            <span
                              style={{
                                fontSize: '11px',
                                fontWeight: 600,
                                color: 'var(--color-text-secondary)',
                                textTransform: 'uppercase',
                                letterSpacing: '0.04em',
                              }}
                            >
                              {t('assistant.sourceProjects', 'Kaynak Projeler')} ({msg.citations.length})
                            </span>
                          </div>

                          <div
                            style={
                              isExpanded
                                ? {
                                    display: 'grid',
                                    gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))',
                                    gap: '10px',
                                  }
                                : {
                                    display: 'flex',
                                    flexDirection: 'column',
                                    gap: '6px',
                                  }
                            }
                          >
                            {msg.citations.map((cit) => (
                              <SourceCitationCard
                                key={cit.projectId}
                                citation={cit}
                                isExpanded={isExpanded}
                              />
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))
            )}

            {/* Loading Indicator */}
            {isLoading && (() => {
              const phaseKeys = [
                t('assistant.loadingStep1', 'Sorunuz analiz ediliyor…'),
                t('assistant.loadingStep2', 'İlgili proje bilgileri aranıyor…'),
                t('assistant.loadingStep3', 'Yetkili proje verileri değerlendiriliyor…'),
                t('assistant.loadingStep4', 'Yanıt hazırlanıyor…'),
              ] as const;
              return (
                <div
                  role="status"
                  aria-live="polite"
                  className="de-assistant-msg-bot"
                  style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '10px',
                    padding: '10px 16px',
                    width: 'fit-content',
                  }}
                >
                  <Spinner size="sm" />
                  <span
                    key={loadingPhase}
                    style={{
                      fontSize: '13px',
                      fontWeight: 500,
                      color: 'var(--color-text-primary)',
                      animation: 'de-thinking-fade 0.4s ease',
                    }}
                  >
                    {phaseKeys[loadingPhase]}
                  </span>
                </div>
              );
            })()}

            <div ref={messagesEndRef} />
          </div>
        </div>

        {/* ─── Footer Input Form ─── */}
        <div className="de-assistant-composer">
          <div
            style={{
              maxWidth: isExpanded ? '900px' : '100%',
              margin: isExpanded ? '0 auto' : undefined,
            }}
          >
            {/* Reassurance Disclaimer */}
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                marginBottom: '8px',
                fontSize: '11px',
                color: 'var(--color-text-muted)',
              }}
            >
              <ShieldCheck size={13} color="var(--color-success)" aria-hidden="true" />
              <span>{t('assistant.disclaimerShort', 'Yetkilendirilmiş Proje Kütüphanesi verileri kullanılır.')}</span>
            </div>

            {localError && (
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '4px',
                  color: 'var(--color-danger)',
                  fontSize: '12px',
                  marginBottom: '6px',
                }}
              >
                <AlertCircle size={13} aria-hidden="true" />
                <span>{localError}</span>
              </div>
            )}

            <form
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                handleSend();
              }}
              style={{ display: 'flex', gap: '10px', alignItems: 'flex-end' }}
            >
              <textarea
                ref={inputRef}
                value={inputVal}
                onChange={(e) => {
                  setInputVal(e.target.value);
                  if (localError) setLocalError(null);
                }}
                onKeyDown={handleKeyDown}
                placeholder={
                  isExpanded
                    ? t('assistant.inputPlaceholder', 'Projeler hakkında bir soru sorun... (Örn: Kestirimci bakım projelerimiz hangileri?)')
                    : t('assistant.inputPlaceholderShort', 'Bir soru sorun...')
                }
                rows={isExpanded ? 2 : 1}
                disabled={isLoading}
                aria-label={t('assistant.inputLabel', 'Proje Kütüphanesi Asistanına soru sorun')}
                className="de-assistant-textarea"
                style={{
                  borderColor: localError ? 'var(--color-danger)' : undefined,
                }}
              />

              <button
                type="submit"
                disabled={isLoading || !inputVal.trim()}
                aria-label={t('assistant.askButton', 'Sor')}
                className="de-assistant-send-btn"
                style={{
                  height: isExpanded ? '44px' : '40px',
                }}
              >
                {isLoading ? (
                  <Spinner size="sm" />
                ) : (
                  <>
                    <Send size={15} aria-hidden="true" />
                    {isExpanded && <span>{t('assistant.askButton', 'Sor')}</span>}
                  </>
                )}
              </button>
            </form>
          </div>
        </div>
      </div>
    </>
  );
};

/**
 * Source citation card component
 */
function SourceCitationCard({
  citation,
  isExpanded,
}: {
  citation: ProjectAssistantCitation;
  isExpanded: boolean;
}) {
  return (
    <Link
      to={`/projects/${citation.slug}`}
      className="de-assistant-citation"
      style={{
        flexDirection: isExpanded ? 'column' : 'row',
        alignItems: isExpanded ? 'flex-start' : 'center',
      }}
    >
      <div style={{ minWidth: 0, flex: 1, width: '100%' }}>
        <div className="de-assistant-citation__title">
          {citation.name}
        </div>

        {isExpanded && citation.shortDescription && (
          <p
            className="de-assistant-citation__desc"
            style={{
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
            }}
          >
            {citation.shortDescription}
          </p>
        )}

        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            marginTop: '4px',
            flexWrap: 'wrap',
          }}
        >
          {citation.categoryName && (
            <Badge variant="navy" size="sm">
              {citation.categoryName}
            </Badge>
          )}
          {citation.locations && citation.locations.length > 0 && (
            <span
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '2px',
                fontSize: '10px',
                color: 'var(--color-text-muted)',
              }}
            >
              <MapPin size={10} aria-hidden="true" />
              {citation.locations[0]}
            </span>
          )}
        </div>
      </div>

      <ExternalLink
        size={13}
        color="var(--color-text-muted)"
        style={{ flexShrink: 0, marginTop: isExpanded ? '4px' : '0' }}
        aria-hidden="true"
      />
    </Link>
  );
}

export default ProjectAssistantWidget;
