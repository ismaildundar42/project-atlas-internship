export interface FormattedImportError {
  title: string;
  description: string;
  remedy: string;
}

export const ERROR_CODE_TRANSLATIONS: Record<string, FormattedImportError> = {
  REQUIRED_FIELD: {
    title: 'Zorunlu Alan Eksik',
    description: 'Bu alan proje kaydı için zorunludur ve boş bırakılamaz.',
    remedy: 'Lütfen Excel dosyanızdaki ilgili hücreye geçerli bir değer giriniz.',
  },
  MAX_LENGTH_EXCEEDED: {
    title: 'Maksimum Karakter Sınırı Aşıldı',
    description: 'Girilen metin izin verilen maksimum karakter sınırını aşıyor.',
    remedy: 'Lütfen metin uzunluğunu belirtilen karakter sınırına uyacak şekilde kısaltınız.',
  },
  LOOKUP_NOT_FOUND: {
    title: 'Tanımsız Referans Değeri',
    description: 'Girilen referans değeri sistemde bulunamadı.',
    remedy: 'Lütfen sistemdeki tanımlı isim veya kodlarla birebir eşleştiğinden emin olunuz.',
  },
  LOOKUP_AMBIGUOUS: {
    title: 'Belirsiz / Çoklu Eşleşme',
    description: 'Girilen metin birden fazla kayıtla eşleştiği için kesin seçim yapılamadı.',
    remedy: 'Lütfen referans adını daha açık veya benzersiz kodunu kullanarak yazınız.',
  },
  DUPLICATE_PROJECT: {
    title: 'Mükerrer Proje (Veritabanı Çakışması)',
    description: 'Aynı ada veya benzer bağlantı adresine (slug) sahip bir proje sistemde zaten kayıtlıdır.',
    remedy: 'Lütfen proje adını mevcut projelerden farklı olacak şekilde güncelleyiniz.',
  },
  DUPLICATE_PROJECT_IN_FILE: {
    title: 'Dosya İçi Mükerrer Proje Adı',
    description: 'Aynı proje adı Excel dosyasının içinde birden fazla satırda tekrar etmektedir.',
    remedy: 'Excel dosyasındaki mükerrer satırları kaldırınız veya proje adlarını farklılaştırınız.',
  },
  INVALID_DATE: {
    title: 'Geçersiz Tarih Formatı',
    description: 'Girilen tarih formatı anlaşılamadı.',
    remedy: 'Tarihleri YYYY-AA-GG (ör. 2026-05-15) formatında veya standart Excel tarih hücresi olarak giriniz.',
  },
  INVALID_DATE_RANGE: {
    title: 'Geçersiz Tarih Aralığı',
    description: 'Bitiş tarihi başlangıç tarihinden önce olamaz.',
    remedy: 'Bitiş tarihinin başlangıç tarihinden sonraki bir tarih olduğunu kontrol ediniz.',
  },
  INVALID_BOOLEAN: {
    title: 'Geçersiz Mantıksal Değer',
    description: 'Doğru/Yanlış alanı için beklenen değer girilmedi.',
    remedy: 'Bu alana yalnızca EVET, HAYIR, true veya false yazabilirsiniz.',
  },
  INVALID_URL: {
    title: 'Geçersiz Web Bağlantısı (URL)',
    description: 'URL formatı geçersiz veya güvenlik riski içeren bir protokol (javascript:, data:) içeriyor.',
    remedy: 'Bağlantının http:// veya https:// ile başladığından ve geçerli bir adres olduğundan emin olunuz.',
  },
  INVALID_DEVELOPMENT_TYPE: {
    title: 'Geçersiz Geliştirme Tipi',
    description: 'Geliştirme tipi değeri kabul edilen tiplerden biri değil.',
    remedy: 'Internal (İç Kaynak), External (Dış Kaynak) veya Hybrid (Hibrit) değerlerinden birini giriniz.',
  },
  FORMULA_NOT_ALLOWED: {
    title: 'Formül Hücresi Desteklenmiyor',
    description: 'Excel dosyasında hesaplanan formül hücreleri tespit edildi.',
    remedy: 'Güvenlik nedeniyle formülleri Excel içinde kopyalayıp "Değer Olarak Yapıştır" (Paste as Values) yaptıktan sonra tekrar yükleyiniz.',
  },
  FILE_EMPTY: {
    title: 'Dosya Boş',
    description: 'Yüklenen Excel dosyasında hiç veri bulunmuyor.',
    remedy: 'Lütfen geçerli veri içeren bir dosya yükleyiniz.',
  },
  FILE_TOO_LARGE: {
    title: 'Dosya Boyutu Çok Büyük',
    description: 'Excel dosyası 10 MB sınırını aşıyor.',
    remedy: 'Lütfen dosya boyutunu küçülterek tekrar deneyiniz.',
  },
  UNSUPPORTED_FILE_TYPE: {
    title: 'Desteklenmeyen Dosya Türü',
    description: 'Yalnızca standart .xlsx formatı kabul edilmektedir.',
    remedy: 'Dosyayı Excel üzerinden .xlsx (Excel Çalışma Kitabı) olarak kaydedip tekrar yükleyiniz.',
  },
  INVALID_FILE_TOKEN: {
    title: 'Oturum Süresi Doldu',
    description: 'Yükleme oturumunun süresi dolmuş veya geçersiz bir dosya oturumu kullanılmış.',
    remedy: 'Lütfen dosyanızı ilk adımdan yeniden yükleyiniz.',
  },
  CONFIRM_LOCK_FAILED: {
    title: 'İşlem Devam Ediyor',
    description: 'Bu dosya için bir içe aktarım işlemi zaten başlatıldı veya onaylandı.',
    remedy: 'Lütfen sayfanın tamamlanmasını bekleyiniz veya yeni bir dosya yükleyiniz.',
  },
};

export function getFormattedImportError(errorCode: string, rawMessage?: string): FormattedImportError {
  if (ERROR_CODE_TRANSLATIONS[errorCode]) {
    return ERROR_CODE_TRANSLATIONS[errorCode];
  }

  return {
    title: 'Doğrulama Sorunu',
    description: rawMessage || 'Belirtilen hücredeki veri doğrulama kurallarına uymuyor.',
    remedy: 'Lütfen hücre içeriğini kontrol edip düzeltiniz.',
  };
}
