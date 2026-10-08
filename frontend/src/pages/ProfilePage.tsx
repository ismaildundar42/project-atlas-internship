import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  User,
  Shield,
  Building2,
  Users,
  FolderGit2,
  CheckCircle2,
  AlertCircle,
  Eye,
  EyeOff,
  ExternalLink,
  Edit3,
  Calendar,
  Lock,
  Plus,
  ArrowRight,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import Card from '../components/ui/Card';
import Badge from '../components/ui/Badge';
import Button from '../components/ui/Button';
import Spinner from '../components/ui/Spinner';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import { profileService } from '../services/profileService';
import type { UserProfileResponse, ProfileProjectItem } from '../types/profile';

type ProfileTab = 'overview' | 'owned' | 'contributed' | 'security';

export function ProfilePage() {
  const { t } = useTranslation(['profile', 'navigation', 'common', 'projects']);
  const [searchParams, setSearchParams] = useSearchParams();
  const initialTab = (searchParams.get('tab') as ProfileTab) || 'overview';

  const [activeTab, setActiveTab] = useState<ProfileTab>(
    ['overview', 'owned', 'contributed', 'security'].includes(initialTab) ? initialTab : 'overview'
  );

  const [profile, setProfile] = useState<UserProfileResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Password change state
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);

  useEffect(() => {
    loadProfile();
  }, []);

  useEffect(() => {
    const tabParam = searchParams.get('tab') as ProfileTab;
    if (['overview', 'owned', 'contributed', 'security'].includes(tabParam)) {
      setActiveTab(tabParam);
    } else {
      setActiveTab('overview');
    }
  }, [searchParams]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await profileService.getProfile();
      setProfile(data);
    } catch (err: any) {
      const errorDetail = err?.response?.data?.detail || err?.response?.data?.message;
      if (errorDetail && !/status code|Network Error|AxiosError|404/i.test(errorDetail)) {
        setError(errorDetail);
      } else {
        setError(t('profile:loadErrorDesc', 'Profil bilgileri yüklenirken bir hata oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.'));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (tab: ProfileTab) => {
    setActiveTab(tab);
    setSearchParams(tab === 'overview' ? {} : { tab });
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError(null);
    setPasswordSuccess(null);

    if (!currentPassword) {
      setPasswordError(t('profile:security.errors.currentRequired', 'Mevcut şifrenizi giriniz.'));
      return;
    }
    if (newPassword.length < 6) {
      setPasswordError(t('profile:security.errors.newMinLength', 'Yeni şifre en az 6 karakter olmalıdır.'));
      return;
    }
    if (newPassword !== confirmPassword) {
      setPasswordError(t('profile:security.errors.mismatch', 'Yeni şifreler eşleşmiyor.'));
      return;
    }

    try {
      setPasswordLoading(true);
      const res = await profileService.changePassword({
        currentPassword,
        newPassword,
        confirmNewPassword: confirmPassword,
      });
      setPasswordSuccess(res.message || t('profile:security.successMessage', 'Şifreniz başarıyla değiştirildi.'));
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: any) {
      const detail = err?.response?.data?.detail || err?.response?.data?.message || err?.response?.data?.title;
      if (detail && !/status code|Network Error|AxiosError/i.test(detail)) {
        setPasswordError(detail);
      } else {
        setPasswordError(t('profile:security.errors.generic', 'Şifre değiştirilirken bir hata oluştu. Lütfen bilgilerinizi kontrol edip tekrar deneyin.'));
      }
    } finally {
      setPasswordLoading(false);
    }
  };

  // Loading state
  if (loading) {
    return (
      <PageLayout
        title={t('profile:title', 'Profilim')}
        description={t('profile:description', 'Hesap bilgilerinizi, organizasyon üyeliğinizi ve proje ilişkilerinizi görüntüleyin.')}
        breadcrumbs={[
          { label: t('profile:breadcrumbs.home', 'Ana Sayfa'), href: '/' },
          { label: t('profile:breadcrumbs.profile', 'Profilim') },
        ]}
      >
        <Card className="flex flex-col items-center justify-center p-12 text-center">
          <Spinner size="lg" className="mb-4" />
          <p className="text-sm text-secondary">{t('profile:loading', 'Profil bilgileri yükleniyor...')}</p>
        </Card>
      </PageLayout>
    );
  }

  // Error state
  if (error || !profile) {
    return (
      <PageLayout
        title={t('profile:title', 'Profilim')}
        description={t('profile:description', 'Hesap bilgilerinizi, organizasyon üyeliğinizi ve proje ilişkilerinizi görüntüleyin.')}
        breadcrumbs={[
          { label: t('profile:breadcrumbs.home', 'Ana Sayfa'), href: '/' },
          { label: t('profile:breadcrumbs.profile', 'Profilim') },
        ]}
      >
        <Card>
          <ErrorState
            title={t('profile:loadErrorTitle', 'Profil Bilgileri Alınamadı')}
            description={error || t('profile:loadErrorDesc', 'Profil bilgileri yüklenirken bir hata oluştu.')}
            retryLabel={t('profile:actions.retry', 'Yeniden Dene')}
            onRetry={loadProfile}
          />
        </Card>
      </PageLayout>
    );
  }

  const user = profile.user;
  const member = profile.member;
  const canManageProjects = user.canCreateProjects || user.isAdmin || user.isSuperAdmin;
  const isAdminOrSuper = user.isAdmin || user.isSuperAdmin;

  const getRoleLabel = () => {
    if (user.isSuperAdmin) return t('profile:roles.superAdmin', 'Süper Yönetici');
    if (user.isAdmin) return t('profile:roles.admin', 'Yönetici');
    if (user.canCreateProjects) return t('profile:roles.creator', 'Proje Geliştirici');
    return t('profile:roles.user', 'Kurumsal Kullanıcı');
  };

  // Header Actions
  const headerActions = (
    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
      {isAdminOrSuper && (
        <Link to="/admin">
          <Button variant="secondary" size="sm" leftIcon={<Shield size={16} />}>
            {t('profile:actions.adminDashboard', 'Yönetim Merkezi')}
          </Button>
        </Link>
      )}
      {canManageProjects && (
        <Link to="/admin/projects/new">
          <Button variant="primary" size="sm" leftIcon={<Plus size={16} />}>
            {t('profile:actions.newProject', 'Yeni Proje')}
          </Button>
        </Link>
      )}
    </div>
  );

  return (
    <PageLayout
      title={t('profile:title', 'Profilim')}
      description={t('profile:description', 'Hesap bilgilerinizi, organizasyon üyeliğinizi ve proje ilişkilerinizi görüntüleyin.')}
      breadcrumbs={[
        { label: t('profile:breadcrumbs.home', 'Ana Sayfa'), href: '/' },
        { label: t('profile:breadcrumbs.profile', 'Profilim') },
      ]}
      actions={headerActions}
    >
      {/* ─── Tabs Navigation ─── */}
      <nav className="profile-tabs" role="tablist" aria-label={t('profile:title', 'Profilim')}>
        <button
          type="button"
          role="tab"
          id="tab-overview"
          aria-controls="panel-overview"
          aria-selected={activeTab === 'overview'}
          className={`profile-tab-btn ${activeTab === 'overview' ? 'profile-tab-btn--active' : ''}`}
          onClick={() => handleTabChange('overview')}
        >
          <User size={16} aria-hidden="true" />
          <span>{t('profile:tabs.overview', 'Genel Bakış')}</span>
        </button>

        <button
          type="button"
          role="tab"
          id="tab-owned"
          aria-controls="panel-owned"
          aria-selected={activeTab === 'owned'}
          className={`profile-tab-btn ${activeTab === 'owned' ? 'profile-tab-btn--active' : ''}`}
          onClick={() => handleTabChange('owned')}
        >
          <FolderGit2 size={16} aria-hidden="true" />
          <span>{t('profile:tabs.owned', 'Projelerim')}</span>
          <span className="profile-tab-badge">{profile.ownedProjects.length}</span>
        </button>

        <button
          type="button"
          role="tab"
          id="tab-contributed"
          aria-controls="panel-contributed"
          aria-selected={activeTab === 'contributed'}
          className={`profile-tab-btn ${activeTab === 'contributed' ? 'profile-tab-btn--active' : ''}`}
          onClick={() => handleTabChange('contributed')}
        >
          <Users size={16} aria-hidden="true" />
          <span>{t('profile:tabs.contributed', 'Katkı Sağladığım Projeler')}</span>
          <span className="profile-tab-badge">{profile.contributedProjects.length}</span>
        </button>

        <button
          type="button"
          role="tab"
          id="tab-security"
          aria-controls="panel-security"
          aria-selected={activeTab === 'security'}
          className={`profile-tab-btn ${activeTab === 'security' ? 'profile-tab-btn--active' : ''}`}
          onClick={() => handleTabChange('security')}
        >
          <Lock size={16} aria-hidden="true" />
          <span>{t('profile:tabs.security', 'Şifre Değiştir')}</span>
        </button>
      </nav>

      {/* ─── Tab 1: Overview ─── */}
      {activeTab === 'overview' && (
        <div
          id="panel-overview"
          role="tabpanel"
          aria-labelledby="tab-overview"
          className="profile-overview-grid"
        >
          {/* Card 1: User Information */}
          <div className="profile-card">
            <div className="profile-card__header">
              <div className="profile-card__title-wrap">
                <span className="profile-card__icon" aria-hidden="true">
                  <User size={18} />
                </span>
                <h2 className="profile-card__title">{t('profile:userInfo.title', 'Kullanıcı Bilgileri')}</h2>
              </div>
              <Badge variant={user.isActive ? 'success' : 'danger'} size="sm">
                {user.isActive
                  ? t('profile:userInfo.statusActive', 'Aktif')
                  : t('profile:userInfo.statusInactive', 'Pasif')}
              </Badge>
            </div>
            <div className="profile-card__body">
              <dl className="profile-dl">
                <div className="profile-dl__row">
                  <dt className="profile-dl__dt">{t('profile:userInfo.fullName', 'Ad Soyad')}</dt>
                  <dd className="profile-dl__dd font-semibold">{user.fullName || `${user.firstName} ${user.lastName}`}</dd>
                </div>
                <div className="profile-dl__row">
                  <dt className="profile-dl__dt">{t('profile:userInfo.email', 'E-posta')}</dt>
                  <dd className="profile-dl__dd">{user.email}</dd>
                </div>
                <div className="profile-dl__row">
                  <dt className="profile-dl__dt">{t('profile:userInfo.accessLevel', 'Yetki Seviyesi')}</dt>
                  <dd className="profile-dl__dd">
                    <Badge variant={user.isSuperAdmin ? 'navy' : user.isAdmin ? 'info' : 'default'} size="sm">
                      {getRoleLabel()}
                    </Badge>
                  </dd>
                </div>
                {user.roles && user.roles.length > 0 && (
                  <div className="profile-dl__row">
                    <dt className="profile-dl__dt">Roller</dt>
                    <dd className="profile-dl__dd flex flex-wrap justify-end gap-1">
                      {user.roles.map((r, idx) => (
                        <span key={idx} className="badge badge--default badge--sm">
                          {r}
                        </span>
                      ))}
                    </dd>
                  </div>
                )}
              </dl>
            </div>
          </div>

          {/* Card 2: Organization Info */}
          <div className="profile-card">
            <div className="profile-card__header">
              <div className="profile-card__title-wrap">
                <span className="profile-card__icon" aria-hidden="true">
                  <Building2 size={18} />
                </span>
                <h2 className="profile-card__title">{t('profile:organizationInfo.title', 'Organizasyon Bilgileri')}</h2>
              </div>
              {member?.isLinked ? (
                <Badge variant="success" size="sm" icon={<CheckCircle2 size={12} />}>
                  {t('profile:organizationInfo.linked', 'Rehberle Eşleştirildi')}
                </Badge>
              ) : (
                <Badge variant="warning" size="sm">
                  {t('profile:organizationInfo.notSpecified', 'Eşleştirilmemiş')}
                </Badge>
              )}
            </div>
            <div className="profile-card__body">
              {member?.isLinked ? (
                <dl className="profile-dl">
                  <div className="profile-dl__row">
                    <dt className="profile-dl__dt">{t('profile:organizationInfo.department', 'Departman')}</dt>
                    <dd className="profile-dl__dd">{member.departmentName || t('profile:organizationInfo.notSpecified', 'Belirtilmemiş')}</dd>
                  </div>
                  <div className="profile-dl__row">
                    <dt className="profile-dl__dt">{t('profile:organizationInfo.team', 'Ekip')}</dt>
                    <dd className="profile-dl__dd">{member.teamName || t('profile:organizationInfo.notSpecified', 'Belirtilmemiş')}</dd>
                  </div>
                  <div className="profile-dl__row">
                    <dt className="profile-dl__dt">{t('profile:organizationInfo.position', 'Ünvan')}</dt>
                    <dd className="profile-dl__dd">{member.title || t('profile:organizationInfo.notSpecified', 'Belirtilmemiş')}</dd>
                  </div>
                </dl>
              ) : (
                <div className="p-4 rounded-md bg-subtle text-secondary text-sm">
                  <p className="m-0">
                    {t(
                      'profile:organizationInfo.notLinked',
                      'Bu kullanıcı henüz organizasyon dizinindeki bir kişiyle eşleştirilmemiş.'
                    )}
                  </p>
                </div>
              )}
            </div>
            {isAdminOrSuper && (
              <div className="profile-card__footer">
                <Link to="/admin/organization" className="btn btn--ghost btn--sm">
                  <span>{t('profile:actions.manageInOrganization', 'Organizasyonda Yönet')}</span>
                  <ArrowRight size={14} className="ml-1" />
                </Link>
              </div>
            )}
          </div>

          {/* Card 3: Permissions */}
          <div className="profile-card">
            <div className="profile-card__header">
              <div className="profile-card__title-wrap">
                <span className="profile-card__icon" aria-hidden="true">
                  <Shield size={18} />
                </span>
                <h2 className="profile-card__title">{t('profile:permissions.title', 'Erişim ve Yetkiler')}</h2>
              </div>
            </div>
            <div className="profile-card__body">
              <div className="profile-perm-list">
                <div className="profile-perm-item">
                  <span className="profile-perm-name">{t('profile:permissions.projectCreation', 'Proje Oluşturma')}</span>
                  <span
                    className={`profile-perm-badge ${
                      user.canCreateProjects || user.isAdmin || user.isSuperAdmin
                        ? 'profile-perm-badge--granted'
                        : 'profile-perm-badge--denied'
                    }`}
                  >
                    {user.canCreateProjects || user.isAdmin || user.isSuperAdmin
                      ? t('profile:permissions.authorized', 'Yetkili')
                      : t('profile:permissions.notAuthorized', 'Yetki Yok')}
                  </span>
                </div>

                <div className="profile-perm-item">
                  <span className="profile-perm-name">{t('profile:permissions.adminCenter', 'Yönetim Merkezi')}</span>
                  <span
                    className={`profile-perm-badge ${
                      user.isAdmin || user.isSuperAdmin
                        ? 'profile-perm-badge--granted'
                        : 'profile-perm-badge--denied'
                    }`}
                  >
                    {user.isAdmin || user.isSuperAdmin
                      ? t('profile:permissions.authorized', 'Yetkili')
                      : t('profile:permissions.notAuthorized', 'Yetki Yok')}
                  </span>
                </div>

                <div className="profile-perm-item">
                  <span className="profile-perm-name">{t('profile:permissions.userManagement', 'Kullanıcı Yönetimi')}</span>
                  <span
                    className={`profile-perm-badge ${
                      user.isSuperAdmin
                        ? 'profile-perm-badge--granted'
                        : 'profile-perm-badge--denied'
                    }`}
                  >
                    {user.isSuperAdmin
                      ? t('profile:permissions.authorized', 'Yetkili')
                      : t('profile:permissions.notAuthorized', 'Yetki Yok')}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ─── Tab 2: Owned Projects ─── */}
      {activeTab === 'owned' && (
        <div id="panel-owned" role="tabpanel" aria-labelledby="tab-owned">
          {profile.ownedProjects.length === 0 ? (
            <Card>
              <EmptyState
                icon={<FolderGit2 size={48} strokeWidth={1.25} />}
                title={t('profile:projects.noOwnedProjects', 'Henüz bir proje oluşturmadınız')}
                description={t(
                  'profile:projects.noOwnedProjectsDesc',
                  'Kurum içinde geliştirdiğiniz veya katkı sağladığınız uygulamaları kütüphaneye ekleyebilirsiniz.'
                )}
                actionLabel={canManageProjects ? t('profile:actions.addFirstProject', 'İlk Projemi Ekle') : undefined}
                onAction={canManageProjects ? () => { window.location.href = '/admin/projects/new'; } : undefined}
              />
            </Card>
          ) : (
            <div className="profile-projects-grid">
              {profile.ownedProjects.map((project: ProfileProjectItem) => (
                <article key={project.id} className="profile-project-card">
                  <div>
                    <div className="profile-project-card__top">
                      <div className="profile-project-card__badges">
                        <Badge variant="navy" size="sm">
                          {project.categoryName || project.categoryCode}
                        </Badge>
                        <Badge variant={project.isPublished ? 'success' : 'warning'} size="sm">
                          {project.statusName || (project.isPublished ? t('profile:projects.published', 'Yayında') : t('profile:projects.draft', 'Taslak'))}
                        </Badge>
                      </div>
                    </div>
                    <h3 className="profile-project-card__title">
                      <Link to={`/projects/${project.slug}`} className="hover:underline">
                        {project.name}
                      </Link>
                    </h3>
                    {project.shortDescription && (
                      <p className="profile-project-card__desc">{project.shortDescription}</p>
                    )}
                  </div>

                  <div>
                    <div className="profile-project-card__meta">
                      <span className="flex items-center gap-1">
                        <Calendar size={13} aria-hidden="true" />
                        <span>{new Date(project.createdAt).toLocaleDateString('tr-TR')}</span>
                      </span>
                    </div>

                    <div className="profile-project-card__actions">
                      <Link to={`/projects/${project.slug}`} className="btn btn--secondary btn--sm flex-1 justify-center">
                        <ExternalLink size={14} className="mr-1" />
                        <span>{t('profile:actions.viewProject', 'İncele')}</span>
                      </Link>
                      {canManageProjects && (
                        <Link to={`/admin/projects/${project.id}/edit`} className="btn btn--ghost btn--sm">
                          <Edit3 size={14} className="mr-1" />
                          <span>{t('profile:actions.editProject', 'Düzenle')}</span>
                        </Link>
                      )}
                    </div>
                  </div>
                </article>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ─── Tab 3: Contributed Projects ─── */}
      {activeTab === 'contributed' && (
        <div id="panel-contributed" role="tabpanel" aria-labelledby="tab-contributed">
          {profile.contributedProjects.length === 0 ? (
            <Card>
              <EmptyState
                icon={<Users size={48} strokeWidth={1.25} />}
                title={t('profile:projects.noContributedProjects', 'Görev aldığınız proje bulunamadı')}
                description={
                  member?.isLinked
                    ? t(
                        'profile:projects.noContributedProjectsDesc',
                        'Proje sahipleri veya yöneticiler sizi proje ekibine eklediğinde burada listelenecektir.'
                      )
                    : t(
                        'profile:projects.noContributedNotLinkedDesc',
                        'Hesabınız henüz kurumsal rehberdeki bir kişiyle eşleştirilmemiştir.'
                      )
                }
              />
            </Card>
          ) : (
            <div className="profile-projects-grid">
              {profile.contributedProjects.map((project: ProfileProjectItem) => (
                <article key={project.id} className="profile-project-card">
                  <div>
                    <div className="profile-project-card__top">
                      <div className="profile-project-card__badges">
                        <Badge variant="navy" size="sm">
                          {project.categoryName || project.categoryCode}
                        </Badge>
                        <Badge variant={project.isPublished ? 'success' : 'warning'} size="sm">
                          {project.statusName || (project.isPublished ? t('profile:projects.published', 'Yayında') : t('profile:projects.draft', 'Taslak'))}
                        </Badge>
                      </div>
                    </div>
                    <h3 className="profile-project-card__title">
                      <Link to={`/projects/${project.slug}`} className="hover:underline">
                        {project.name}
                      </Link>
                    </h3>
                    {project.shortDescription && (
                      <p className="profile-project-card__desc">{project.shortDescription}</p>
                    )}
                  </div>

                  <div>
                    <div className="profile-project-card__meta">
                      <span className="flex items-center gap-1">
                        <Calendar size={13} aria-hidden="true" />
                        <span>{new Date(project.createdAt).toLocaleDateString('tr-TR')}</span>
                      </span>
                    </div>

                    <div className="profile-project-card__actions">
                      <Link to={`/projects/${project.slug}`} className="btn btn--secondary btn--sm w-full justify-center">
                        <ExternalLink size={14} className="mr-1" />
                        <span>{t('profile:actions.viewProject', 'İncele')}</span>
                      </Link>
                    </div>
                  </div>
                </article>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ─── Tab 4: Account Security ─── */}
      {activeTab === 'security' && (
        <div id="panel-security" role="tabpanel" aria-labelledby="tab-security">
          <div className="profile-security-card">
            <div className="profile-security-header">
              <h2 className="profile-security-header__title">{t('profile:security.subtitle', 'Şifrenizi Değiştir')}</h2>
              <p className="profile-security-header__desc">
                {t('profile:security.description', 'Hesabınız için güçlü ve benzersiz bir parola kullanın.')}
              </p>
            </div>

            {passwordSuccess && (
              <div className="profile-alert profile-alert--success" role="alert">
                <CheckCircle2 size={18} className="flex-shrink-0" />
                <span>{passwordSuccess}</span>
              </div>
            )}

            {passwordError && (
              <div className="profile-alert profile-alert--error" role="alert">
                <AlertCircle size={18} className="flex-shrink-0" />
                <span>{passwordError}</span>
              </div>
            )}

            <form onSubmit={handlePasswordSubmit}>
              <div className="profile-form-group">
                <label className="profile-form-label" htmlFor="current-password">
                  {t('profile:security.currentPassword', 'Mevcut Şifre')}
                </label>
                <div className="profile-input-wrapper">
                  <input
                    id="current-password"
                    type={showCurrentPassword ? 'text' : 'password'}
                    className="profile-input"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    placeholder={t('profile:security.currentPasswordPlaceholder', 'Mevcut parolanızı girin')}
                    required
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    className="profile-input-toggle-btn"
                    onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                    aria-label={showCurrentPassword ? t('profile:security.hidePassword') : t('profile:security.showPassword')}
                  >
                    {showCurrentPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div className="profile-form-group">
                <label className="profile-form-label" htmlFor="new-password">
                  {t('profile:security.newPassword', 'Yeni Şifre')}
                </label>
                <div className="profile-input-wrapper">
                  <input
                    id="new-password"
                    type={showNewPassword ? 'text' : 'password'}
                    className="profile-input"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder={t('profile:security.newPasswordPlaceholder', 'En az 6 karakter')}
                    required
                    minLength={6}
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    className="profile-input-toggle-btn"
                    onClick={() => setShowNewPassword(!showNewPassword)}
                    aria-label={showNewPassword ? t('profile:security.hidePassword') : t('profile:security.showPassword')}
                  >
                    {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div className="profile-form-group">
                <label className="profile-form-label" htmlFor="confirm-password">
                  {t('profile:security.confirmPassword', 'Yeni Şifre Tekrar')}
                </label>
                <div className="profile-input-wrapper">
                  <input
                    id="confirm-password"
                    type={showConfirmPassword ? 'text' : 'password'}
                    className="profile-input"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder={t('profile:security.confirmPasswordPlaceholder', 'Yeni şifrenizi tekrar girin')}
                    required
                    minLength={6}
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    className="profile-input-toggle-btn"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    aria-label={showConfirmPassword ? t('profile:security.hidePassword') : t('profile:security.showPassword')}
                  >
                    {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div className="flex justify-end pt-2">
                <Button
                  type="submit"
                  variant="primary"
                  isLoading={passwordLoading}
                  leftIcon={<Lock size={16} />}
                >
                  {passwordLoading
                    ? t('profile:actions.updating', 'Güncelleniyor...')
                    : t('profile:actions.updatePassword', 'Şifreyi Güncelle')}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </PageLayout>
  );
}

export default ProfilePage;
