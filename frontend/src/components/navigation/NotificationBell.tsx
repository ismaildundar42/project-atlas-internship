import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Bell,
  CheckCheck,
  Clock,
  CheckCircle2,
  XCircle,
  FileCode,
  Loader2,
  Trash2,
} from 'lucide-react';
import {
  useNotifications,
  useUnreadNotificationCount,
  useMarkNotificationAsRead,
  useMarkAllNotificationsAsRead,
  useDeleteNotification,
  useClearReadNotifications,
} from '../../hooks/useNotifications';
import type { InAppNotification } from '../../types/notification';
import { formatRelativeTime } from '../../utils/formatters';

export function NotificationBell() {
  const { t, i18n } = useTranslation(['notifications', 'common']);
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const triggerButtonRef = useRef<HTMLButtonElement>(null);
  const targetItemRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  const { data: notifications = [], isLoading } = useNotifications(15);
  const { data: unreadCount = 0 } = useUnreadNotificationCount();
  const markAsReadMutation = useMarkNotificationAsRead();
  const markAllAsReadMutation = useMarkAllNotificationsAsRead();
  const deleteMutation = useDeleteNotification();
  const clearReadMutation = useClearReadNotifications();

  const hasReadNotifications = notifications.some((n) => n.isRead);
  const firstUnreadIndex = notifications.findIndex((n) => !n.isRead);
  const targetFocusIndex = firstUnreadIndex !== -1 ? firstUnreadIndex : (notifications.length > 0 ? 0 : -1);

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  // Handle Escape key to close dropdown and return focus to trigger button
  useEffect(() => {
    if (!isOpen) return;

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        event.preventDefault();
        setIsOpen(false);
        triggerButtonRef.current?.focus();
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  // Focus target notification (first unread or first available) when dropdown opens
  useEffect(() => {
    if (isOpen && !isLoading) {
      const timer = setTimeout(() => {
        if (targetItemRef.current) {
          targetItemRef.current.focus();
        }
      }, 50);
      return () => clearTimeout(timer);
    }
  }, [isOpen, isLoading, targetFocusIndex]);

  const handleNotificationClick = async (notif: InAppNotification) => {
    if (!notif.isRead) {
      await markAsReadMutation.mutateAsync(notif.id);
    }
    setIsOpen(false);
    if (notif.targetUrl) {
      navigate(notif.targetUrl);
    } else {
      triggerButtonRef.current?.focus();
    }
  };

  const handleMarkAllRead = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (markAllAsReadMutation.isPending) return;
    await markAllAsReadMutation.mutateAsync();
  };

  const handleClearRead = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (clearReadMutation.isPending) return;
    await clearReadMutation.mutateAsync();
  };

  const handleDeleteNotification = async (e: React.MouseEvent, id: number) => {
    e.stopPropagation();
    if (deleteMutation.isPending) return;
    await deleteMutation.mutateAsync(id);
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case 'ProjectSubmittedForReview':
        return <Clock size={16} style={{ color: '#2563eb', flexShrink: 0 }} />;
      case 'ProjectApproved':
        return <CheckCircle2 size={16} style={{ color: '#16a34a', flexShrink: 0 }} />;
      case 'ProjectRejected':
        return <XCircle size={16} style={{ color: '#dc2626', flexShrink: 0 }} />;
      default:
        return <FileCode size={16} style={{ color: '#6b7280', flexShrink: 0 }} />;
    }
  };

  return (
    <div
      className="notification-bell-container"
      ref={dropdownRef}
      style={{ position: 'relative', display: 'inline-flex', alignItems: 'center' }}
    >
      <button
        ref={triggerButtonRef}
        type="button"
        className={`icon-btn icon-btn--ghost icon-btn--md ${isOpen ? 'icon-btn--active' : ''}`}
        aria-label={`${t('notifications.title', 'Bildirimler')}${unreadCount > 0 ? ` (${t('notifications.unreadCount', { count: unreadCount })})` : ''}`}
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        aria-controls="notifications-panel"
        onClick={() => setIsOpen((prev) => !prev)}
        id="notifications-btn"
        style={{ position: 'relative' }}
      >
        <span aria-hidden="true" style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}>
          <Bell size={18} />
        </span>
        {unreadCount > 0 && (
          <span
            style={{
              position: 'absolute',
              top: '4px',
              right: '4px',
              minWidth: '16px',
              height: '16px',
              padding: '0 4px',
              backgroundColor: '#ef4444',
              color: '#ffffff',
              fontSize: '10px',
              fontWeight: 700,
              borderRadius: '999px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              lineHeight: 1,
              boxShadow: '0 0 0 2px var(--color-brand-navy, #1e293b)',
            }}
          >
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div
          id="notifications-panel"
          role="dialog"
          aria-label={t('notifications.panelAria', 'Bildirimler paneli')}
          style={{
            position: 'absolute',
            top: 'calc(100% + 8px)',
            right: 0,
            width: '360px',
            maxWidth: '92vw',
            backgroundColor: 'var(--color-bg-surface, #ffffff)',
            border: '1px solid var(--color-border, #e5e7eb)',
            borderRadius: '10px',
            boxShadow: '0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)',
            zIndex: 1000,
            overflow: 'hidden',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          {/* Header */}
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              padding: '12px 16px',
              borderBottom: '1px solid var(--color-border, #e5e7eb)',
              backgroundColor: 'var(--color-bg-subtle, #f9fafb)',
              gap: '8px',
            }}
          >
            <div style={{ fontWeight: 600, fontSize: '14px', color: 'var(--color-text-primary, #111827)', whiteSpace: 'nowrap' }}>
              {t('notifications.title', 'Bildirimler')}
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap', justifyContent: 'flex-end' }}>
              {unreadCount > 0 && (
                <button
                  type="button"
                  onClick={handleMarkAllRead}
                  disabled={markAllAsReadMutation.isPending}
                  style={{
                    background: 'none',
                    border: 'none',
                    padding: 0,
                    fontSize: '11px',
                    color: 'var(--color-primary-600, #2563eb)',
                    cursor: 'pointer',
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '3px',
                    fontWeight: 500,
                  }}
                  title={t('notifications.markAllAsRead', 'Tümünü okundu olarak işaretle')}
                >
                  <CheckCheck size={13} /> {t('notifications.markAllAsReadShort', 'Tümünü oku')}
                </button>
              )}
              {hasReadNotifications && (
                <button
                  type="button"
                  onClick={handleClearRead}
                  disabled={clearReadMutation.isPending}
                  style={{
                    background: 'none',
                    border: 'none',
                    padding: 0,
                    fontSize: '11px',
                    color: 'var(--color-text-secondary, #6b7280)',
                    cursor: 'pointer',
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '3px',
                    fontWeight: 500,
                  }}
                  title={t('notifications.clearRead', 'Okunmuş bildirimleri temizle')}
                >
                  <Trash2 size={12} /> {t('notifications.clearReadShort', 'Okunmuşları temizle')}
                </button>
              )}
            </div>
          </div>

          {/* Body */}
          <div style={{ maxHeight: '380px', overflowY: 'auto' }}>
            {isLoading ? (
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '24px', color: 'var(--color-text-secondary, #6b7280)' }}>
                <Loader2 size={20} className="animate-spin" />
              </div>
            ) : notifications.length === 0 ? (
              <div style={{ padding: '32px 16px', textAlign: 'center', color: 'var(--color-text-secondary, #6b7280)', fontSize: '13px' }}>
                <Bell size={28} style={{ margin: '0 auto 8px', opacity: 0.4 }} />
                {t('notifications.empty', 'Henüz bildiriminiz yok.')}
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column' }}>
                {notifications.map((notif, idx) => (
                  <div
                    key={notif.id}
                    ref={idx === targetFocusIndex ? targetItemRef : undefined}
                    className="notification-item-row"
                    onClick={() => handleNotificationClick(notif)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        handleNotificationClick(notif);
                      }
                    }}
                    style={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      gap: '10px',
                      padding: '12px 16px',
                      borderBottom: '1px solid var(--color-border-subtle, #f3f4f6)',
                      backgroundColor: notif.isRead
                        ? 'transparent'
                        : 'var(--color-bg-subtle, #f0fdf4)',
                      textAlign: 'left',
                      cursor: 'pointer',
                      transition: 'background-color 0.15s ease',
                      width: '100%',
                      position: 'relative',
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.backgroundColor = 'var(--color-bg-hover, #f3f4f6)';
                      const btn = e.currentTarget.querySelector<HTMLElement>('.notif-delete-btn');
                      if (btn) btn.style.opacity = '1';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.backgroundColor = notif.isRead
                        ? 'transparent'
                        : 'var(--color-bg-subtle, #f0fdf4)';
                      const btn = e.currentTarget.querySelector<HTMLElement>('.notif-delete-btn');
                      if (btn && document.activeElement !== btn) btn.style.opacity = '0';
                    }}
                  >
                    <div style={{ marginTop: '2px' }}>{getNotificationIcon(notif.type)}</div>
                    <div style={{ flex: 1, minWidth: 0, paddingRight: '20px' }}>
                      <div
                        style={{
                          fontSize: '13px',
                          fontWeight: notif.isRead ? 500 : 700,
                          color: 'var(--color-text-primary, #111827)',
                          lineHeight: 1.3,
                        }}
                      >
                        {notif.title}
                      </div>
                      <div
                        style={{
                          fontSize: '12px',
                          color: 'var(--color-text-secondary, #4b5563)',
                          marginTop: '2px',
                          lineHeight: 1.4,
                          wordBreak: 'break-word',
                        }}
                      >
                        {notif.message}
                      </div>
                      <div
                        style={{
                          fontSize: '11px',
                          color: 'var(--color-text-muted, #9ca3af)',
                          marginTop: '4px',
                        }}
                      >
                        {formatRelativeTime(notif.createdAtUtc, i18n.language)}
                      </div>
                    </div>

                    <div
                      style={{
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        gap: '6px',
                        flexShrink: 0,
                        alignSelf: 'stretch',
                      }}
                    >
                      {!notif.isRead ? (
                        <span
                          style={{
                            width: '7px',
                            height: '7px',
                            borderRadius: '50%',
                            backgroundColor: '#2563eb',
                            marginTop: '4px',
                            flexShrink: 0,
                          }}
                        />
                      ) : (
                        <span style={{ width: '7px', height: '7px' }} />
                      )}

                      <button
                        type="button"
                        className="notif-delete-btn"
                        aria-label={t('notifications.deleteNotification', 'Bildirimi sil')}
                        title={t('notifications.deleteNotification', 'Bildirimi sil')}
                        onClick={(e) => handleDeleteNotification(e, notif.id)}
                        disabled={deleteMutation.isPending}
                        style={{
                          background: 'none',
                          border: 'none',
                          padding: '4px',
                          color: 'var(--color-text-muted, #9ca3af)',
                          cursor: 'pointer',
                          borderRadius: '4px',
                          display: 'inline-flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          opacity: 0,
                          transition: 'opacity 0.15s ease, color 0.15s ease',
                        }}
                        onFocus={(e) => {
                          e.currentTarget.style.opacity = '1';
                          e.currentTarget.style.color = 'var(--color-danger, #ef4444)';
                        }}
                        onBlur={(e) => {
                          e.currentTarget.style.opacity = '0';
                          e.currentTarget.style.color = 'var(--color-text-muted, #9ca3af)';
                        }}
                        onMouseEnter={(e) => {
                          e.currentTarget.style.color = 'var(--color-danger, #ef4444)';
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.color = 'var(--color-text-muted, #9ca3af)';
                        }}
                      >
                        <Trash2 size={13} />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default NotificationBell;

