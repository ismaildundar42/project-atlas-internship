import { useState, type FormEvent, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Lock,
  Mail,
  Eye,
  EyeOff,
  AlertCircle,
  ArrowLeft,
  RotateCw,
  ShieldCheck,
} from 'lucide-react';
import { useAuth } from '../hooks/useAuth';
import { authService } from '../services/authService';
import type { CaptchaChallenge } from '../types/auth';
import demirExportLogo from '../assets/brand/demir-export-logo.png';
import kocLogo from '../assets/brand/koc-logo.png';
import Button from '../components/ui/Button';
import LanguageSwitcher from '../components/navigation/LanguageSwitcher';

export function LoginPage() {
  const { t } = useTranslation(['common', 'validation', 'projects']);
  const { login, isAuthenticated, isAdmin, canCreateProjects, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // ─── First-Party CAPTCHA State ─────────────────────────────────────────────
  const [captcha, setCaptcha] = useState<CaptchaChallenge | null>(null);
  const [captchaAnswer, setCaptchaAnswer] = useState('');
  const [isCaptchaLoading, setIsCaptchaLoading] = useState(false);
  const [captchaError, setCaptchaError] = useState(false);

  const fetchCaptcha = async () => {
    try {
      setIsCaptchaLoading(true);
      setCaptchaError(false);
      const data = await authService.getCaptcha();
      setCaptcha(data);
      setCaptchaAnswer('');
    } catch (err) {
      console.error('Failed to load CAPTCHA challenge:', err);
      setCaptchaError(true);
    } finally {
      setIsCaptchaLoading(false);
    }
  };

  useEffect(() => {
    fetchCaptcha();
  }, []);

  const requestedFrom = (location.state as { from?: { pathname: string } })?.from?.pathname;

  const getDefaultRedirect = (adminUser: boolean, canCreate: boolean) => {
    if (requestedFrom && requestedFrom !== '/login' && requestedFrom !== '/admin') {
      return requestedFrom;
    }
    if (adminUser) return '/admin';
    if (canCreate) return '/admin/projects';
    return '/projects';
  };

  useEffect(() => {
    if (!isAuthLoading && isAuthenticated) {
      navigate(getDefaultRedirect(isAdmin, canCreateProjects), { replace: true });
    }
  }, [isAuthenticated, isAdmin, canCreateProjects, isAuthLoading, navigate]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!email.trim() || !password) {
      setErrorMessage(t('validation.requiredCredentials', 'Lütfen e-posta adresi ve parolanızı giriniz.'));
      return;
    }

    if (captcha && !captchaAnswer.trim()) {
      setErrorMessage(t('auth.login.captchaRequired', 'Lütfen güvenlik doğrulama kodunu giriniz.'));
      return;
    }

    try {
      setIsSubmitting(true);
      const user = await login({
        email: email.trim(),
        password,
        captchaChallengeId: captcha?.challengeId,
        captchaAnswer: captchaAnswer.trim(),
      });
      const target = getDefaultRedirect(user.isAdmin, user.canCreateProjects);
      navigate(target, { replace: true });
    } catch (err: any) {
      let detail = err?.response?.data?.detail;
      if (!detail) {
        if (err?.message === 'Network Error' || !err?.response) {
          detail = t('common.networkError', 'Sunucuya bağlanılamadı. Lütfen sunucu bağlantısını kontrol edin.');
        } else {
          detail = t('validation.invalidCredentials', 'E-posta adresi veya parola hatalı.');
        }
      }
      setErrorMessage(detail);
      // Giriş hatasında (parola veya CAPTCHA hatası) yeni bir CAPTCHA meydan okuması üret
      fetchCaptcha();
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-page__container">
        {/* LEFT / BRAND EXPERIENCE ZONE (approx 62%) */}
        <div className="login-page__brand-zone">
          {/* Brand Header */}
          <div className="login-page__brand-header">
            <div className="login-page__de-logo-container">
              <img
                src={demirExportLogo}
                alt="Demir Export"
                className="login-page__de-logo"
              />
            </div>
            <div className="login-page__sys-code">SYS-ID: DE-PK-2026</div>
          </div>

          {/* Abstract Custom Project Intelligence Network SVG Graphic */}
          <svg
            className="login-page__network-graphic"
            viewBox="0 0 520 480"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            aria-hidden="true"
          >
            {/* Grid Coordinates Lines */}
            <line x1="40" y1="80" x2="480" y2="80" stroke="rgba(255,255,255,0.06)" strokeDasharray="4 4" />
            <line x1="40" y1="240" x2="480" y2="240" stroke="rgba(255,255,255,0.08)" strokeDasharray="4 4" />
            <line x1="40" y1="400" x2="480" y2="400" stroke="rgba(255,255,255,0.06)" strokeDasharray="4 4" />
            <line x1="160" y1="40" x2="160" y2="440" stroke="rgba(255,255,255,0.06)" strokeDasharray="4 4" />
            <line x1="320" y1="40" x2="320" y2="440" stroke="rgba(255,255,255,0.06)" strokeDasharray="4 4" />

            {/* Network Connection Lines */}
            <path d="M 260 240 L 140 120" stroke="rgba(255,255,255,0.18)" strokeWidth="1.5" />
            <path d="M 260 240 L 380 120" stroke="rgba(255,255,255,0.18)" strokeWidth="1.5" />
            <path d="M 260 240 L 120 360" stroke="rgba(255,255,255,0.18)" strokeWidth="1.5" />
            <path d="M 260 240 L 400 360" stroke="rgba(255,255,255,0.18)" strokeWidth="1.5" />
            <path d="M 260 240 L 260 90" stroke="rgba(197,22,5,0.4)" strokeWidth="1.5" />
            <path d="M 140 120 L 380 120" stroke="rgba(255,255,255,0.1)" strokeWidth="1" strokeDasharray="2 2" />
            <path d="M 120 360 L 400 360" stroke="rgba(255,255,255,0.1)" strokeWidth="1" strokeDasharray="2 2" />

            {/* Central PROJE Node */}
            <circle cx="260" cy="240" r="28" fill="#0f1d32" stroke="#c51605" strokeWidth="2" />
            <circle cx="260" cy="240" r="8" fill="#c51605" />
            <text x="260" y="282" fill="#ffffff" fontSize="11" fontWeight="700" textAnchor="middle" letterSpacing="1">{t('projects.detail.networkProject', 'PROJE')}</text>

            {/* Peripheral Nodes & Micro Labels */}
            {/* SAHA */}
            <circle cx="140" cy="120" r="14" fill="#0b1728" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5" />
            <circle cx="140" cy="120" r="4" fill="#64748b" />
            <text x="140" y="96" fill="#94a3b8" fontSize="10" fontWeight="600" textAnchor="middle">{t('projects.detail.networkSite', 'SAHA')}</text>

            {/* TEKNOLOJİ */}
            <circle cx="380" cy="120" r="14" fill="#0b1728" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5" />
            <circle cx="380" cy="120" r="4" fill="#64748b" />
            <text x="380" y="96" fill="#94a3b8" fontSize="10" fontWeight="600" textAnchor="middle">{t('projects.detail.networkTech', 'TEKNOLOJİ')}</text>

            {/* EKİP */}
            <circle cx="260" cy="90" r="12" fill="#0b1728" stroke="#ff5240" strokeWidth="1.5" />
            <circle cx="260" cy="90" r="4" fill="#ff5240" />
            <text x="260" y="70" fill="#ff6b5b" fontSize="10" fontWeight="700" textAnchor="middle">{t('projects.detail.networkTeam', 'EKİP')}</text>

            {/* VERİ */}
            <circle cx="120" cy="360" r="14" fill="#0b1728" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5" />
            <circle cx="120" cy="360" r="4" fill="#64748b" />
            <text x="120" y="390" fill="#94a3b8" fontSize="10" fontWeight="600" textAnchor="middle">{t('projects.detail.networkData', 'VERİ')}</text>

            {/* ENTEGRASYON */}
            <circle cx="400" cy="360" r="14" fill="#0b1728" stroke="rgba(255,255,255,0.3)" strokeWidth="1.5" />
            <circle cx="400" cy="360" r="4" fill="#64748b" />
            <text x="400" y="390" fill="#94a3b8" fontSize="10" fontWeight="600" textAnchor="middle">{t('projects.detail.networkIntegration', 'ENTEGRASYON')}</text>

            {/* Micro Coordinate Markings */}
            <text x="50" y="75" fill="#334155" fontSize="8" fontFamily="monospace">N: 39°45'</text>
            <text x="430" y="75" fill="#334155" fontSize="8" fontFamily="monospace">E: 37°01'</text>
            <text x="50" y="420" fill="#334155" fontSize="8" fontFamily="monospace">NODE-01</text>
            <text x="430" y="420" fill="#334155" fontSize="8" fontFamily="monospace">NODE-04</text>
          </svg>

          {/* Brand Main Content Area */}
          <div className="login-page__brand-content">
            <div className="login-page__product-lockup">
              <div className="login-page__product-eyebrow">
                <span className="login-page__product-eyebrow-line" />
                <span>{t('auth.login.corporateSystem', 'KURUMSAL BİLGİ SİSTEMİ')}</span>
              </div>

              <h1 className="login-page__product-title">
                {t('auth.login.projectTitleWord', 'PROJE')}
                <span className="login-page__product-title-highlight">{t('auth.login.libraryTitleWord', 'KÜTÜPHANESİ')}</span>
              </h1>

              <p className="login-page__product-subtitle">
                {t('auth.login.productSubtitle', 'Demir Export kurumsal proje, Ar-Ge ve teknoloji bilgi platformu.')}
              </p>
            </div>

            {/* Single Concise Product Statement */}
            <div className="login-page__statement">
              <p className="login-page__statement-text">
                "{t('auth.login.statementQuote', 'Projeleri keşfedin. Bilgiyi kurumsal hafızaya dönüştürün.')}"
              </p>
              <span className="login-page__statement-subtext">
                {t('auth.login.departmentName', 'DEMİR EXPORT DİJİTAL DÖNÜŞÜM DİREKTÖRLÜĞÜ')}
              </span>
            </div>
          </div>

          {/* Brand Footer: Koç Corporate Affiliation Strip */}
          <div className="login-page__brand-footer">
            <div className="login-page__koc-affiliation">
              <span className="login-page__koc-text">BİR KOÇ TOPLULUĞU ŞİRKETİ</span>
              <img src={kocLogo} alt="Koç Holding" className="login-page__koc-logo" />
            </div>
            <span className="login-page__copyright">© 2026 Demir Export A.Ş.</span>
          </div>
        </div>

        {/* RIGHT / AUTHENTICATION ZONE (approx 38%) */}
        <div className="login-page__auth-zone">
          <div className="login-page__auth-container">
            <div className="login-page__auth-card">
              <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '8px' }}>
                <LanguageSwitcher variant="compact" />
              </div>

              <div className="login-page__auth-header">
                <h2 className="login-page__auth-title">{t('auth.login.adminLoginTitle', 'Yönetim Paneline Giriş')}</h2>

                <p className="login-page__auth-desc">
                  {t('auth.login.adminLoginDesc', 'Proje Kütüphanesi yönetim araçlarına erişmek için kurumsal hesabınızla giriş yapın.')}
                </p>
              </div>

              <form onSubmit={handleSubmit} className="login-page__form" noValidate>
                {errorMessage && (
                  <div className="login-page__error-alert" role="alert">
                    <AlertCircle size={18} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
                    <span>{errorMessage}</span>
                  </div>
                )}

                {/* Email Field */}
                <div className="login-page__field-group">
                  <label htmlFor="login-email" className="login-page__label">
                    {t('auth.login.emailLabel', 'Kurumsal E-posta Adresi')}
                  </label>
                  <div className="login-page__input-wrapper">
                    <Mail size={18} className="login-page__input-icon" aria-hidden="true" />
                    <input
                      id="login-email"
                      type="email"
                      className="login-page__input"
                      placeholder="ad.soyad@demirexport.com"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      autoComplete="username"
                      required
                      disabled={isSubmitting}
                    />
                  </div>
                </div>

                {/* Password Field */}
                <div className="login-page__field-group">
                  <label htmlFor="login-password" className="login-page__label">
                    {t('auth.login.passwordLabel', 'Parola')}
                  </label>
                  <div className="login-page__input-wrapper">
                    <Lock size={18} className="login-page__input-icon" aria-hidden="true" />
                    <input
                      id="login-password"
                      type={showPassword ? 'text' : 'password'}
                      className="login-page__input"
                      placeholder="••••••••"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      autoComplete="current-password"
                      required
                      disabled={isSubmitting}
                      style={{ paddingRight: '40px' }}
                    />
                    <button
                      type="button"
                      className="login-page__password-toggle"
                      onClick={() => setShowPassword(!showPassword)}
                      aria-label={showPassword ? t('auth.login.hidePassword', 'Parolayı gizle') : t('auth.login.showPassword', 'Parolayı göster')}
                      tabIndex={0}
                    >
                      {showPassword ? <EyeOff size={18} aria-hidden="true" /> : <Eye size={18} aria-hidden="true" />}
                    </button>
                  </div>
                </div>

                {/* First-Party CAPTCHA Challenge Field */}
                <div className="login-page__field-group login-page__captcha-group">
                  <label htmlFor="login-captcha" className="login-page__label">
                    {t('auth.login.captchaLabel', 'Doğrulama Kodu')}
                  </label>

                  <div className="login-page__captcha-row">
                    {/* Left: CAPTCHA Input */}
                    <div className="login-page__input-wrapper login-page__captcha-input-wrap">
                      <ShieldCheck size={18} className="login-page__input-icon" aria-hidden="true" />
                      <input
                        id="login-captcha"
                        type="text"
                        className="login-page__input login-page__captcha-input"
                        placeholder={t('auth.login.captchaPlaceholder', 'Kodu giriniz')}
                        value={captchaAnswer}
                        onChange={(e) => setCaptchaAnswer(e.target.value.toUpperCase())}
                        maxLength={6}
                        autoComplete="off"
                        autoCorrect="off"
                        autoCapitalize="characters"
                        spellCheck={false}
                        required
                        disabled={isSubmitting || isCaptchaLoading}
                      />
                    </div>

                    {/* Right: CAPTCHA Visual Surface & Refresh Action */}
                    <div className="login-page__captcha-preview-wrap" aria-live="polite">
                      {isCaptchaLoading && !captcha ? (
                        <div className="login-page__captcha-skeleton">
                          <RotateCw size={15} className="login-page__spin" aria-hidden="true" />
                          <span>{t('auth.login.captchaLoading', 'Yükleniyor…')}</span>
                        </div>
                      ) : captchaError ? (
                        <button
                          type="button"
                          onClick={fetchCaptcha}
                          className="login-page__captcha-error-retry"
                          title={t('auth.login.captchaLoadError', 'Kod yüklenemedi. Yenilemek için tıklayın.')}
                        >
                          <AlertCircle size={15} aria-hidden="true" />
                        </button>
                      ) : captcha?.imageDataUrl ? (
                        <img
                          src={captcha.imageDataUrl}
                          alt={t('auth.login.captchaLabel', 'Doğrulama Kodu')}
                          className="login-page__captcha-img"
                          draggable={false}
                        />
                      ) : null}

                      <button
                        type="button"
                        className="login-page__captcha-refresh-btn"
                        onClick={fetchCaptcha}
                        disabled={isCaptchaLoading || isSubmitting}
                        aria-label={t('auth.login.captchaRefresh', 'Kodu yenile')}
                        title={t('auth.login.captchaRefresh', 'Kodu yenile')}
                      >
                        <RotateCw size={15} className={isCaptchaLoading ? 'login-page__spin' : ''} aria-hidden="true" />
                      </button>
                    </div>
                  </div>
                </div>

                {/* Submit Action */}
                <div style={{ marginTop: 'var(--space-2)' }}>
                  <Button
                    type="submit"
                    variant="primary"
                    size="lg"
                    className="login-page__submit-btn"
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? t('auth.login.loggingIn', 'Giriş Yapılıyor...') : t('auth.login.loginButton', 'Giriş Yap')}
                  </Button>
                </div>

                {/* Public Back Action */}
                <div className="login-page__back-link">
                  <Link to="/projects" className="login-page__back-button">
                    <ArrowLeft size={16} aria-hidden="true" /> {t('projects.detail.backToLibrary', 'Proje Kütüphanesine Dön')}
                  </Link>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
