import i18n, { changeAppLanguage, normalizeLanguage, LANGUAGE_STORAGE_KEY } from '../index';

export interface TestResult {
  name: string;
  passed: boolean;
  message?: string;
}

export async function runI18nVerificationSuite(): Promise<{ total: number; passed: number; failed: number; results: TestResult[] }> {
  const results: TestResult[] = [];

  const assert = (name: string, condition: boolean, message?: string) => {
    results.push({
      name,
      passed: condition,
      message: condition ? undefined : (message || 'Assertion failed'),
    });
  };

  // Storage key check
  assert('LANGUAGE_STORAGE_KEY is "demir-export-language"', LANGUAGE_STORAGE_KEY === 'demir-export-language');

  // 1. Language Normalization Tests
  assert('normalizeLanguage("tr") -> "tr"', normalizeLanguage('tr') === 'tr');
  assert('normalizeLanguage("tr-TR") -> "tr"', normalizeLanguage('tr-TR') === 'tr');
  assert('normalizeLanguage("en") -> "en"', normalizeLanguage('en') === 'en');
  assert('normalizeLanguage("en-US") -> "en"', normalizeLanguage('en-US') === 'en');
  assert('normalizeLanguage(null) -> "tr" fallback', normalizeLanguage(null) === 'tr');
  assert('normalizeLanguage("de") -> "tr" fallback', normalizeLanguage('de') === 'tr');

  // 2. Switch to TR and test Turkish keys
  await changeAppLanguage('tr');
  assert('i18n.language is "tr"', i18n.language === 'tr');

  // TopHeader & Navigation in TR
  assert('TR: navigation.dashboard is "Ana Sayfa"', i18n.t('navigation.dashboard') === 'Ana Sayfa');
  assert('TR: navigation.projectLibrary is "Proje Kütüphanesi"', i18n.t('navigation.projectLibrary') === 'Proje Kütüphanesi');
  assert('TR: navigation.reports is "Raporlar"', i18n.t('navigation.reports') === 'Raporlar');
  assert('TR: navigation.teams is "Ekipler"', i18n.t('navigation.teams') === 'Ekipler');
  assert('TR: navigation.adminCenter is "Yönetim Merkezi"', i18n.t('navigation.adminCenter') === 'Yönetim Merkezi');
  assert('TR: projects.searchPlaceholder is "Proje, teknoloji veya ekip ara..."', i18n.t('projects.searchPlaceholder') === 'Proje, teknoloji veya ekip ara...');

  // Dashboard in TR
  assert('TR: projects.dashboard.pageTitle is "Demir Export Proje Kütüphanesi"', i18n.t('projects.dashboard.pageTitle') === 'Demir Export Proje Kütüphanesi');
  assert('TR: projects.dashboard.totalProjects is "Toplam Proje"', i18n.t('projects.dashboard.totalProjects') === 'Toplam Proje');
  assert('TR: projects.dashboard.activeProjects is "Aktif Proje"', i18n.t('projects.dashboard.activeProjects') === 'Aktif Proje');
  assert('TR: projects.dashboard.inProgressProjects is "Devam Eden"', i18n.t('projects.dashboard.inProgressProjects') === 'Devam Eden');
  assert('TR: projects.dashboard.featuredProjects is "Öne Çıkan"', i18n.t('projects.dashboard.featuredProjects') === 'Öne Çıkan');
  assert('TR: projects.dashboard.statusDistribution is "Proje Durum Dağılımı"', i18n.t('projects.dashboard.statusDistribution') === 'Proje Durum Dağılımı');
  assert('TR: projects.dashboard.categoryDistribution is "Kategori Dağılımı"', i18n.t('projects.dashboard.categoryDistribution') === 'Kategori Dağılımı');
  
  // Interpolation in TR
  const trDistCount = i18n.t('projects.dashboard.projectDistributionCount', { count: 84, percentage: 100 });
  assert('TR: projectDistributionCount interpolates properly', trDistCount === '84 proje (%100)', `Received: "${trDistCount}"`);

  // Project Library in TR
  assert('TR: projects.library.title is "Proje Kütüphanesi"', i18n.t('projects.library.title') === 'Proje Kütüphanesi');
  assert('TR: projects.library.description is "Demir Export bünyesinde geliştirilen ve kullanılan projeleri keşfedin."', i18n.t('projects.library.description') === 'Demir Export bünyesinde geliştirilen ve kullanılan projeleri keşfedin.');
  assert('TR: projects.filters.searchPlaceholder is "Proje adı veya açıklama ara..."', i18n.t('projects.filters.searchPlaceholder') === 'Proje adı veya açıklama ara...');
  assert('TR: projects.filters.status is "Durum"', i18n.t('projects.filters.status') === 'Durum');
  assert('TR: projects.filters.allStatuses is "Tüm Durumlar"', i18n.t('projects.filters.allStatuses') === 'Tüm Durumlar');
  assert('TR: projects.filters.category is "Kategori"', i18n.t('projects.filters.category') === 'Kategori');
  assert('TR: projects.filters.allCategories is "Tüm Kategoriler"', i18n.t('projects.filters.allCategories') === 'Tüm Kategoriler');
  assert('TR: projects.filters.technology is "Teknoloji"', i18n.t('projects.filters.technology') === 'Teknoloji');
  assert('TR: projects.filters.allTechnologies is "Tüm Teknolojiler"', i18n.t('projects.filters.allTechnologies') === 'Tüm Teknolojiler');
  assert('TR: projects.filters.developmentType is "Geliştirme Tipi"', i18n.t('projects.filters.developmentType') === 'Geliştirme Tipi');
  assert('TR: projects.filters.allTypes is "Tüm Tipler"', i18n.t('projects.filters.allTypes') === 'Tüm Tipler');
  assert('TR: projects.filters.location is "Lokasyon"', i18n.t('projects.filters.location') === 'Lokasyon');
  assert('TR: projects.filters.allLocations is "Tüm Lokasyonlar"', i18n.t('projects.filters.allLocations') === 'Tüm Lokasyonlar');
  assert('TR: projects.filters.team is "Ekip"', i18n.t('projects.filters.team') === 'Ekip');
  assert('TR: projects.filters.allTeams is "Tüm Ekipler"', i18n.t('projects.filters.allTeams') === 'Tüm Ekipler');
  assert('TR: projects.library.sortBy is "Sırala:"', i18n.t('projects.library.sortBy') === 'Sırala:');
  assert('TR: projects.library.sortUpdatedDesc is "Son Güncellenen"', i18n.t('projects.library.sortUpdatedDesc') === 'Son Güncellenen');

  // Pagination in TR
  const trPagingRange = i18n.t('projects.pagination.showingRange', { total: 84, start: 1, end: 12 });
  assert('TR: pagination showingRange interpolates', trPagingRange === '84 projeden 1–12 arası gösteriliyor', `Received: "${trPagingRange}"`);
  assert('TR: pagination previous is "Önceki"', i18n.t('projects.pagination.previous') === 'Önceki');
  assert('TR: pagination next is "Sonraki"', i18n.t('projects.pagination.next') === 'Sonraki');

  // 3. Switch to EN and test English keys
  await changeAppLanguage('en');
  assert('i18n.language is "en"', i18n.language === 'en');

  // TopHeader & Navigation in EN
  assert('EN: navigation.dashboard is "Dashboard"', i18n.t('navigation.dashboard') === 'Dashboard');
  assert('EN: navigation.projectLibrary is "Project Library"', i18n.t('navigation.projectLibrary') === 'Project Library');
  assert('EN: navigation.reports is "Reports"', i18n.t('navigation.reports') === 'Reports');
  assert('EN: navigation.teams is "Teams"', i18n.t('navigation.teams') === 'Teams');
  assert('EN: navigation.adminCenter is "Administration"', i18n.t('navigation.adminCenter') === 'Administration');
  assert('EN: projects.searchPlaceholder is "Search projects, technologies or teams..."', i18n.t('projects.searchPlaceholder') === 'Search projects, technologies or teams...');

  // Dashboard in EN
  assert('EN: projects.dashboard.pageTitle is "Demir Export Project Library"', i18n.t('projects.dashboard.pageTitle') === 'Demir Export Project Library');
  assert('EN: projects.dashboard.totalProjects is "Total Projects"', i18n.t('projects.dashboard.totalProjects') === 'Total Projects');
  assert('EN: projects.dashboard.activeProjects is "Active Projects"', i18n.t('projects.dashboard.activeProjects') === 'Active Projects');
  assert('EN: projects.dashboard.inProgressProjects is "In Progress"', i18n.t('projects.dashboard.inProgressProjects') === 'In Progress');
  assert('EN: projects.dashboard.featuredProjects is "Featured"', i18n.t('projects.dashboard.featuredProjects') === 'Featured');
  assert('EN: projects.dashboard.statusDistribution is "Project Status Distribution"', i18n.t('projects.dashboard.statusDistribution') === 'Project Status Distribution');
  assert('EN: projects.dashboard.categoryDistribution is "Category Distribution"', i18n.t('projects.dashboard.categoryDistribution') === 'Category Distribution');

  // Interpolation in EN
  const enDistCount = i18n.t('projects.dashboard.projectDistributionCount', { count: 84, percentage: 100 });
  assert('EN: projectDistributionCount interpolates properly', enDistCount === '84 projects (100%)', `Received: "${enDistCount}"`);

  // Project Library in EN
  assert('EN: projects.library.title is "Project Library"', i18n.t('projects.library.title') === 'Project Library');
  assert('EN: projects.library.description is "Discover and explore projects developed and used across Demir Export."', i18n.t('projects.library.description') === 'Discover and explore projects developed and used across Demir Export.');
  assert('EN: projects.filters.searchPlaceholder is "Search by project name or description..."', i18n.t('projects.filters.searchPlaceholder') === 'Search by project name or description...');
  assert('EN: projects.filters.status is "Status"', i18n.t('projects.filters.status') === 'Status');
  assert('EN: projects.filters.allStatuses is "All Statuses"', i18n.t('projects.filters.allStatuses') === 'All Statuses');
  assert('EN: projects.filters.category is "Category"', i18n.t('projects.filters.category') === 'Category');
  assert('EN: projects.filters.allCategories is "All Categories"', i18n.t('projects.filters.allCategories') === 'All Categories');
  assert('EN: projects.filters.technology is "Technology"', i18n.t('projects.filters.technology') === 'Technology');
  assert('EN: projects.filters.allTechnologies is "All Technologies"', i18n.t('projects.filters.allTechnologies') === 'All Technologies');
  assert('EN: projects.filters.developmentType is "Development Type"', i18n.t('projects.filters.developmentType') === 'Development Type');
  assert('EN: projects.filters.allTypes is "All Types"', i18n.t('projects.filters.allTypes') === 'All Types');
  assert('EN: projects.filters.location is "Location"', i18n.t('projects.filters.location') === 'Location');
  assert('EN: projects.filters.allLocations is "All Locations"', i18n.t('projects.filters.allLocations') === 'All Locations');
  assert('EN: projects.filters.team is "Team"', i18n.t('projects.filters.team') === 'Team');
  assert('EN: projects.filters.allTeams is "All Teams"', i18n.t('projects.filters.allTeams') === 'All Teams');
  assert('EN: projects.library.sortBy is "Sort By:"', i18n.t('projects.library.sortBy') === 'Sort By:');
  assert('EN: projects.library.sortUpdatedDesc is "Recently Updated"', i18n.t('projects.library.sortUpdatedDesc') === 'Recently Updated');

  // Pagination in EN
  const enPagingRange = i18n.t('projects.pagination.showingRange', { total: 84, start: 1, end: 12 });
  assert('EN: pagination showingRange interpolates', enPagingRange === 'Showing 1–12 of 84 projects', `Received: "${enPagingRange}"`);
  assert('EN: pagination previous is "Previous"', i18n.t('projects.pagination.previous') === 'Previous');
  assert('EN: pagination next is "Next"', i18n.t('projects.pagination.next') === 'Next');

  // 4. Raw Key Safety: verify no key equals its key path
  const keysToTest = [
    'navigation.dashboard',
    'navigation.projectLibrary',
    'projects.dashboard.pageTitle',
    'projects.dashboard.projectDistributionCount',
    'projects.library.title',
    'projects.library.description',
    'projects.filters.status',
    'projects.filters.allStatuses',
    'projects.pagination.showingRange',
    'projects.pagination.previous',
    'projects.pagination.next',
    'common.actions.save',
    'workflow.actions.submitForReview',
    'reports.pageTitle',
    'excel.import.wizardTitle',
  ];

  for (const key of keysToTest) {
    const val = i18n.t(key);
    assert(`Raw key check: ${key} is not raw`, val !== key, `Key returned raw key string: "${val}"`);
  }

  // 5. Round trip back to TR
  await changeAppLanguage('tr');
  assert('Round trip back to TR: i18n.language is "tr"', i18n.language === 'tr');
  assert('Round trip back to TR: navigation.dashboard is "Ana Sayfa"', i18n.t('navigation.dashboard') === 'Ana Sayfa');

  const passed = results.filter((r) => r.passed).length;
  const failed = results.filter((r) => !r.passed).length;

  return {
    total: results.length,
    passed,
    failed,
    results,
  };
}
