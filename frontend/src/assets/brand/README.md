# Brand Assets — Demir Export Proje Kütüphanesi

Bu klasör, uygulamanın marka görsellerini (logo, ikon vb.) barındırmak için ayrılmıştır.

## Durum

Resmi Koç Holding / Demir Export logo asset'leri henüz teslim edilmemiştir.
Uygulamada logo yokken devreye giren `BrandMark` bileşeni geçici görsel sunar.

## Logo Ekleme Kılavuzu

Gerçek logo dosyaları temin edildiğinde aşağıdaki adımları izleyin:

### 1. Dosyaları Bu Klasöre Kopyalayın

Önerilen dosya isimleri:

```
src/assets/brand/
├── demir-export-logo.svg      ← Tercih edilen (vektör, ölçeklenir)
├── demir-export-logo.png      ← Alternatif (yüksek çözünürlük, 2x)
├── koc-holding-logo.svg       ← Koç Holding logo (gerekiyorsa)
└── favicon.svg                ← Tarayıcı sekmesi ikonu (public/favicon.svg'e kopyalayın)
```

### 2. `BrandMark` Bileşenini Güncelleyin

`src/components/brand/BrandMark.tsx` dosyasında yorum satırları göreceksiniz.

```tsx
// Gerçek logo eklendiğinde yorumdan kaldırın:
// import demirExportLogo from '../../assets/brand/demir-export-logo.svg';
```

Sadece bu satırı yorumdan kaldırıp `img` elemanını etkinleştirin.
**Başka hiçbir bileşeni veya dosyayı değiştirmenize gerek yoktur.**

### 3. Erişilebilirlik

Logo `<img>` kullandığında, anlamlı `alt` metin kullanın:

```tsx
<img src={demirExportLogo} alt="Demir Export" />
```

Dekoratif kullanımlarda `alt=""` ile ekran okuyuculardan gizleyin.

## Boyut Kılavuzu

| Kullanım         | Önerilen Format | Minimum Genişlik |
|------------------|-----------------|------------------|
| Sidebar header   | SVG             | 120px            |
| Mobile header    | SVG             | 96px             |
| Favicon          | SVG / ICO       | 32×32px          |

## Notlar

- SVG formatı tercih edilir (her çözünürlükte net görünür)
- PNG kullanılacaksa minimum 2x (retina) çözünürlük önerilir
- Logo dosyaları `.gitignore`'a eklenmemeli — kaynak kodla birlikte yönetilmeli
