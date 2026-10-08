import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Sparkles, RefreshCw, AlertCircle, Check, Copy, ChevronDown, ChevronUp, Bot } from 'lucide-react';
import { aiSummaryService } from '../../services/aiSummaryService';
import type { ProjectAiSummaryResponse } from '../../types/aiSummary';
import SafeMarkdown from '../ai/SafeMarkdown';

interface ProjectAiSummaryCardProps {
  projectId: number;
  projectName: string;
}

export const ProjectAiSummaryCard: React.FC<ProjectAiSummaryCardProps> = ({
  projectId,
  projectName,
}) => {
  const { t, i18n } = useTranslation('projects');
  const [summaryData, setSummaryData] = useState<ProjectAiSummaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [loadingStep, setLoadingStep] = useState<0 | 1 | 2 | 3>(0);
  const [error, setError] = useState<string | null>(null);
  const [isExpanded, setIsExpanded] = useState(true);
  const [copied, setCopied] = useState(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  // Progressive loading step timer: 0 -> 1 (2.5s) -> 2 (5.0s) -> 3 (7.5s)
  useEffect(() => {
    if (!isLoading) {
      setLoadingStep(0);
      return;
    }

    setLoadingStep(0);
    const t1 = setTimeout(() => setLoadingStep(1), 2500);
    const t2 = setTimeout(() => setLoadingStep(2), 5000);
    const t3 = setTimeout(() => setLoadingStep(3), 7500);

    return () => {
      clearTimeout(t1);
      clearTimeout(t2);
      clearTimeout(t3);
    };
  }, [isLoading]);

  // Reset when project ID changes
  useEffect(() => {
    setSummaryData(null);
    setError(null);
    setIsLoading(false);
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
  }, [projectId]);

  const handleGenerate = async () => {
    if (isLoading) return;

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const activeLang = i18n.language?.startsWith('en') ? 'en' : 'tr';
      const result = await aiSummaryService.getProjectSummary(
        projectId,
        { language: activeLang },
        controller.signal
      );
      setSummaryData(result);
      setIsExpanded(true);
    } catch (err: any) {
      if (err?.name === 'CanceledError' || err?.code === 'ERR_CANCELED') {
        return;
      }
      const status = err?.response?.status;
      if (status === 403) {
        setError(t('aiSummary.forbidden', 'Bu projenin özetine erişim yetkiniz bulunmamaktadır.'));
      } else if (status === 503) {
        setError(t('aiSummary.unavailable', 'Yapay zeka özet servisi şu anda kullanılamıyor.'));
      } else {
        setError(t('aiSummary.error', 'Özet oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.'));
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopy = async () => {
    if (!summaryData?.summary) return;
    try {
      await navigator.clipboard.writeText(summaryData.summary);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // ignore clipboard error
    }
  };

  const loadingMessages = [
    t('aiSummary.loadingStep1', 'Proje bilgileri hazırlanıyor…'),
    t('aiSummary.loadingStep2', 'Teknik ve operasyonel detaylar değerlendiriliyor…'),
    t('aiSummary.loadingStep3', 'AI özeti oluşturuluyor…'),
    t('aiSummary.loadingStep4', 'Özet tamamlanıyor…'),
  ];

  return (
    <div
      className="project-ai-summary-card"
      aria-label={`${projectName} - ${t('aiSummary.title', 'AI Proje Özeti')}`}
    >
      {/* Header Bar */}
      <div
        className={`project-ai-summary-card__header ${
          summaryData && isExpanded ? 'project-ai-summary-card__header--has-summary' : ''
        }`}
      >
        <div className="project-ai-summary-card__title-group">
          <div className="project-ai-summary-card__icon" aria-hidden="true">
            <Sparkles size={16} />
          </div>
          <div className="project-ai-summary-card__title-content">
            <div className="project-ai-summary-card__title-row">
              <h3 className="project-ai-summary-card__title">
                {t('aiSummary.title', 'AI Proje Özeti')}
              </h3>
              <span className="project-ai-summary-card__badge">
                {t('aiSummary.badge', 'Akıllı Özet')}
              </span>
            </div>
            {!summaryData && !isLoading && (
              <p className="project-ai-summary-card__hint">
                {t(
                  'aiSummary.hint',
                  'Projenin amacı, mimarisi ve sağladığı iş katkısını saniyeler içinde özetleyin.'
                )}
              </p>
            )}
          </div>
        </div>

        {/* Action Controls */}
        <div className="project-ai-summary-card__actions">
          {!summaryData && !isLoading && (
            <button
              type="button"
              onClick={handleGenerate}
              id="btn-generate-ai-summary"
              className="project-ai-summary-card__btn-generate"
            >
              <Sparkles size={14} aria-hidden="true" />
              <span>{t('aiSummary.generateButton', 'AI ile Özetle')}</span>
            </button>
          )}

          {summaryData && (
            <>
              <button
                type="button"
                onClick={handleCopy}
                title={t('aiSummary.copyAria', 'Özeti Kopyala')}
                aria-label={t('aiSummary.copyAria', 'Özeti Kopyala')}
                className={`project-ai-summary-card__btn-icon ${
                  copied ? 'project-ai-summary-card__btn-icon--copied' : ''
                }`}
              >
                {copied ? <Check size={14} aria-hidden="true" /> : <Copy size={14} aria-hidden="true" />}
              </button>

              <button
                type="button"
                onClick={handleGenerate}
                disabled={isLoading}
                title={t('aiSummary.refreshAria', 'Yeniden Özetle')}
                aria-label={t('aiSummary.refreshAria', 'Yeniden Özetle')}
                className="project-ai-summary-card__btn-icon"
              >
                <RefreshCw
                  size={14}
                  className={isLoading ? 'project-ai-summary-card__spin' : ''}
                  aria-hidden="true"
                />
              </button>

              <button
                type="button"
                onClick={() => setIsExpanded(!isExpanded)}
                title={isExpanded ? t('aiSummary.collapse', 'Daralt') : t('aiSummary.expand', 'Genişlet')}
                aria-label={isExpanded ? t('aiSummary.collapse', 'Daralt') : t('aiSummary.expand', 'Genişlet')}
                className="project-ai-summary-card__btn-icon"
              >
                {isExpanded ? <ChevronUp size={14} aria-hidden="true" /> : <ChevronDown size={14} aria-hidden="true" />}
              </button>
            </>
          )}
        </div>
      </div>

      {/* Loading State with Progressive UX */}
      {isLoading && (
        <div
          className="project-ai-summary-card__loading"
          role="status"
          aria-live="polite"
        >
          <div className="project-ai-summary-card__loading-status">
            <RefreshCw
              size={16}
              className="project-ai-summary-card__spin"
              aria-hidden="true"
            />
            <span
              key={loadingStep}
              className="project-ai-summary-card__loading-text"
            >
              {loadingMessages[loadingStep]}
            </span>
          </div>

          <div className="project-ai-summary-card__skeleton" aria-hidden="true">
            <div className="project-ai-summary-card__skeleton-line" style={{ width: '92%' }} />
            <div className="project-ai-summary-card__skeleton-line" style={{ width: '84%' }} />
            <div className="project-ai-summary-card__skeleton-line" style={{ width: '68%' }} />
          </div>
        </div>
      )}

      {/* Error State */}
      {error && !isLoading && (
        <div className="project-ai-summary-card__error" role="alert">
          <div className="project-ai-summary-card__error-msg">
            <AlertCircle size={16} aria-hidden="true" />
            <span>{error}</span>
          </div>
          <button
            type="button"
            onClick={handleGenerate}
            className="project-ai-summary-card__btn-retry"
          >
            {t('aiSummary.retry', 'Tekrar Dene')}
          </button>
        </div>
      )}

      {/* Content Area */}
      {summaryData && isExpanded && !isLoading && (
        <div className="project-ai-summary-card__content-area">
          <div className="project-ai-summary-card__markdown">
            <SafeMarkdown content={summaryData.summary} />
          </div>

          {/* Footer Metadata */}
          <div className="project-ai-summary-card__footer">
            <div className="project-ai-summary-card__model-meta">
              <Bot size={13} aria-hidden="true" />
              <span>
                {t('aiSummary.modelMeta', 'Model')}: {summaryData.model || summaryData.provider}
                {summaryData.durationMs > 0 && ` (${(summaryData.durationMs / 1000).toFixed(1)}s)`}
              </span>
            </div>
            <span>
              {t(
                'aiSummary.disclaimer',
                'Bu özet yapay zeka tarafından doğrudan doğrulanmış veritabanı kayıtlarından üretilmiştir.'
              )}
            </span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectAiSummaryCard;
