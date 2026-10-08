export interface SystemFieldDefinition {
  key: string;
  label: string;
  required: boolean;
  category: 'core' | 'team' | 'tech' | 'narrative' | 'dates' | 'links' | 'settings';
  description: string;
}

export const SYSTEM_IMPORT_FIELDS: SystemFieldDefinition[] = [
  // Zorunlu Temel Alanlar
  {
    key: 'name',
    label: 'Proje Adı *',
    required: true,
    category: 'core',
    description: 'Projenin tam adı (Maks. 200 karakter, benzersiz olmalıdır).',
  },
  {
    key: 'shortDescription',
    label: 'Kısa Açıklama *',
    required: true,
    category: 'core',
    description: 'Proje kartlarında ve liste görünümünde gösterilecek özet (Maks. 500 karakter).',
  },
  {
    key: 'category',
    label: 'Kategori *',
    required: true,
    category: 'core',
    description: 'Sistemde tanımlı proje kategorisi adı veya kodu (ör. Yazılım, Yapay Zeka).',
  },
  {
    key: 'status',
    label: 'Durum *',
    required: true,
    category: 'core',
    description: 'Projenin güncel durumu (ör. Aktif, Planlama, Pilot, Tamamlandı).',
  },

  // Ekipler ve Kişiler
  {
    key: 'primaryTeam',
    label: 'Sorumlu Ekip',
    required: false,
    category: 'team',
    description: 'Projeden birincil sorumlu ana ekip (ör. Yazılım Geliştirme Ekibi).',
  },
  {
    key: 'supportingTeams',
    label: 'Destekleyen Ekipler',
    required: false,
    category: 'team',
    description: 'Noktalı virgül (;) ile ayrılmış yardımcı ekipler listesi.',
  },
  {
    key: 'members',
    label: 'Proje Üyeleri',
    required: false,
    category: 'team',
    description: 'Noktalı virgül (;) ile ayrılmış üye e-posta adresleri veya ad-soyadları.',
  },

  // Teknik ve Saha Özellikleri
  {
    key: 'developmentType',
    label: 'Geliştirme Tipi',
    required: false,
    category: 'tech',
    description: 'Internal, External veya CoDevelopment (Boş bırakılırsa Internal varsayılır).',
  },
  {
    key: 'technologies',
    label: 'Teknolojiler',
    required: false,
    category: 'tech',
    description: 'Noktalı virgül (;) ile ayrılmış mevcut teknolojiler (ör. React; .NET 9).',
  },
  {
    key: 'locations',
    label: 'Lokasyonlar',
    required: false,
    category: 'tech',
    description: 'Noktalı virgül (;) ile ayrılmış maden/ofis lokasyonları.',
  },
  {
    key: 'tags',
    label: 'Etiketler',
    required: false,
    category: 'tech',
    description: 'Noktalı virgül (;) ile ayrılmış sistemde tanımlı etiketler.',
  },

  // Detay Metinleri & Anlatım
  {
    key: 'description',
    label: 'Genel Açıklama',
    required: false,
    category: 'narrative',
    description: 'Projenin kapsamlı detaylı açıklaması.',
  },
  {
    key: 'purpose',
    label: 'Amaç',
    required: false,
    category: 'narrative',
    description: 'Projenin hayata geçirilme amacı.',
  },
  {
    key: 'problemSolved',
    label: 'Çözülen Problem',
    required: false,
    category: 'narrative',
    description: 'Bu projenin sahada veya kurumda çözdüğü darboğaz/sorun.',
  },
  {
    key: 'nonTechnicalDescription',
    label: 'Teknik Olmayan Açıklama',
    required: false,
    category: 'narrative',
    description: 'Yöneticiler ve genel kullanıcılar için sade anlatım.',
  },
  {
    key: 'technicalDescription',
    label: 'Teknik Açıklama',
    required: false,
    category: 'narrative',
    description: 'Mimari ve teknik altyapı detayları.',
  },
  {
    key: 'businessImpact',
    label: 'İş Etkisi',
    required: false,
    category: 'narrative',
    description: 'Maliyet tasarrufu, zaman kazanımı veya verimlilik etkisi.',
  },
  {
    key: 'targetAudience',
    label: 'Hedef Kitle',
    required: false,
    category: 'narrative',
    description: 'Uygulamayı kullanan hedef kullanıcı kitlesi.',
  },
  {
    key: 'accessInstructions',
    label: 'Erişim Talimatları',
    required: false,
    category: 'narrative',
    description: 'Uygulamaya nasıl erişileceğine dair bilgiler.',
  },

  // Tarihler
  {
    key: 'startDate',
    label: 'Başlangıç Tarihi',
    required: false,
    category: 'dates',
    description: 'YYYY-AA-GG formatında veya standart Excel tarih hücresi.',
  },
  {
    key: 'endDate',
    label: 'Bitiş Tarihi',
    required: false,
    category: 'dates',
    description: 'YYYY-AA-GG formatında veya standart Excel tarih hücresi.',
  },

  // Bağlantılar ve Görseller
  {
    key: 'applicationUrl',
    label: 'Canlı Uygulama URL',
    required: false,
    category: 'links',
    description: 'Canlı sistem web adresi (http:// veya https:// ile başlamalıdır).',
  },
  {
    key: 'repositoryUrl',
    label: 'Repository URL',
    required: false,
    category: 'links',
    description: 'Kaynak kod deposu bağlantısı.',
  },
  {
    key: 'coverImageUrl',
    label: 'Kapak Görseli URL',
    required: false,
    category: 'links',
    description: 'Proje kapak görseli internet bağlantısı.',
  },

  // Ayarlar
  {
    key: 'isFeatured',
    label: 'Öne Çıkan',
    required: false,
    category: 'settings',
    description: 'EVET, HAYIR, true veya false (Boş ise HAYIR varsayılır).',
  },
];

export const REQUIRED_SYSTEM_FIELDS = SYSTEM_IMPORT_FIELDS.filter((f) => f.required);
export const OPTIONAL_SYSTEM_FIELDS = SYSTEM_IMPORT_FIELDS.filter((f) => !f.required);
