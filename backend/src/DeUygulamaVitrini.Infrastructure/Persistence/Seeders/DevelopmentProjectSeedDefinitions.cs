using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Seeders;

/// <summary>
/// Demo ve geliştirme veriseti için proje taslak tanımları (Blueprint).
/// </summary>
public class ProjectBlueprint
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Purpose { get; set; }
    public string? ProblemSolved { get; set; }
    public string? NonTechnicalDescription { get; set; }
    public string? TechnicalDescription { get; set; }
    public string? BusinessImpact { get; set; }
    public string? TargetAudience { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";
    public string CategoryCode { get; set; } = "SOFTWARE";
    public DevelopmentType DevelopmentType { get; set; } = DevelopmentType.Internal;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; } = true;
    public string? CoverImageTheme { get; set; } // örn: "open-pit-mine" -> /uploads/projects/demo/open-pit-mine.svg
    public string PrimaryTeamName { get; set; } = "Yazılım Geliştirme Ekibi";
    public string? SecondaryTeamName { get; set; }
    public string[] MemberEmails { get; set; } = [];
    public string[] LocationNames { get; set; } = [];
    public string[] TechnologyNames { get; set; } = [];
    public string[] TagSlugs { get; set; } = [];
    public (string Name, IntegrationType Type, string Desc)[] Integrations { get; set; } = [];
    public (string Theme, string Title, string Caption)[] GalleryImages { get; set; } = [];
    public (string Name, string DocFile, string DocType, string Desc)[] Documents { get; set; } = [];
}

public static class DevelopmentProjectSeedDefinitions
{
    public static List<ProjectBlueprint> GetBlueprints()
    {
        return new List<ProjectBlueprint>
        {
            // ── 1. Konveyör Bant Anomali Tespiti ────────────────────────────────────
            new()
            {
                Name = "Konveyör Bant Anomali Tespiti",
                Slug = "konveyor-bant-anomali-tespiti",
                ShortDescription = "Konveyör bantlarındaki boyuna yırtılma, kayma ve yabancı metal parçalarını yüksek hızlı kameralarla tespit eden yapay zeka sistemi.",
                Description = "Kırıcı besleme ve cevher transfer hatlarındaki ana konveyör bantlarının optik kameralar ve derin öğrenme modelleri ile 7/24 kesintisiz izlenmesini sağlar. Sistem, kauçuk bant yüzeyi ve kord dokusundaki en ufak mekanik yırtılma veya yabancı metal batmasını anında teşhis ederek tesisi koruma altına alır.",
                Purpose = "Kırma-eleme ve zenginleştirme tesislerinde 7/24 çalışan yüksek debili ana konveyör bant hatlarını yapay zeka destekli bilgisayarlı görü sistemleri ile sürekli denetlemektir.\n\nBant yüzeyinde oluşabilecek boyuna yırtılmaları ilk 30-50 cm içerisinde tespit ederek tahrik motorlarını milisaniyeler mertebesinde acil durdurmak, böylece yüz binlerce dolarlık bant ekipmanı hasarını ve günlerce sürebilecek plansız tesis duruşlarını tamamen engellemektir.\n\nAyrıca bant ek yerlerinin (vulcanized splices) mekanik aşınma durumunu, kenar kaymalarını ve tahrik tamburundaki aşırı ısınmaları izleyerek kestirimci bakım süreçlerine veri sağlamayı hedefler.",
                ProblemSolved = "Açık ocak ve yeraltı madenciliğinde patlatılmış tüvenan cevherin taşınması sırasında kırıcı kovanlarından veya tahkimat malzemelerinden kopan sivri metal parçaları (ekskavatör tırnağı, çelik cıvata, astar parçası vb.) döküş şutlarında sıkışarak hareket halindeki kauçuk bandı boydan boya jilet gibi yırtabilmektedir.\n\nGeleneksel elektromanyetik metal ayırıcılar ve mekanik yırtılma halatları ancak hasar yüzlerce metre geliştikten sonra devreye girebilmekte; bu durum 1.5 - 2 km uzunluğundaki ana hat bantlarının tamamen hurdaya çıkmasına, yeni bant tedarik ve vulkanize ek sürecinde tesisin 3 ila 7 gün tamamen durmasına ve milyonlarca liralık üretim/ciro kaybına yol açmaktaydı.",
                NonTechnicalDescription = "Sistem, cevher taşıyan dev konveyör hatlarının kritik transfer ve döküş noktalarına yerleştirilen endüstriyel yüksek hızlı ve toza dayanıklı kameralar ile çalışır:\n\n• Akıllı Görsel Analiz: Kameralar, bant yüzeyini ve alt katmanını saniyede 60 kare hızında tarar. Geliştirilen görüntü işleme yapay zekası, bant üzerindeki en ufak çatlağı, delinmeyi veya sıkışan yabancı cismi anında ayırt eder.\n• Otomatik Acil Durdurma: Bir anomali algılandığında sistem insan müdahalesine gerek kalmadan doğrudan tesis otomasyonuna (PLC) sinyal gönderir ve bandı 400 milisaniye içinde güvenli şekilde durdurur.\n• Anlık Mobil & Sesli Alarm: Eş zamanlı olarak kontrol odasındaki operatör ekranında ve bakım ekibinin tabletinde yırtığın tam konumu, fotoğrafı ve hasar boyutu alarm olarak gösterilir.",
                TechnicalDescription = "NVIDIA Jetson AGX Orin kenar bilişim cihazları üzerinde TensorRT ile optimize edilmiş YOLOv8 tabanlı derin öğrenme modeli, 60 FPS RTSP kamera akışlarını eşzamanlı işler. Siemens S7-1500 PLC ile Profinet ve kuru kontak röle üzerinden haberleşilerek acil stop zinciri tetiklenir. Tüm telemetri, anomali kareleri ve aşınma metrikleri MQTT protokolüyle merkezi sunucuya aktarılır.",
                BusinessImpact = "• Yılda ortalama yaşanabilecek 2 büyük bant yırtılma kazasını önleyerek doğrudan ~450.000 USD yeni bant ve montaj maliyeti tasarrufu sağlar.\n• Plansız tesis duruş süresini yıllık ortalama 120 saatten 4 saatin altına indirerek üretim sürekliliğini %99.2 seviyesinde tutar.\n• Bant ek yeri aşınmalarını önceden haber vererek planlı duruşlarda bakım yapılmasını sağlar, ekipman ömrünü %35 oranında uzatır.",
                TargetAudience = "Kırma Eleme Tesis Operatörleri, Mekanik Bakım Şefleri ve Tesis Mühendisleri",
                StatusCode = "ACTIVE",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 11, 10),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "ai-computer-vision",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "murat.koc@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "MQTT / IoT Edge", "Docker / Kubernetes"],
                TagSlugs = ["yapay-zeka", "kestirimci-bakim", "goruntu-isleme", "otomasyon"],
                Integrations = [("Siemens S7-1500 PLC", IntegrationType.ExternalService, "Acil durdurma tetikleme ve bant hızı okuma")],
                GalleryImages = [("industrial-equipment", "Konveyör Kamera İstasyonu", "Bant üzeri yüksek hızlı kamera montajı"), ("industrial-dashboard", "Anomali Algılama Paneli", "Gerçek zamanlı bant yüzey analizi")],
                Documents = [("Konveyör Bant AI Mimari Dokümanı", "teknik-mimari-ozeti.pdf", "Teknik Doküman", "Görüntü işleme ve PLC haberleşme şeması")]
            },

            // ── 2. Açık Ocak Şev Duraylılık Takip Sistemi ─────────────────────────
            new()
            {
                Name = "Açık Ocak Şev Duraylılık Takip Sistemi",
                Slug = "acik-ocak-sev-duraylilik-takibi",
                ShortDescription = "Maden basamaklarındaki milimetrik zemin hareketlerini ve heyelan riskini gerçek zamanlı takip eden jeoteknik radar platformu.",
                Description = "Açık ocak şev basamaklarına yerleştirilen prizmalar ve interferometrik yer radarı verilerini birleştiren emniyet izleme yazılımı. Ocak içi basamak deformasyonlarını harita üzerinde renklendirerek erken uyarı ve tahliye süreçlerini yönetir.",
                Purpose = "Açık ocak maden işletmelerinde şev basamakları ve nihai ocak yamaçlarında meydana gelebilecek zemin oturmalarını, mikro fay kaymalarını ve potansiyel heyelan risklerini gerçek zamanlı olarak izlemek; basamak stabilitesini sürekli ölçerek maden çalışanlarının can güvenliğini ve yüksek tonajlı iş makinelerinin operasyonel emniyetini en üst seviyede teminat altına almaktır.",
                ProblemSolved = "Geleneksel jeoteknik izleme yöntemlerinde reflektör prizmalar haritacılar tarafından optik total station cihazlarıyla periyodik olarak (haftada veya ayda bir) manuel okunmaktaydı. Ani yağışlar, tektonik hareketler veya patlatma sonrası oluşan mikroskobik kaya hareketleri periyot aralarında kaldığı için öngörülememekte ve ani şev göçmeleri riski doğurmaktaydı. Bu durum milyonlarca liralık ekskavatör ve kamyonun göçük altında kalma riskine, basamağın günlerce işletmeye kapatılmasına ve en önemlisi saha personeli için hayati tehlikeye neden olmaktaydı.",
                NonTechnicalDescription = "Sistem, maden çukurunun karşısına konumlandırılan mikrodalga interferometrik yer radarı ve GNSS sensörlü robotik prizmaların verilerini tek bir harita üzerinde birleştirir:\n\n• Milimetrik Hassasiyet: Radar, tüm ocak yamaçlarını her 3 dakikada bir tarayarak basamaklardaki 0.1 mm mertebesindeki en ufak gevşeme ve kabarmaları tespit eder.\n• Dinamik Risk Isı Haritası: Bilgisayar ekranında stabil bölgeler yeşil, hareket tespit edilen kritik şevler ise sarı ve kırmızı renk tonlarıyla anlık gösterilir.\n• Otomatik Siren ve Telsiz Uyarısı: Hareket hızı belirlenen güvenlik eşiğini aştığı anda sistem sahadaki tüm operatörlerin el telsizlerine ve ocak içi siren kulelerine otomatik tahliye anonsu geçer.",
                TechnicalDescription = "IBIS interferometrik yer radarı veri akışları, GNSS prizma telemetrisi, Python tabanlı jeo-uzamsal interpolasyon algoritmaları ve Leaflet/Mapbox GIS görselleştirme motoru. Mikro hareket vektörleri PostgreSQL/PostGIS uzamsal veritabanında saklanır.",
                BusinessImpact = "• Sıfır iş kazası hedefi doğrultusunda ocak içi riskli alan tahliye süresini 45 dakikadan 5 dakikaya indirmiştir.\n• Olası şev kaymalarında ekskavatör ve kamyonların önceden güvenli bölgeye çekilmesini sağlayarak milyonlarca dolarlık ekipman kaybını engeller.",
                TargetAudience = "Jeoteknik Mühendisleri, Maden Emniyet Şefleri ve Ocak Operatörleri",
                StatusCode = "ACTIVE",
                CategoryCode = "MINING_TECHNOLOGY",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2022, 8, 15),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "mapping-gis",
                PrimaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["selin.yildiz@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "PostgreSQL", "React", "Docker / Kubernetes"],
                TagSlugs = ["saha-yonetimi", "is-guvenligi", "cbs-haritalama", "jeolojik-modelleme"],
                Integrations = [("Jeoteknik Radar Servisi", IntegrationType.RestApi, "Mikro hareket vektörlerinin anlık aktarımı")],
                GalleryImages = [("open-pit-mine", "Ocak Şev İzleme Görünümü", "Basamak deformasyon ısı haritası")],
                Documents = [("Şev İzleme Prosedürü", "kullanim-kilavuzu.pdf", "Kılavuz", "Acil durum seviyeleri ve tahliye protokolleri")]
            },

            // ── 3. Otonom Saha Kantar ve RFID Entegrasyonu ────────────────────────
            new()
            {
                Name = "Otonom Saha Kantar ve RFID Entegrasyonu",
                Slug = "otonom-saha-kantar-entegrasyonu",
                ShortDescription = "Cevher ve pasa kamyonlarının şoför inmeden plaka ve RFID ile 10 saniyede otomatik tartılmasını sağlayan istasyon çözümü.",
                Description = "Aks kantarlarına entegre RFID antenleri, bariyerler, trafik ışıkları ve plaka tanıma kameraları ile insansız tartım otomasyonu. Maden sahasından zenginleştirme tesisine ve limana giden nakliye döngüsünü tamamen dijitalleştirir.",
                Purpose = "Maden sahası ile zenginleştirme tesisi veya liman sevkiyat noktaları arasındaki tüm cevher, pasa ve konsantre nakliyesini yöneten aks kantarlarını tam otonom hale getirmek; kamyon tartım döngüsünü insan bağımlılığından kurtararak kantar kuyruklarını sıfırlamak, kayıt doğruluğunu %100'e ulaştırmak ve lojistik operasyon hızını maksimize etmektir.",
                ProblemSolved = "Manuel tartım sürecinde kamyon şoförünün araçtan inmesi, kantar kulübesine gidip irsaliye uzatması, operatörün plakayı ve brüt tonajı el ile sisteme girmesi araç başına 90 ila 120 saniye sürmekteydi. Vardiya değişimlerinde ve yoğun sevkiyat saatlerinde kantar önünde 20-30 kamyonluk konvoylar oluşmakta, araçların rölantide beklemesi ciddi yakıt israfına yol açmakta ve elle giriş sırasında sehven yapılan rakam hataları aylık stok mutabakatlarında ciddi tonaj farkları yaratmaktaydı.",
                NonTechnicalDescription = "Kantar geçişleri tamamen temassız ve insansız bir gişe sistemine dönüştürülmüştür:\n\n• Otomatik Tanıma: Kamyon kantara yaklaştığı anda yüksek hızlı ANPR kameralar plakayı okur, camdaki RFID etiketi ise araç sicil numarası ve boş darasını kablosuz algılar.\n• 10 Saniyede Tartım: Kamyon kantar üzerinde yavaşladığında aks ağırlıkları otomatik ölçülür, net cevher yükü hesaplanır ve sürücünün inmesine gerek kalmadan yeşil ışık yakılarak bariyer açılır.\n• Dijital İrsaliye: Tartım fişi ve dijital sevk irsaliyesi saniyeler içinde SAP ERP sistemine aktarılır; şoförün cep telefonuna SMS/QR kod olarak iletilir.",
                TechnicalDescription = "C# .NET 9 Worker servisleri, Modbus TCP kantar indikatör haberleşmesi, Hikvision ANPR kamera SDK, SQL Server veri ambarı aktarımı ve SAP RFC entegrasyonu.",
                BusinessImpact = "• Tartım başına işlem süresini 90 saniyeden 12 saniyeye düşürerek kantar kapasitesini 7 kat artırmıştır.\n• Rölanti bekleme sürelerinin azalması sayesinde filo yakıt tüketiminde yıllık ~65.000 litre tasarruf sağlanmıştır.\n• Elle veri giriş hataları sıfırlanarak maden ile liman arasındaki stok mutabakat farkları %0.1'in altına çekilmiştir.",
                TargetAudience = "Kantar Operatörleri, Lojistik Şefleri ve Nakliye Müteahhitleri",
                StatusCode = "COMPLETED",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 2, 1),
                EndDate = new DateOnly(2023, 10, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "warehouse-logistics",
                PrimaryTeamName = "Lojistik ve Filo Optimizasyonu Ekibi",
                SecondaryTeamName = "Yazılım Geliştirme Ekibi",
                MemberEmails = ["onur.simsek@fictional-demirexport.com", "deniz.arslan@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Balıkesir Manyas Sahası", "İzmir Liman ve Lojistik Merkezi"],
                TechnologyNames = [".NET 9", "SQL Server", "OPC UA", "Vue.js"],
                TagSlugs = ["otomasyon", "lojistik-filo", "saha-yonetimi"],
                Integrations = [("SAP Lojistik Modülü", IntegrationType.RestApi, "Kantar tartım verilerinin irsaliye numarasıyla eşleşmesi")]
            },

            // ── 4. Cevher Tenör Harmanlama Optimizasyon Motoru ─────────────────────
            new()
            {
                Name = "Cevher Tenör Harmanlama Optimizasyon Motoru",
                Slug = "cevher-tenor-optimizasyon-motoru",
                ShortDescription = "Farklı ocak aynalarından çıkarılan cevherlerin fabrika besleme tenörünü sabit tutacak matematiksel harmanlama algoritması.",
                Description = "Zenginleştirme tesisine giren demir ve kükürt oranlarını ideal aralıkta tutmak için günlük kamyon döküm planını optimize eder. Tesis besleme tenörünü dengede tutarak metalurjik geri kazanımı maksimize eder.",
                Purpose = "Açık ocaktaki farklı üretim aynalarından ve stok alanlarından çıkarılan demir cevherlerinin tenör, silis, alümina ve nem parametrelerini yapay zeka ve doğrusal programlama modelleriyle analiz ederek, kırma-eleme ve konsantratör tesisine sevk edilecek günlük harmanlama reçetesini otomatik optimize etmektir. Böylece tesis besleme tenörünün dalgalanmasını engelleyerek kimyasal zenginleştirme prosesinin maksimum verimlilikte çalışmasını sağlamaktır.",
                ProblemSolved = "Maden yatağının jeolojik yapısı gereği farklı ocak basamaklarındaki tenör oranları %45 Fe ile %62 Fe arasında ciddi değişkenlik göstermektedir. Sahada günlük kamyon döküm planlaması tecrübeye dayalı yapıldığında tesise bir gün çok yüksek silisli, ertesi gün çok yüksek tenörlü cevher gitmekteydi. Bu dengesizlik flotasyon reaktif tüketimini %25 artırmakta, peletleme kalitesini bozmakta ve metalurjik geri kazanım oranlarında %3-5 arasında verim kayıplarına yol açmaktaydı.",
                NonTechnicalDescription = "Sistem, ocak jeoloji haritasındaki cevher blokları ile fabrikanın anlık talebini birbirine bağlayan akıllı bir harmanlama kılavuzudur:\n\n• Günlük Reçete Optimizasyonu: Sistem her sabah laboratuvardan gelen ayna analizlerini inceler ve fabrikanın istediği %57.5 Fe sabit tenörünü tutturmak için 'A panosundan 40 kamyon, B panosundan 25 kamyon, stok 3'ten 15 kamyon dökülmeli' şeklinde en düşük maliyetli rotayı hesaplar.\n• Anlık Adaptasyon: Gün içinde bir ekskavatör arızalandığında veya tenör saptığında sistem saniyeler içinde yeni döküm planını güncelleyerek filo yönlendirme ekranlarına gönderir.",
                TechnicalDescription = "Python SciPy ve PuLP doğrusal programlama optimizasyon çözücüsü, SSAS veri küpü bağlantısı ve React tabanlı harmanlama simülatörü. Laboratuvar LIMS sisteminden gelen analizlerle kapalı döngü çalışır.",
                BusinessImpact = "• Tesis flotasyon metalurjik verimini %2.3 artırarak aylık ortalama 18.000 ton ek konsantre üretimi sağlamıştır.\n• Reaktif kimyasal tüketimini %14 azaltarak yıllık 320.000 USD işletme maliyeti tasarrufu gerçekleştirmiştir.",
                TargetAudience = "Maden Planlama Mühendisleri, Metalurji ve Cevher Hazırlama Şefleri",
                StatusCode = "ACTIVE",
                CategoryCode = "DATA_ANALYTICS",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 1, 20),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "data-analytics",
                PrimaryTeamName = "Veri Analitiği Ekibi",
                SecondaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                MemberEmails = ["ayse.kaya@fictional-demirexport.com", "serkan.ozturk@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi"],
                TechnologyNames = ["Python / PyTorch", "React", "PostgreSQL", "Apache Spark"],
                TagSlugs = ["veri-analitigi", "cevher-kalite", "uretim-takibi"],
                Integrations = [("LIMS Laboratuvar Sistemi", IntegrationType.Database, "Günlük ayna kimyasal analiz sonuçlarının çekilmesi")],
                GalleryImages = [("laboratory-analysis", "Tenör Dağılımı ve Harman Eğrisi", "Simülasyon sonuç paneli")]
            },

            // ── 5. Ağır Ekipman Yağ Spektrometri Analiz Sistemi ───────────────────
            new()
            {
                Name = "Ağır Ekipman Yağ Spektrometri Analiz Sistemi",
                Slug = "agir-ekipman-yag-analiz-sistemi",
                ShortDescription = "Kamyon ve ekskavatör motor/şanzıman yağlarındaki aşınma metallerini takip eden kestirimci bakım modülü.",
                Description = "Periyodik yağ numunelerindeki Fe, Cu, Cr ve Si ppm değerlerini takip ederek dahili parça aşınmasını arıza öncesi saptar.",
                Purpose = "Dizel motor yatak sarması ve diferansiyel dişli kırılmalarını en az 100 çalışma saati öncesinden tespit etmek.",
                ProblemSolved = "Ani motor kilitlenmeleri sonucu 80.000$ üzerindeki revizyon masrafları ve ekipmanın 3 hafta atıl kalması.",
                NonTechnicalDescription = "İş makinelerinden kan alır gibi yağ örneği alıp içindeki mikroskobik demir parçacıklarını analiz ederek motorun sağlığını ölçen sistem.",
                TechnicalDescription = "ASP.NET Core Web API, Scikit-Learn regresyon modelleri, InfluxDB zaman serisi ölçüm arşivi ve PowerBI gömülü raporlama.",
                BusinessImpact = "Geçtiğimiz çeyrekte 3 büyük kamyon motorunun yatak sarmadan önce kurtarılmasını sağlayarak 210.000$ tasarruf etmiştir.",
                TargetAudience = "Ağır Ekipman Bakım Şefleri, Yağlama Teknisyenleri ve Filo Yöneticileri",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2025, 1, 10),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "predictive-maintenance",
                PrimaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                SecondaryTeamName = "Veri Analitiği Ekibi",
                MemberEmails = ["murat.koc@fictional-demirexport.com", "cemal.cetin@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = [".NET 9", "Scikit-Learn", "SQL Server", "Power BI"],
                TagSlugs = ["kestirimci-bakim", "saha-yonetimi", "veri-analitigi"]
            },

            // ── 6. Yeraltı Personel ve Araç Konum Takibi ─────────────────────────
            new()
            {
                Name = "Yeraltı Personel ve Araç Konum Takibi",
                Slug = "yeralti-personel-konum-takibi",
                ShortDescription = "Yeraltı galerilerinde GPS çekmeyen alanlarda UWB ve BLE baz istasyonları ile personelin santimetre hassasiyetinde canlı takibi.",
                Description = "Baret lambalarına entegre aktif RFID/UWB etiketleri ile acil durum tahliye sayacı ve gaz sızıntısı anında personel koordinatı sağlar.",
                Purpose = "Yeraltı madenciliğinde olası göçük, yangın veya gaz birikmesinde mahsur kalan personelin yerini saniyeler içinde tespit etmek.",
                ProblemSolved = "Yeraltı galerilerinde kimin hangi galeride veya arında olduğunun manuel panolarla takip edilmesindeki riskler.",
                NonTechnicalDescription = "Yeraltında çalışan madencilerin baretlerindeki küçük çipler sayesinde yeryüzündeki kontrol merkezinde harita üstünde nerede olduklarını gösteren canlı takip sistemi.",
                TechnicalDescription = "Ultra-Wideband (UWB) TDoA yer belirleme algoritması, LoRaWAN omurga haberleşmesi, C# WebSocket sunucusu ve 3D Three.js galeri modeli.",
                BusinessImpact = "Acil durum yoklama süresini 35 dakikadan 20 saniyeye düşürerek yeraltı emniyet standartlarında en üst düzeye ulaşılmıştır.",
                TargetAudience = "Yeraltı İşletme Şefleri, Tahlisiye (Kurtarma) Ekipleri ve İSG Uzmanları",
                StatusCode = "PILOT",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.External,
                StartDate = new DateOnly(2025, 4, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "iot-sensors",
                PrimaryTeamName = "Saha Teknolojileri ve İletişim Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["emre.polat@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Balıkesir Manyas Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Node.js", "LoRaWAN", "Redis", "React"],
                TagSlugs = ["iot-sensor", "is-guvenligi", "saha-yonetimi", "telemetri"],
                Integrations = [("Yeraltı Gaz Alarm Sistemi", IntegrationType.ExternalService, "Yüksek metan/CO alarmında en yakın personellere titreşim uyarısı")],
                Documents = [("Yeraltı Konum Takip Pilot Kılavuzu", "kullanim-kilavuzu.pdf", "Kılavuz", "Baret etiketi şarj ve montaj talimatları")]
            },

            // ── 7. Patlatma Titreşim ve Hava Şoku Simülasyonu ────────────────────
            new()
            {
                Name = "Patlatma Titreşim ve Hava Şoku Simülasyonu",
                Slug = "patlatma-titresim-simulasyonu",
                ShortDescription = "Açık ocak basamak patlatmalarının çevre köylere ve yapılara etkisini sismograf verileriyle modelleyen simülatör.",
                Description = "Gecikmeli kapsül ateşleme sıraları ve delik şarj miktarlarına göre sismik dalga yayılımını ve hava şoku basıncını hesaplar.",
                Purpose = "Yasal sınırların altında kalacak optimum patlatma deseni tasarlayarak çevre şikayetlerini ve bina hasarı riskini sıfırlamak.",
                ProblemSolved = "Aşırı titreşim veya gürültü nedeniyle çevre yerleşimlerden gelen şikayetler ve durdurulan patlatma izinleri.",
                NonTechnicalDescription = "Dinamit patlatılmadan önce bilgisayarda simüle edilerek çevre köylerdeki evlerin sallanıp sallanmayacağını önceden gösteren program.",
                TechnicalDescription = "Python NumPy sismik dalga denklemleri çözümü, sismograf cihazlarından otomatik FTP veri çekme, GIS harita katmanları.",
                BusinessImpact = "Yıllık patlatma izin onay süreçlerini hızlandırmış ve çevre yerleşimlerle sürdürülebilir komşuluk ilişkisi sağlamıştır.",
                TargetAudience = "Delme-Patlatma Mühendisleri, Çevre Mühendisleri ve Hukuk Müşavirliği",
                StatusCode = "COMPLETED",
                CategoryCode = "MINING_TECHNOLOGY",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 5, 10),
                EndDate = new DateOnly(2024, 2, 28),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "mapping-gis",
                PrimaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                SecondaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                MemberEmails = ["selin.yildiz@fictional-demirexport.com", "ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "PostgreSQL", ".NET 9"],
                TagSlugs = ["cevre-surdurulebilirlik", "cbs-haritalama", "saha-yonetimi"]
            },

            // ── 8. Toz Bastırma ve Hava Kalitesi Otomatik İzleme ──────────────────
            new()
            {
                Name = "Toz Bastırma ve Hava Kalitesi Otomatik İzleme",
                Slug = "toz-bastirma-ve-hava-kalitesi-izleme",
                ShortDescription = "Ocak ve kırma tesislerindeki PM10 toz sensörlerine göre otomatik sis topları ve fıskiyeleri tetikleyen çevre otomasyonu.",
                Description = "Optik partikül sayaçları, rüzgar anemometreleri ve su püskürtme sistemlerini kapalı döngüde birleştiren IoT platformu.",
                Purpose = "Toz emisyonunu yasal sınırların altında tutarken su tüketimini ihtiyaca göre optimize etmek.",
                ProblemSolved = "Manuel kontrol edilen su arazözlerinin ve fıskiyelerin ya gecikmesi ya da aşırı su israfına yol açması.",
                NonTechnicalDescription = "Havadaki tozu koklayan sensörler toz arttığında otomatik olarak su sisi sıkan dev fanları çalıştırır.",
                TechnicalDescription = "Modbus TCP sensörler, MQTT broker, ASP.NET Core kontrol servisi ve Vue.js operatör paneli.",
                BusinessImpact = "Toz bastırma su tüketiminde %35 tasarruf sağlarken çevre mevzuatı emisyon uyumunu %100 seviyesine çıkarmıştır.",
                TargetAudience = "Çevre Mühendisleri, Tesis Yöneticileri ve İSG Uzmanları",
                StatusCode = "ACTIVE",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 4, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "environmental-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                SecondaryTeamName = "IoT ve Otomasyon Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com", "mehmet.demir@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi"],
                TechnologyNames = ["MQTT / IoT Edge", "ASP.NET Core", "Vue.js", "InfluxDB"],
                TagSlugs = ["cevre-surdurulebilirlik", "iot-sensor", "otomasyon"]
            },

            // ── 9. Mobil Vardiya Raporlama ve Görev Yönetimi ─────────────────────
            new()
            {
                Name = "Mobil Vardiya Raporlama ve Görev Yönetimi",
                Slug = "mobil-vardiya-raporlama-uygulamasi",
                ShortDescription = "Saha formenlerinin internet çekmeyen açık ocakta dahi tablet üzerinden üretim ve makine arıza kaydı girebildiği mobil platform.",
                Description = "Offline-first SQLite önbelleği ve internete kavuştuğunda otomatik senkronizasyon yeteneğine sahip cross-platform uygulama.",
                Purpose = "Vardiya sonu kağıt form doldurma yükünü ortadan kaldırmak ve verilerin anında genel merkeze akmasını sağlamak.",
                ProblemSolved = "Kağıt formların kaybolması, okunaksız yazılar ve ertesi güne sarkan üretim raporlama gecikmeleri.",
                NonTechnicalDescription = "Çavuşların ve formenlerin ellerindeki dayanıklı tabletlerle ocakta gezerken kamyon arızasını veya kaç ton taş taşındığını girdiği kolay uygulama.",
                TechnicalDescription = "Flutter / Dart cross-platform istemci, SQLite local cache, ASP.NET Core Web API ve JWT token yetkilendirme.",
                BusinessImpact = "Vardiya teslim sürelerini 40 dakika kısaltmış ve rapor hatalarını %95 oranında azaltmıştır.",
                TargetAudience = "Vardiya Çavuşları, Formenler ve Maden Mühendisleri",
                StatusCode = "ACTIVE",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 8, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "mobile-field-worker",
                PrimaryTeamName = "Yazılım Geliştirme Ekibi",
                SecondaryTeamName = "Saha Teknolojileri ve İletişim Ekibi",
                MemberEmails = ["derya.aksoy@fictional-demirexport.com", "ahmet.yilmaz@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası", "Balıkesir Manyas Sahası"],
                TechnologyNames = ["Flutter", "ASP.NET Core", "SQL Server"],
                TagSlugs = ["mobil-cozumler", "vardiya-yonetimi", "saha-yonetimi"],
                Documents = [("Mobil Vardiya Tablet Kılavuzu", "kullanim-kilavuzu.pdf", "Kılavuz", "Tablet şarj, senkronizasyon ve veri giriş adımları")]
            },

            // ── 10. Maden Dijital İkiz ve 3D Simülasyon Platformu ─────────────────
            new()
            {
                Name = "Maden Dijital İkiz ve 3D Simülasyon Platformu",
                Slug = "maden-dijital-ikiz-platformu",
                ShortDescription = "Açık ocak sahası, kırma tesisi ve lojistik hatlarının gerçek zamanlı sensör verileriyle beslenen 3D sanal ikizi.",
                Description = "Drone fotogrametrisi, SCADA telemetrisi ve kamyon GNSS verilerini WebGL/WebGPU tabanlı 3D haritada canlı birleştirir.",
                Purpose = "Üst yönetimin ve mühendislerin madendeki tüm operasyonu sanki sahadaymış gibi üç boyutlu canlı izlemesini sağlamak.",
                ProblemSolved = "Farklı 10 ayrı sistemin (kantar, filo, kırma, hava durumu) ayrı ayrı ekranlarda takip edilmesinin yarattığı koordinasyon eksikliği.",
                NonTechnicalDescription = "Tüm maden sahasının bilgisayar oyunu grafiğinde canlı kopyası; kamyonların gidişi, kırıcıların çalışması hepsi 3 boyutlu ekranda akmaktadır.",
                TechnicalDescription = "React, Three.js / WebGPU grafik motoru, SignalR gerçek zamanlı veri akışı, Redis Pub/Sub ve mikroservis mimarisi.",
                BusinessImpact = "Genel müdürlük ve saha arasındaki operasyonel karar alma süresini %60 oranında hızlandırmıştır.",
                TargetAudience = "Üst Yönetim, Genel Müdürlük Operasyon Direktörleri ve Başmühendisler",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 6, 1),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "control-room",
                PrimaryTeamName = "Yazılım Geliştirme Ekibi",
                SecondaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                MemberEmails = ["ahmet.yilmaz@fictional-demirexport.com", "burak.aydin@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Genel Müdürlük (Ankara)"],
                TechnologyNames = ["React", "Typescript", ".NET 9", "Redis", "Docker / Kubernetes"],
                TagSlugs = ["dijital-ikiz", "yapay-zeka", "cbs-haritalama", "uretim-takibi"],
                GalleryImages = [("control-room", "3D Dijital İkiz Kokpiti", "Tüm tesisin interaktif 3D modeli"), ("industrial-dashboard", "Sensör Katmanları", "Ocak içi anlık telemetri göstergeleri")],
                Documents = [("Dijital İkiz Mimari Dokümanı", "teknik-mimari-ozeti.pdf", "Teknik Doküman", "WebGPU render pipeline ve veri bus entegrasyonu")]
            },

            // ── 11. Su Yönetimi ve Asit Maden Drenajı Nötralizasyonu ──────────────
            new()
            {
                Name = "Su Yönetimi ve Asit Maden Drenajı Nötralizasyonu",
                Slug = "su-yonetimi-ve-asit-maden-drenaji",
                ShortDescription = "Maden göletleri ve deşarj kanallarındaki pH ve iletkenlik değerlerine göre otomatik kireç dozajlama yapan çevre sistemi.",
                Description = "Açık ocak drenaj sularının doğaya deşarj edilmeden önce online analizörlerle ölçülerek çevre standartlarına getirilmesini sağlar.",
                Purpose = "Asit maden drenajını önleyerek çevre mevzuatı deşarj limitlerine 7/24 tam uyum sağlamak.",
                ProblemSolved = "Manuel su numunesi alımının yetersiz kalması ve yağışlı günlerde gölet taşma riskleri.",
                NonTechnicalDescription = "Maden sularının asit oranını ölçüp doğaya temiz su bırakılmasını sağlayan ve kireç pompasını otomatik çalıştıran sistem.",
                TechnicalDescription = "Endress+Hauser pH/iletkenlik analizörleri, Modbus RTU PLC bağlantısı, Node.js gateway ve TimescaleDB.",
                BusinessImpact = "Çevre izin denetimlerinde %100 kusursuzluk sağlanmış ve su geri kazanım oranı %40 artırılmıştır.",
                TargetAudience = "Çevre Mühendisleri, Atık Barajı Şefleri ve Kimya Teknisyenleri",
                StatusCode = "ACTIVE",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 3, 20),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "environmental-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                SecondaryTeamName = "IoT ve Otomasyon Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com", "can.ozkan@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Node.js", "PostgreSQL", "MQTT / IoT Edge"],
                TagSlugs = ["cevre-surdurulebilirlik", "otomasyon", "iot-sensor"]
            },

            // ── 12. Değirmen Astar Aşınma ve Bilye Yükü Tahmini ──────────────────
            new()
            {
                Name = "Değirmen Astar Aşınma ve Bilye Yükü Tahmini",
                Slug = "degirmen-astar-asinma-tahmini",
                ShortDescription = "Bilyeli ve yarı otojen (SAG) değirmen gövdesine takılan akustik sensörlerle astar ömrünü ve bilye darbe şiddetini modelleyen yapay zeka.",
                Description = "Değirmen dönüşü sırasında cevher ve çelik bilyelerin astar plakalarına çarpma ses frekanslarını Fourier dönüşümüyle analiz eder.",
                Purpose = "Değirmen astar delinmelerini önlemek ve bilye ekleme zamanlamasını optimize ederek öğütme verimini artırmak.",
                ProblemSolved = "Astarın plansız delinmesiyle gövde hasarı oluşması ve değirmenin günlerce durdurulmak zorunda kalınması.",
                NonTechnicalDescription = "Değirmenin çıkardığı sesi dinleyerek içindeki çelik zırhın ne kadar inceldiğini ve ne zaman değişmesi gerektiğini söyleyen ses analizörü.",
                TechnicalDescription = "Yüksek frekanslı akustik piezo sensörler, Python librosa ve PyTorch ses spektrogram sınıflandırma ağı, Edge Linux sunucu.",
                BusinessImpact = "Astar değişim planlamasını 2 ay önceden netleştirerek 120.000$ astar maliyeti tasarrufu sağlamıştır.",
                TargetAudience = "Öğütme Tesis Şefleri, Bakım Mühendisleri ve Metalurji Uzmanları",
                StatusCode = "PROOF_OF_CONCEPT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2025, 3, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "industrial-equipment",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "murat.koc@fictional-demirexport.com"],
                LocationNames = ["Sivas Kangal Zenginleştirme Tesisi", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "MQTT / IoT Edge", "Docker / Kubernetes"],
                TagSlugs = ["yapay-zeka", "kestirimci-bakim", "uretim-takibi"]
            },

            // ── 13. Filo Yakıt Tüketimi ve Eko-Sürüş Optimizasyonu ────────────────
            new()
            {
                Name = "Filo Yakıt Tüketimi ve Eko-Sürüş Optimizasyonu",
                Slug = "filo-yakit-tuketim-optimizasyonu",
                ShortDescription = "Kaya kamyonlarının CAN-bus telemetrisinden rölanti, aşırı hız ve vites kullanımını analiz ederek yakıt tasarrufu sağlayan analitik sistem.",
                Description = "Maden içi rampalarda kamyonların motor yükü ve anlık yakıt tüketimini harita eğimleriyle eşleştirerek şoför bazlı eko-skor üretir.",
                Purpose = "Şirketin en büyük maliyet kalemi olan dizel yakıt tüketimini filo genelinde %8-12 oranında azaltmak.",
                ProblemSolved = "Gereksiz rölanti çalışma süreleri, rampa çıkışlarında yanlış vites kullanımı ve şoförler arası devasa yakıt farkları.",
                NonTechnicalDescription = "Kamyon şoförlerinin arabayı ne kadar ekonomik sürdüğünü gösteren ve şoförlere 'Rölantide bekleme, motoru kapat' diyerek yakıt kurtaran sistem.",
                TechnicalDescription = "J1939 CAN-bus gateway, InfluxDB telemetri veritabanı, Apache Spark büyük veri analizi ve ASP.NET Core raporlama API.",
                BusinessImpact = "Filo genelinde yıllık 420.000 litre dizel yakıt tasarrufu ve 1.100 ton karbon emisyonu düşüşü sağlamıştır.",
                TargetAudience = "Maden Filo Şefleri, Kamyon Eğitmenleri ve Lojistik Yöneticileri",
                StatusCode = "COMPLETED",
                CategoryCode = "DATA_ANALYTICS",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 4, 1),
                EndDate = new DateOnly(2024, 6, 30),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "haul-truck",
                PrimaryTeamName = "Lojistik ve Filo Optimizasyonu Ekibi",
                SecondaryTeamName = "Veri Analitiği Ekibi",
                MemberEmails = ["onur.simsek@fictional-demirexport.com", "ayse.kaya@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Kangallı Altın Sahası"],
                TechnologyNames = ["Apache Spark", "InfluxDB", ".NET 9", "Power BI"],
                TagSlugs = ["lojistik-filo", "enerji-verimliligi", "veri-analitigi", "cevre-surdurulebilirlik"]
            },

            // ── 14. İSG Baret ve Kişisel Koruyucu Donanım Denetimi ─────────────────
            new()
            {
                Name = "İSG Baret ve Kişisel Koruyucu Donanım Denetimi",
                Slug = "isg-kisisel-koruyucu-donanim-denetimi",
                ShortDescription = "Tesis ve ocak giriş kapılarında baret, yelek ve koruyucu gözlük takmayan personeli anında tespit eden yapay zeka kamerası.",
                Description = "Maden sahası güvenlik turnikelerine ve tesis girişlerine yerleştirilen IP kameralar üzerinden gerçek zamanlı KKD tespiti yapar.",
                Purpose = "Kişisel koruyucu donanım ihlallerini sıfıra indirerek kafa ve göz travması kaynaklı iş kazalarını engellemek.",
                ProblemSolved = "Güvenlik görevlilerinin yoğun giriş saatlerinde tüm personelin baret ve gözlük durumunu manuel denetleyememesi.",
                NonTechnicalDescription = "Fabrikaya girerken baretini takmayı unutan personeli kapıdaki kameradan fark edip turnikeyi açmayan ve 'Lütfen baretinizi takınız' diyen sistem.",
                TechnicalDescription = "Python OpenCV ve TensorRT ile optimize edilmiş YOLOv8 nesne tespit modeli, ONVIF kamera entegrasyonu ve turnike röle kontrolü.",
                BusinessImpact = "KKD ihlal oranını %14'ten %0.4 seviyesine indirmiş ve iş güvenliği denetimlerinde tam uyum sağlamıştır.",
                TargetAudience = "İSG Uzmanları, Saha Emniyet Denetçileri ve Güvenlik Birimi",
                StatusCode = "ACTIVE",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 2, 10),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "safety-monitoring",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası", "Malatya Hekimhan Peletleme Tesisi"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "TensorFlow", "FastAPI"],
                TagSlugs = ["is-guvenligi", "yapay-zeka", "goruntu-isleme"],
                Integrations = [("Turnike Geçiş Kontrol Sistemi", IntegrationType.RestApi, "KKD onayı verilmeden kapı açılmasını engelleme")],
                GalleryImages = [("safety-monitoring", "Kamera Denetim Ekranı", "Baret ve yelek tespit kutucukları")]
            },

            // ── 15. Laboratuvar Bilgi Yönetim Sistemi (LIMS) ──────────────────────
            new()
            {
                Name = "Laboratuvar Bilgi Yönetim Sistemi (LIMS)",
                Slug = "laboratuvar-bilgi-yonetim-sistemi-lims",
                ShortDescription = "Sondaj karotları ve tesis proses numunelerinin ICP-OES ve XRF kimyasal tahlil sonuçlarını yöneten kurumsal laboratuvar yazılımı.",
                Description = "Numune barkodlama, spektrometre cihaz entegrasyonu, kalite kontrol standartları (QA/QC) ve otomatik sertifika üretim modülü.",
                Purpose = "Numune karışıklığını önlemek, tahlil sonuçlarını dakikalar içinde maden mühendislerinin ekranına ulaştırmak.",
                ProblemSolved = "Manuel Excel tablolarına girilen kimyasal tenör sonuçlarında yapılan kopyalama hataları ve geciken laboratuvar raporları.",
                NonTechnicalDescription = "Madenden alınan taş numunelerinin hangi cihazda ne testinden geçtiğini ve içindeki altın/demir oranını kaydeden dijital laboratuvar defteri.",
                TechnicalDescription = "ASP.NET Core Web API, Next.js web portalı, SQL Server veritabanı, Thermo Fisher XRF cihazı RS232 seri port entegrasyon servisi.",
                BusinessImpact = "Günlük numune raporlama döngüsünü 6 saatten 30 dakikaya indirmiş, numune izlenebilirliğini %100 yapmıştır.",
                TargetAudience = "Kimyagerler, Laboratuvar Teknisyenleri ve Jeokimya Mühendisleri",
                StatusCode = "COMPLETED",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2023, 1, 15),
                EndDate = new DateOnly(2023, 11, 20),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "laboratory-analysis",
                PrimaryTeamName = "Kurumsal Uygulamalar ve ERP Ekibi",
                SecondaryTeamName = "Yazılım Geliştirme Ekibi",
                MemberEmails = ["gamze.aslan@fictional-demirexport.com", "hakan.yavuz@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["ASP.NET Core", "Next.js", "SQL Server", "Docker / Kubernetes"],
                TagSlugs = ["cevher-kalite", "erp-entegrasyonu", "uretim-takibi"],
                Integrations = [("Thermo XRF Spektrometre", IntegrationType.MessageQueue, "Cihaz analiz bittiğinde anlık veri aktarımı")]
            },

            // ── 16. Otonom Sondaj Delgi Makinesi Telemetrisi ──────────────────────
            new()
            {
                Name = "Otonom Sondaj Delgi Makinesi Telemetrisi",
                Slug = "otonom-sondaj-delgi-telemetrisi",
                ShortDescription = "Basamak patlatma delgi makinelerinin delme hızı, kafa basıncı ve delik derinliğini anlık takip eden IoT telemetri sistemi.",
                Description = "DTH rotary delici makinelerin CAN-bus ve hidrolik sensörlerinden alınan 'Measure While Drilling' (MWD) kaya sertlik haritası.",
                Purpose = "Delik derinlik ve açı sapmalarını önlemek, sert kaya geçişlerinde delgi ucu kırılmasını engellemek.",
                ProblemSolved = "Hatalı delinen eğri patlatma deliklerinin basamak tabanında 'tırnak' bırakması ve ikincil kırma maliyetleri.",
                NonTechnicalDescription = "Dinamit deliklerini delen dev makinelerin başında operatör olmasa bile deliği ne kadar derine ve düzgün deldiğini yeryüzünden takip eden ekran.",
                TechnicalDescription = "CANopen / J1939 telemetri arayüzü, Rust tabanlı yüksek frekanslı veri toplayıcı, MQTT protokolü ve React kontrol arayüzü.",
                BusinessImpact = "Delgi sapmalarını %85 azaltarak patlatma homojenliğini ve yükleme ekskavatörlerinin verimini %12 artırmıştır.",
                TargetAudience = "Delme-Patlatma Şefleri, Delgi Operatörleri ve Makine Bakımcıları",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 9, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "industrial-automation",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                SecondaryTeamName = "Saha Teknolojileri ve İletişim Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com", "volkan.tekin@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Balıkesir Manyas Sahası"],
                TechnologyNames = ["Rust Engine", "MQTT / IoT Edge", "React", "PostgreSQL"],
                TagSlugs = ["otomasyon", "telemetri", "saha-yonetimi"]
            },

            // ── 17. Kırıcı Besleyici Akıllı Hız Kontrolü ──────────────────────────
            new()
            {
                Name = "Kırıcı Besleyici Akıllı Hız Kontrolü",
                Slug = "kirici-besleyici-otomatik-hiz-kontrolu",
                ShortDescription = "Primer kırıcı haznesindeki kaya seviyesi ve motor akımına göre titreşimli besleyici hızını otomatik ayarlayan kontrol algoritması.",
                Description = "Kırıcı motor yükü amperajını ve besleme bunker ultrasonik seviyesini dengeleyen bulanık mantık (Fuzzy Logic) kontrolü.",
                Purpose = "Kırıcının boğulmasını veya yüksüz boş çalışmasını engelleyerek saatlik kırma tonajını sabit tavanda tutmak.",
                ProblemSolved = "Kamyon döküm anlarında kırıcının aniden sıkışması ve kırıcıyı açmak için saatler süren manuel kaya temizleme operasyonu.",
                NonTechnicalDescription = "Büyük kayalar geldiğinde besleyiciyi yavaşlatan, hazne boşaldığında hızlandıran, kırıcının midesini tam doldurup hiç tıkamayan akıllı sistem.",
                TechnicalDescription = "Siemens S7-1500 PLC üzerinde koşan PID / Fuzzy kontrol blokları, SCADA WinCC Open Architecture ve C# izleme arayüzü.",
                BusinessImpact = "Primer kırıcı saatlik besleme kapasitesini 1.200 tondan 1.410 tona yükselterek %17.5 kapasite artışı sağlamıştır.",
                TargetAudience = "Kırma Eleme Tesis Operatörleri ve Tesis Kontrol Odası Görevlileri",
                StatusCode = "ACTIVE",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 7, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "industrial-equipment",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                SecondaryTeamName = "Tesis ve Bakım Yönetimi Direktörlüğü",
                MemberEmails = ["can.ozkan@fictional-demirexport.com", "cemal.cetin@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi"],
                TechnologyNames = ["OPC UA", ".NET 9", "SQL Server"],
                TagSlugs = ["otomasyon", "scada-plc", "uretim-takibi"]
            },

            // ── 18. Saha Geofence ve Dinamik Güvenlik Bariyeri ───────────────────
            new()
            {
                Name = "Saha Geofence ve Dinamik Güvenlik Bariyeri",
                Slug = "saha-geofence-guvenlik-bariyeri",
                ShortDescription = "Patlatma alanları ve şev kenarlarına yaklaşan araç ve yayaları GNSS ile tespit edip kabin içi sesli alarm üreten güvenlik yazılımı.",
                Description = "Maden haritasında çizilen dinamik tehlikeli bölgelerin kamyon kabin tabletlerine anlık aktarılarak coğrafi sınır ihlalini denetlemesi.",
                Purpose = "Açık ocakta patlatma saatlerinde veya heyelan riski olan şev dibine araç girişlerini kesin olarak engellemek.",
                ProblemSolved = "Gözden kaçan emniyet şeritleri veya tabelaları fark etmeyip tehlikeli alana giren araçların kaza riski.",
                NonTechnicalDescription = "Maden haritasında kırmızıyla çizilen tehlikeli alana bir kamyon yaklaştığında şoförün kabininde 'DUR! Tehlikeli Alana Yaklaşıyorsun' diye bağıran sistem.",
                TechnicalDescription = "PostGIS coğrafi veritabanı, RTK-GNSS santimetre hassasiyetli GPS telemetrisi, MQTT websocket bildirimi ve Android kabin tableti.",
                BusinessImpact = "Saha güvenlik ihlali vakalarını son 1 yılda %98 oranında düşürmüştür.",
                TargetAudience = "İSG Ekipleri, Açık Ocak Şefleri ve Maden Kamyon Şoförleri",
                StatusCode = "ACTIVE",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 10, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "safety-monitoring",
                PrimaryTeamName = "İSG Dijital Takip Ekibi",
                SecondaryTeamName = "Saha Teknolojileri ve İletişim Ekibi",
                MemberEmails = ["elif.demir@fictional-demirexport.com", "emre.polat@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["PostgreSQL", "MQTT / IoT Edge", "Flutter", "Docker / Kubernetes"],
                TagSlugs = ["is-guvenligi", "iot-sensor", "cbs-haritalama", "saha-yonetimi"],
                Documents = [("Geofence Saha Güvenlik Talimatı", "kullanim-kilavuzu.pdf", "Kılavuz", "Sanal bariyer oluşturma ve acil bölge tahliye adımları")]
            },

            // ── 19. Cevher Stok Hacim Ölçümü ve LiDAR Drone Entegrasyonu ─────────
            new()
            {
                Name = "Cevher Stok Hacim Ölçümü ve LiDAR Drone",
                Slug = "cevher-stok-hacim-olcum-drone",
                ShortDescription = "Maden sahasındaki devasa cevher stok yığınlarının LiDAR ve fotogrametri donanımlı drone ile 30 dakikada tonaj hesaplaması.",
                Description = "Aylık envanter sayımlarında milyonlarca tonluk tüvenan ve konsantre stok konilerinin hacim ve kütle hesabını otomatikleştirir.",
                Purpose = "Stok sayım süresini 4 günden yarım güne indirmek ve muhasebe maliyet sapmalarını sıfırlamak.",
                ProblemSolved = "Harita mühendislerinin dik stok konilerine tırmanarak GPS tutmasının getirdiği düşme riskleri ve günlerce süren hesaplamalar.",
                NonTechnicalDescription = "Stok yığınının üstünde uçan drone'un 3 boyutlu lazer tarama yaparak 'Burada tam 245.000 ton demir cevheri var' diye rapor çıkardığı sistem.",
                TechnicalDescription = "DJI Matrice LiDAR sensörü, Python PDAL nokta bulutu kütüphanesi, Open3D yüzey örme algoritmaları ve otomatik PDF raporlayıcı.",
                BusinessImpact = "Her ay yapılan stok sayım maliyetini %75 düşürmüş, ERP stok kayıtları ile gerçek saha tonajı farkını %0.8'e indirmiştir.",
                TargetAudience = "Harita Mühendisleri, Finans/Muhasebe Yöneticileri ve Stok Saha Sorumluları",
                StatusCode = "COMPLETED",
                CategoryCode = "MINING_TECHNOLOGY",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2022, 10, 15),
                EndDate = new DateOnly(2023, 8, 30),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "drone-surveying",
                PrimaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                SecondaryTeamName = "Veri Analitiği Ekibi",
                MemberEmails = ["kerem.vural@fictional-demirexport.com", "selin.yildiz@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Kangallı Altın Sahası", "İzmir Liman ve Lojistik Merkezi"],
                TechnologyNames = ["Python / PyTorch", "PostgreSQL", ".NET 9"],
                TagSlugs = ["drone-iha", "cbs-haritalama", "uretim-takibi", "lojistik-filo"]
            },

            // ── 20. Reaktif Güç ve Trafo İzleme Otomasyonu ────────────────────────
            new()
            {
                Name = "Reaktif Güç ve Trafo İzleme Otomasyonu",
                Slug = "reaktif-guc-ve-trafo-izleme-sistemi",
                ShortDescription = "34.5 kV ana trafo merkezlerinin harmoniklerini, sargı sıcaklıklarını ve reaktif ceza sınırlarını anlık izleyen enerji sistemi.",
                Description = "Şebekeden çekilen endüktif ve kapasitif reaktif güç oranlarını takip ederek dinamik kompanzasyon kademelerini otomatik yönetir.",
                Purpose = "Yüksek elektrik faturalarında reaktif enerji cezası ödenmesini engellemek ve trafo ömrünü uzatmak.",
                ProblemSolved = "Ağır değirmen motorlarının anlık devreye girmesinde oluşan harmonik bozulmalar ve trafo aşırı ısınması.",
                NonTechnicalDescription = "Elektrik dağıtım şirketinden ceza yememek için fabrikadaki koca elektrik trafolarının ısısını ve elektrik kalitesini canlı izleyen sayaç sistemi.",
                TechnicalDescription = "Janitza enerji analizörleri, Modbus RTU / TCP protokolleri, InfluxDB zaman serisi kaydı ve Grafana / React görselleştirme.",
                BusinessImpact = "Yıllık yaklaşık 85.000$ reaktif enerji cezası riskini tamamen ortadan kaldırmıştır.",
                TargetAudience = "Elektrik Bakım Mühendisleri, Enerji Yöneticileri ve Tesis Müdürleri",
                StatusCode = "ACTIVE",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 6, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "energy-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                SecondaryTeamName = "IoT ve Otomasyon Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com", "mehmet.demir@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi", "Malatya Hekimhan Peletleme Tesisi"],
                TechnologyNames = ["InfluxDB", "MQTT / IoT Edge", "React", "ASP.NET Core"],
                TagSlugs = ["enerji-verimliligi", "iot-sensor", "otomasyon"]
            },

            // ── 21. OTR Kaya Kamyonu Lastik Basınç ve Isı Takip (TPMS) ───────────
            new()
            {
                Name = "OTR Kaya Kamyonu Lastik Basınç ve Isı Takip (TPMS)",
                Slug = "lastik-basinc-ve-isi-takip-tpms",
                ShortDescription = "Tanesi 40.000$ olan dev kaya kamyonu lastiklerinin iç basınç ve sıcaklığını kablosuz sensörlerle takip eden koruma sistemi.",
                Description = "Lastik subaplarına takılan dayanıklı sensörler ile anlık basınç düşüşü ve aşırı ısınmada (TKPH limiti) kabin ve kontrol odasını uyarır.",
                Purpose = "Aşırı ısınmadan kaynaklanan lastik patlamalarını ve erken aşınmaları önleyerek lastik kullanım ömrünü %25 artırmak.",
                ProblemSolved = "Yaz aylarında rampada ağır yükle yürüyen kamyonların lastiklerinin patlayarak aracın devrilme riski ve devasa lastik maliyeti.",
                NonTechnicalDescription = "Kamyonların 4 metrelik tekerleklerinin havasını ve sıcaklığını ölçüp 'Lastik ısınıyor, kamyonu 10 dakika gölgede beklet' diyen sistem.",
                TechnicalDescription = "433 MHz RF lastik sensörleri, CAN-bus araç gateway, telemetri sunucusu ve C# ASP.NET Core analiz altyapısı.",
                BusinessImpact = "Lastik ömrünü ortalama 4.200 saatten 5.400 saate çıkararak yılda 320.000$ lastik amortisman tasarrufu sağlamıştır.",
                TargetAudience = "Lastik Bakım Atölye Şefleri, Filo Yöneticileri ve Kamyon Operatörleri",
                StatusCode = "COMPLETED",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2022, 5, 10),
                EndDate = new DateOnly(2023, 4, 20),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "haul-truck",
                PrimaryTeamName = "Lojistik ve Filo Optimizasyonu Ekibi",
                SecondaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                MemberEmails = ["onur.simsek@fictional-demirexport.com", "murat.koc@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = [".NET 9", "SQL Server", "MQTT / IoT Edge"],
                TagSlugs = ["lojistik-filo", "iot-sensor", "kestirimci-bakim"]
            },

            // ── 22. Tedarik Zinciri Kritik Yedek Parça Talep Tahmini ───────────────
            new()
            {
                Name = "Tedarik Zinciri Kritik Yedek Parça Talep Tahmini",
                Slug = "tedarik-zinciri-yedek-parca-tahmini",
                ShortDescription = "İş makinelerinin çalışma saatleri ve arıza geçmişine göre rulman, filtre ve pompa parça ihtiyaçlarını aylar öncesinden tahmin eden yapay zeka.",
                Description = "Tedarik süreleri 6 aya varan ithal maden yedek parçalarının stok maliyeti ile duruş riski arasındaki optimum sipariş noktasını hesaplar.",
                Purpose = "Acil siparişlerdeki yüksek hava kargo maliyetlerini önlemek ve stokta milyonlarca liralık atıl parça birikmesini engellemek.",
                ProblemSolved = "Kritik bir pompanın bozulduğunda yedek parçanın stokta olmaması nedeniyle tesisin 2 hafta beklemesi.",
                NonTechnicalDescription = "Gelecek 6 ayda hangi iş makinesinin hangi parçasının eskiyeceğini hesaplayıp yurt dışından tam vaktinde sipariş verdiren akıllı depo programı.",
                TechnicalDescription = "Python ARIMA ve XGBoost zaman serisi modelleri, SAP MM (Malzeme Yönetimi) RFC arayüzü ve PowerBI karar destek arayüzü.",
                BusinessImpact = "Stok devir hızını %30 artırmış, acil yurt dışı kargo harcamalarını yıllık 140.000$ azaltmıştır.",
                TargetAudience = "Satınalma Uzmanları, Ambar ve Stok Yöneticileri, Bakım Planlamacıları",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "DATA_ANALYTICS",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 7, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "warehouse-logistics",
                PrimaryTeamName = "Kurumsal Uygulamalar ve ERP Ekibi",
                SecondaryTeamName = "Veri Analitiği Ekibi",
                MemberEmails = ["gamze.aslan@fictional-demirexport.com", "ayse.kaya@fictional-demirexport.com"],
                LocationNames = ["Kayseri Develi Lojistik Sahası", "Genel Müdürlük (Ankara)"],
                TechnologyNames = ["Python / PyTorch", "SQL Server", "Power BI"],
                TagSlugs = ["veri-analitigi", "erp-entegrasyonu", "lojistik-filo"]
            },

            // ── 23. Patlayıcı Madde Barkodlu Depo ve Sevk Takibi ──────────────────
            new()
            {
                Name = "Patlayıcı Madde Barkodlu Depo ve Sevk Takibi",
                Slug = "patlayici-madde-stok-ve-sevk-takibi",
                ShortDescription = "Maden patlayıcı depolarındaki dinamit, emülsiyon ve elektronik kapsüllerin seri numaralarıyla yasal mevzuata tam uyumlu izlenmesi.",
                Description = "İçişleri Bakanlığı Patlayıcı Madde Bilgi Sistemi (PATBİS) entegrasyonu ile depodan ateşleme basamağına kadar uçtan uca izlenebilirlik.",
                Purpose = "Patlayıcı maddelerin her bir kapsülünün nerede, ne zaman ve kim tarafından kullanıldığının hatasız kayıt altına alınması.",
                ProblemSolved = "Manuel defter kayıtlarında yaşanan imza gecikmeleri ve yasal denetimlerdeki sayım tutarsızlıkları.",
                NonTechnicalDescription = "Maden deposundaki her bir dinamit lokumunun ve kapsülün barkodla okutulup hangi delikte patlatıldığını saniye saniye kaydeden emniyetli yazılım.",
                TechnicalDescription = "ASP.NET Core Web API, ATEX sertifikalı el terminalleri için optimize edilmiş React PWA arayüzü ve şifreli veri tabanı.",
                BusinessImpact = "Emniyet denetimlerinde %100 kusursuz kayıt ve sıfır uygunsuzluk raporu elde edilmiştir.",
                TargetAudience = "Ateşçiler, Patlatma Mühendisleri ve Depo Güvenlik Sorumluları",
                StatusCode = "COMPLETED",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2022, 11, 1),
                EndDate = new DateOnly(2023, 7, 30),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "software-platform",
                PrimaryTeamName = "Yazılım Geliştirme Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["deniz.arslan@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["ASP.NET Core", "React", "SQL Server"],
                TagSlugs = ["is-guvenligi", "saha-yonetimi", "siber-guvenlik"]
            },

            // ── 24. Jeolojik Karot Fotoğrafları Görüntü İşleme ve Çatlak Analizi ──
            new()
            {
                Name = "Jeolojik Karot Fotoğrafları Görüntü İşleme",
                Slug = "jeolojik-karot-goruntu-isleme",
                ShortDescription = "Sondajlardan çıkarılan karot sandıklarının yüksek çözünürlüklü fotoğraflarından RQD (Kaya Kalite Değeri) ve kırık sıklığını çıkaran yapay zeka.",
                Description = "Karot sandıklarındaki silindirik kayaçların çatlak açılarını, kırıntılı bölgelerini ve litoloji geçişlerini bilgisayarlı görüyle etiketler.",
                Purpose = "Jeologların karot loglama süresini 10 kat hızlandırmak ve jeoteknik veri standardizasyonu sağlamak.",
                ProblemSolved = "Farklı jeologların öznel gözlemlerinden kaynaklanan RQD sapmaları ve binlerce metrelik karotların elle ölçüm zorluğu.",
                NonTechnicalDescription = "Topraktan çıkarılan taş borularının fotoğrafını çekince yapay zekanın kayanın ne kadar sağlam ve çatlaklı olduğunu otomatik ölçtüğü yazılım.",
                TechnicalDescription = "Python PyTorch UNet segmentasyon mimarisi, OpenCV perspektif düzeltme, FastAI sınıflandırma ve web tabanlı inceleme arayüzü.",
                BusinessImpact = "Karot loglama hızını günde 25 metreden 250 metreye çıkarmış ve modelleme verimliliğini devasa oranda artırmıştır.",
                TargetAudience = "Arama Jeologları, Maden Jeoloji Mühendisleri ve Jeoteknik Ekip",
                StatusCode = "PILOT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 10, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "ai-computer-vision",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "selin.yildiz@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "Vue.js", "PostgreSQL"],
                TagSlugs = ["yapay-zeka", "goruntu-isleme", "jeolojik-modelleme"]
            },

            // ── 25. Akıllı Havalandırma Otomasyonu ve Gaz İzleme (VOD) ───────────
            new()
            {
                Name = "Akıllı Havalandırma Otomasyonu ve Gaz İzleme",
                Slug = "havalandirma-otomasyon-ve-gaz-izleme",
                ShortDescription = "Yeraltı maden galerilerindeki gaz yoğunluğu ve araç egzozuna göre ana fan debisini frekans konvertörleriyle ayarlayan VOD sistemi.",
                Description = "Ventilation on Demand (VOD) mantığıyla çalışan, CO, NO, CO2 ve CH4 gaz sensörleri ile fan motorlarını senkronize eden yeşil madencilik projesi.",
                Purpose = "Yeraltı hava kalitesini maksimumda tutarken devasa havalandırma fanlarının elektrik tüketimini %30 azaltmak.",
                ProblemSolved = "Galerilerde araç veya personel yokken dahi mega fanların tam güç çalışmasıyla harcanan milyonlarca liralık elektrik israfı.",
                NonTechnicalDescription = "Yeraltında araba çalışıp duman çıktığında havalandırma pervanelerini hızlandıran, ortam temizken rölantiye alan akıllı maden ciğeri.",
                TechnicalDescription = "Schneider PLC altyapısı, Modbus RTU gaz transmitterları, C# .NET 9 orkestrasyon servisi ve SCADA haberleşme sürücüleri.",
                BusinessImpact = "Yıllık 1.8 Milyon kWh elektrik tasarrufu sağlayarak karbon salımını 820 ton düşürmüştür.",
                TargetAudience = "Yeraltı Tesis Mühendisleri, Elektrik Bakım Şefleri ve İSG Ekipleri",
                StatusCode = "ACTIVE",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2023, 9, 15),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "control-room",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                SecondaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com", "ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Balıkesir Manyas Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["OPC UA", ".NET 9", "SQL Server"],
                TagSlugs = ["otomasyon", "enerji-verimliligi", "is-guvenligi", "scada-plc"],
                Integrations = [("Ana Fan SCADA Sistemi", IntegrationType.ExternalService, "İnverter frekans hızı komut iletimi")],
                Documents = [("Havalandırma Otomasyonu VOD Dokümanı", "teknik-mimari-ozeti.pdf", "Teknik Doküman", "Gaz sınır eşik değerleri ve motor kontrol döngüsü")]
            },

            // ── 26. Ekskavatör Kova Dişi Düşme Erken Tespit Sistemi ──────────────
            new()
            {
                Name = "Ekskavatör Kova Dişi Düşme Erken Tespit Sistemi",
                Slug = "ekskavator-kova-disi-dusme-tespiti",
                ShortDescription = "Ağır ekskavatörlerin çelik kova dişlerinin kopup kırıcıya gitmesini kamyon yükleme anında kamerayla tespit eden güvenlik çözümü.",
                Description = "Kaya yükleme anında kova diş profilini analiz eden termal/optik kamera ile düşen çelik dişin primer kırıcıyı tıkamasını önler.",
                Purpose = "Kırıcıya giren 40 kiloluk sert çelik dişlerin kırıcı çenesini patlatmasını ve 2 günlük tesis duruşunu engellemek.",
                ProblemSolved = "Ocakta kopan çelik kova dişinin kamyona fark edilmeden yüklenip kırma tesisine gitmesiyle oluşan devasa mekanik hasar.",
                NonTechnicalDescription = "Kamyona taş yükleyen kepçenin dişi kırılıp taşların arasına düştüğünde anında alarm çalıp o kamyonun kırıcıya döküm yapmasını engelleyen göz.",
                TechnicalDescription = "Edge AI kamera, YOLOv8 özel kova ucu modeli, LoRa telsiz haberleşmesi ve kırıcı döküm rampası otomatik bariyer kilitleme.",
                BusinessImpact = "Yılda ortalama 1 kırıcı çene kırılma felaketini engelleyerek 350.000$ tamir ve üretim duruşu maliyetini önlemektedir.",
                TargetAudience = "Kırma Tesis Şefleri, Açık Ocak Formenleri ve Ekskavatör Operatörleri",
                StatusCode = "PROOF_OF_CONCEPT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2025, 2, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "excavator",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "Saha Operasyonları",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "volkan.tekin@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "MQTT / IoT Edge"],
                TagSlugs = ["yapay-zeka", "kestirimci-bakim", "saha-yonetimi"]
            },

            // ── 27. Maden Kapanış ve Çevresel Rehabilitasyon Takibi ──────────────
            new()
            {
                Name = "Maden Kapanış ve Çevresel Rehabilitasyon Takibi",
                Slug = "maden-kapanis-ve-rehabilitasyon-takibi",
                ShortDescription = "İşletmesi tamamlanan pasa döküm sahalarındaki ağaçlandırma ve bitki örtüsü gelişimini uydu NDVI verileriyle izleyen sürdürülebilirlik yazılımı.",
                Description = "Sentinel-2 ve Landsat uydu spektral bantlarını işleyerek maden sahasındaki yeşillenme indeksini ve toprak tutunmasını raporlar.",
                Purpose = "Maden sonrası çevreye kazandırma taahhütlerinin bilimsel verilerle doğrulanması ve erozyon risklerinin izlenmesi.",
                ProblemSolved = "Geniş pasa sahalarındaki fidan tutma oranlarının arazide yaya olarak gezilerek tespit edilmesinin imkansızlığı.",
                NonTechnicalDescription = "Eski maden alanlarına diktiğimiz ağaçların ne kadar büyüdüğünü ve yeşerdiğini uzaydaki uydulardan fotoğraflayıp haritada gösteren çevreci program.",
                TechnicalDescription = "Python rasterio ve GeoPandas kütüphaneleri, Sentinel Hub API entegrasyonu, PostgreSQL / PostGIS ve React UI.",
                BusinessImpact = "Orman Genel Müdürlüğü ve Çevre Bakanlığı denetim raporlarını otomatikleştirerek resmi kabul süreçlerini hızlandırmıştır.",
                TargetAudience = "Çevre Mühendisleri, Kurumsal İletişim ve Sürdürülebilirlik Komitesi",
                StatusCode = "ACTIVE",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 12, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "environmental-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                SecondaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com", "selin.yildiz@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası", "Balıkesir Manyas Sahası"],
                TechnologyNames = ["Python / PyTorch", "PostgreSQL", "React"],
                TagSlugs = ["cevre-surdurulebilirlik", "cbs-haritalama", "drone-iha"]
            },

            // ── 28. Yeraltı Dizel Partikül Emisyon ve DPF İzleme ─────────────────
            new()
            {
                Name = "Yeraltı Dizel Partikül Emisyon ve DPF İzleme",
                Slug = "dizel-partikul-emisyon-izleme",
                ShortDescription = "Yeraltı yükleyici ve kamyonların egzoz dizel partikül filtre (DPF) doluluk ve geri basınç verilerini telemetriyle izleyen sistem.",
                Description = "Yeraltında çalışan ağır makinelerin egzoz gazı filtrelerinin tıkanma durumunu canlı izleyerek zehirli partikül salımını engeller.",
                Purpose = "Yeraltı madencilerinin soluduğu hava kalitesini korumak ve egzoz tıkanmasıyla oluşan motor yangınlarını önlemek.",
                ProblemSolved = "Tıkanmış DPF filtrelerinin egzoz borusunu 700°C üzerine ısıtarak yeraltında yangın riski oluşturması.",
                NonTechnicalDescription = "Yeraltındaki kepçelerin egzozunun temiz çalışıp çalışmadığını ölçen ve filtre dolduğunda tamirhaneye uyarı gönderen sistem.",
                TechnicalDescription = "CAN-bus diferansiyel basınç sensörleri, LoRaWAN yerel ağ aktarıcısı, InfluxDB ve ASP.NET Core alarm motoru.",
                BusinessImpact = "Yeraltı hava kalitesi partikül indeksinde %40 iyileşme sağlamış ve egzoz yangını riskini sıfıra indirmiştir.",
                TargetAudience = "Yeraltı Mekanik Atölye Şefleri ve Maden Emniyet Mühendisleri",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 11, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "iot-sensors",
                PrimaryTeamName = "IoT ve Otomasyon Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["mehmet.demir@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Balıkesir Manyas Sahası"],
                TechnologyNames = ["MQTT / IoT Edge", "InfluxDB", ".NET 9"],
                TagSlugs = ["iot-sensor", "is-guvenligi", "cevre-surdurulebilirlik"]
            },

            // ── 29. Trafo Merkezi Radyometrik Termal Kamera Denetimi ─────────────
            new()
            {
                Name = "Trafo Merkezi Radyometrik Termal Kamera Denetimi",
                Slug = "trafo-merkezi-isi-haritasi-termal-kamera",
                ShortDescription = "34.5 kV şalt sahası izolatör ve kesicilerindeki aşırı ısınmaları 7/24 termal kameralarla tarayan yangın önleme otomasyonu.",
                Description = "Radyometrik termal kameralar ile önceden tanımlanmış trafo buşing ve kablo pabucu noktalarındaki sıcaklık artışını piksel bazlı izler.",
                Purpose = "Gevşek bağlantı kaynaklı ark ve patlamaları henüz sıcaklık 60°C seviyesindeyken tespit ederek trafo yangınlarını önlemek.",
                ProblemSolved = "Periyodik el termal kamerası kontrolleri arasında aniden gelişen gevşek klemens ısınmalarının fark edilememesi.",
                NonTechnicalDescription = "Şalt sahasındaki elektrik direklerine bakan termal kameraların 'Bu kablo pabucu fazla ısınıyor, yangın çıkabilir' diye alarm vermesi.",
                TechnicalDescription = "FLIR radyometrik termal kamera SDK, Python OpenCV ısı matrisi analitiği, OPC UA SCADA bağlantısı.",
                BusinessImpact = "Tesis ana trafosunda oluşabilecek 500.000$ değerindeki patlama ve 10 günlük fabrika duruş riskini engellemiştir.",
                TargetAudience = "Elektrik Bakım Mühendisleri ve Yüksek Gerilim İşletme Şefleri",
                StatusCode = "COMPLETED",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2022, 9, 1),
                EndDate = new DateOnly(2023, 6, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "control-room",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                SecondaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com", "murat.koc@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Sivas Kangal Zenginleştirme Tesisi"],
                TechnologyNames = ["OpenCV", "Python / PyTorch", "OPC UA"],
                TagSlugs = ["otomasyon", "kestirimci-bakim", "is-guvenligi"]
            },

            // ── 30. CBS Tabanlı Arazi Kamulaştırma ve Ruhsat İzin Takibi ──────────
            new()
            {
                Name = "CBS Tabanlı Arazi Kamulaştırma ve Ruhsat İzin Takibi",
                Slug = "cbs-tabanli-arazi-kamulastirma-ve-izin",
                ShortDescription = "Maden ruhsat sahalarındaki orman izinleri, mera vasıfları ve kadastro parsel mülkiyet durumunu harita üzerinde yöneten platform.",
                Description = "Tapu ve Kadastro (MEGSİS) ile Orman Bakanlığı izin katmanlarını maden işletme ruhsat sınırlarıyla çakıştıran web CBS sistemi.",
                Purpose = "Ruhsat ve orman izin sürelerinin yaklaşan bitiş tarihlerini otomatik takip ederek maden operasyonunun kesintiye uğramasını önlemek.",
                ProblemSolved = "Yüzlerce parselin ve izin evrakının Excel ve klasörlerde takip edilmesi sonucu yaşanan izin yenileme gecikmeleri.",
                NonTechnicalDescription = "Madenin bulunduğu dağdaki hangi tarlanın kime ait olduğunu ve devletten alınan orman izinlerinin süresini haritada gösteren sistem.",
                TechnicalDescription = "PostGIS mekânsal veritabanı, GeoServer WMS/WFS harita servisleri, React OpenLayers harita bileşeni ve ASP.NET Core.",
                BusinessImpact = "Kamulaştırma ve orman izin takip süreçlerindeki bürokratik gecikmeleri %70 oranında azaltmıştır.",
                TargetAudience = "Harita Mühendisleri, Ruhsat ve İzin Direktörlüğü, Hukuk Müşavirliği",
                StatusCode = "COMPLETED",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2022, 3, 1),
                EndDate = new DateOnly(2023, 1, 30),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "mapping-gis",
                PrimaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                SecondaryTeamName = "Kurumsal Uygulamalar ve ERP Ekibi",
                MemberEmails = ["selin.yildiz@fictional-demirexport.com", "gamze.aslan@fictional-demirexport.com"],
                LocationNames = ["Genel Müdürlük (Ankara)", "Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["PostgreSQL", "React", "ASP.NET Core"],
                TagSlugs = ["cbs-haritalama", "saha-yonetimi", "erp-entegrasyonu"]
            },

            // ── 31. Maden Makine Operatör Yetkinlik ve Sertifika Matrisi ─────────
            new()
            {
                Name = "Maden Makine Operatör Yetkinlik ve Sertifika Matrisi",
                Slug = "personel-yetkinlik-ve-sertifika-matrisi",
                ShortDescription = "Ekskavatör, loder ve delici operatörlerinin ehliyet, G sınıfı iş makinesi sertifikası ve sağlık muayene tarihlerini takip eden İK modülü.",
                Description = "Sertifikası veya periyodik sağlık muayenesi dolan operatörlerin araç çalıştırmasını araç takip sistemiyle entegre olarak engeller.",
                Purpose = "Yasal geçerliliği bitmiş sertifikayla veya sağlık onayı olmadan ağır iş makinesi kullanımından doğan yasal sorumlulukları önlemek.",
                ProblemSolved = "Yüzlerce şoförün sertifika yenileme tarihlerinin insan kaynakları tarafından manuel takibinde yaşanan atlamalar.",
                NonTechnicalDescription = "Ehliyeti veya sağlık raporu biten kepçe şoförünün araca bindiğinde kartını okuttuğunda kontağın açılmasını engelleyen güvenlik sistemi.",
                TechnicalDescription = "C# .NET 9 Web API, React frontend, SAP HR entegrasyonu ve araç içi RFID kart okuyucu kontrol servisi.",
                BusinessImpact = "Yasal İSG mevzuatı denetimlerinde %100 ehliyet ve yetkinlik uygunluğu sağlamıştır.",
                TargetAudience = "İnsan Kaynakları Uzmanları, İSG Ekipleri ve Saha Formenleri",
                StatusCode = "ACTIVE",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 11, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "software-platform",
                PrimaryTeamName = "Kurumsal Uygulamalar ve ERP Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["gamze.aslan@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası", "Balıkesir Manyas Sahası"],
                TechnologyNames = [".NET 9", "React", "SQL Server"],
                TagSlugs = ["is-guvenligi", "erp-entegrasyonu", "saha-yonetimi"]
            },

            // ── 32. Drenaj Pompa İstasyonu Vibrasyon ve Debi Kontrolü ───────────
            new()
            {
                Name = "Drenaj Pompa İstasyonu Vibrasyon ve Debi Kontrolü",
                Slug = "pompa-istasyonu-vibrasyon-ve-debi-kontrolu",
                ShortDescription = "Açık ocak su tahliye pompalarının debi, kavitasyon ve yatak titreşimlerini online izleyerek ocak su basmasını önleyen telemetri.",
                Description = "Ağır hizmet çamur pompalarında kavitasyon aşınmasını erken tespit eden piezoelektrik titreşim ve elektromanyetik debimetre sistemi.",
                Purpose = "Yoğun bahar yağmurlarında maden çukurunu su basmasını önlemek ve drenaj pompalarının arızasız çalışmasını sağlamak.",
                ProblemSolved = "Pompa kavitasyonu sonucu çarkın aniden parçalanmasıyla ocak tabanındaki zengin cevher aynasının sular altında kalması.",
                NonTechnicalDescription = "Maden çukurundaki yağmur sularını dışarı basan dev pompaların bozulmadan önce titreşiminden anlayıp bakım ekibini çağıran sistem.",
                TechnicalDescription = "IFM titreşim sensörleri, Modbus TCP gateway, InfluxDB ve Node.js telemetri motoru.",
                BusinessImpact = "Geçtiğimiz bahar sezonunda 2 kritik pompa arızasını erkenden önleyerek ocak tabanında 4 günlük üretim duruşunu engellemiştir.",
                TargetAudience = "Su Tahliye Ekipleri, Mekanik Bakım Şefleri ve Maden Mühendisleri",
                StatusCode = "ACTIVE",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 3, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "industrial-automation",
                PrimaryTeamName = "IoT ve Otomasyon Ekibi",
                SecondaryTeamName = "Kestirimci Bakım ve Güvenilirlik Ekibi",
                MemberEmails = ["mehmet.demir@fictional-demirexport.com", "murat.koc@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["InfluxDB", "MQTT / IoT Edge", "React"],
                TagSlugs = ["otomasyon", "kestirimci-bakim", "saha-yonetimi"]
            },

            // ── 33. Kaya Kamyonu Sürücü Yorgunluk ve Dikkat Dağınıklığı Tespiti ───
            new()
            {
                Name = "Kaya Kamyonu Sürücü Yorgunluk ve Dikkat Tespiti",
                Slug = "kaya-kamyonu-yorgunluk-tespit-sistemi",
                ShortDescription = "Gece vardiyasında çalışan dev kaya kamyonu şoförlerinin göz kırpma, esneme ve dikkat dağınıklığını kızılötesi kamerayla izleyen yapay zeka.",
                Description = "Kabin içi kızılötesi kamera ve yapay zeka ile göz kapağı kapanma süresi (PERCLOS) ve esneme sıklığını analiz edip koltuğu titreten sistem.",
                Purpose = "Özellikle sabaha karşı 03:00-05:00 saatleri arasında yaşanan uyuklama kaynaklı şevden yuvarlanma ve çarpışma kazalarını önlemek.",
                ProblemSolved = "Gece vardiyalarında monoton maden rampalarında şoförlerin saniyeler süren mikro-uyku nöbetlerine girmesi.",
                NonTechnicalDescription = "Gece kamyon sürerken gözü kapanan veya dikkati dağılan şoförü anında koltuğunu titretip kabinde sesli siren çalarak uyandıran hayat kurtarıcı sistem.",
                TechnicalDescription = "Kızılötesi aydınlatmalı kabin kamerası, Edge AI işlemci üzerinde koşan yüz mihenk noktası modeli, CAN-bus ve titreşimli koltuk aktüatörü.",
                BusinessImpact = "Pilot uygulandığı sahalarda gece vardiyası şerit ihlali ve uyuklama olaylarını %96 oranında azaltmıştır.",
                TargetAudience = "Kamyon Şoförleri, Gece Vardiya Amirleri ve İSG Ekipleri",
                StatusCode = "ACTIVE",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 5, 20),
                IsFeatured = true,
                IsPublished = true,
                CoverImageTheme = "haul-truck",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "İSG Dijital Takip Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "elif.demir@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Kangallı Altın Sahası"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "MQTT / IoT Edge"],
                TagSlugs = ["is-guvenligi", "yapay-zeka", "lojistik-filo", "goruntu-isleme"],
                GalleryImages = [("haul-truck", "Kabin İçi Kamera Montajı", "Kızılötesi şoför yüz analiz kamerası"), ("safety-monitoring", "Dikkat Dağınıklığı Alarmı", "Göz kapanma süresi uyarı grafiği")]
            },

            // ── 34. Peletleme Fırını Sıcaklık Profili Yapay Zeka Optimizasyonu ────
            new()
            {
                Name = "Peletleme Fırını Sıcaklık Profili Yapay Zeka Optimizasyonu",
                Slug = "peletleme-firini-sicaklik-profili-yapay-zeka",
                ShortDescription = "Demir cevheri peletleme döner fırınındaki 1.250°C sıcaklık dağılımını ve brülör yakıt beslemesini optimize eden yapay zeka.",
                Description = "Fırın boyu termokupl sensörleri ve baca gazı analizörlerinden beslenen sinir ağı ile pelet mekanik mukavemetini maksimize eder.",
                Purpose = "Fırın doğalgaz ve kömür tozu tüketimini düşürürken üretilen peletlerin kırılma/basma dayanımını standart üstü tutmak.",
                ProblemSolved = "Fırındaki sıcaklık dengesizlikleri nedeniyle peletlerin ya çiğ kalıp ufalanması ya da aşırı pişip fırın cidarına yapışması.",
                NonTechnicalDescription = "Demir tozlarını pişiren dev fırının ateşini tam kıvamında tutarak doğalgaz faturasını düşüren ve en kaliteli demir peletini üreten yapay zeka.",
                TechnicalDescription = "Python TensorFlow LSTM zaman serisi modeli, Yokogawa DCS sistemi OPC UA entegrasyonu, model predictive control (MPC).",
                BusinessImpact = "Pelet basma mukavemetinde %4.2 artış sağlarken fırın doğalgaz tüketiminde %6.5 tasarruf sağlamıştır.",
                TargetAudience = "Peletleme Tesis Şefleri, Proses Kontrol Mühendisleri ve Metalurji Uzmanları",
                StatusCode = "ACTIVE_DEVELOPMENT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 8, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "industrial-dashboard",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "can.ozkan@fictional-demirexport.com"],
                LocationNames = ["Malatya Hekimhan Peletleme Tesisi", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "TensorFlow", "OPC UA", "PostgreSQL"],
                TagSlugs = ["yapay-zeka", "enerji-verimliligi", "otomasyon", "uretim-takibi"]
            },

            // ── 35. Saha Sayısal Telsiz ve Mesh Haberleşme Altyapısı ──────────────
            new()
            {
                Name = "Saha Sayısal Telsiz ve Mesh Haberleşme Altyapısı",
                Slug = "saha-sayisal-telsiz-ve-mesh-haberlesme",
                ShortDescription = "Derin maden çukurlarında kesintisiz ses ve telemetri aktarımı sağlayan DMR Tier III sayısal telsiz ve Wi-Fi Mesh şebekesi.",
                Description = "Baz istasyonu kapsamasının girmediği 300 metre derinlikteki basamaklarda güneş enerjili mobil mesh tekrarlayıcılarla kurulan iletişim ağı.",
                Purpose = "Kamyon telemetrisi, acil durum ses haberleşmesi ve mobil formen tabletlerinin madenin en ücra noktasında dahi online kalması.",
                ProblemSolved = "Açık ocak derinleştikçe GSM çekiminin kaybolması ve araçların kontrol odasıyla iletişiminin kopması.",
                NonTechnicalDescription = "Madenin derinliklerinde cep telefonu çekmediği için kamyonların ve telsizlerin birbirine bağlanarak yeryüzüne internet ulaştırdığı telsiz ağı.",
                TechnicalDescription = "Motorola MotoTRBO DMR Tier III altyapısı, Rajant Kinetic Mesh kablosuz düğümleri, SNMP ağ izleme yazılımı.",
                BusinessImpact = "Saha genelinde haberleşme kör noktalarını sıfırlamış ve telemetri veri kaybı oranını %22'den %0.3'e düşürmüştür.",
                TargetAudience = "Haberleşme & IT Teknisyenleri, Saha Operasyon Şefleri ve Emniyet Birimi",
                StatusCode = "COMPLETED",
                CategoryCode = "IOT",
                DevelopmentType = DevelopmentType.External,
                StartDate = new DateOnly(2022, 4, 1),
                EndDate = new DateOnly(2023, 2, 28),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "control-room",
                PrimaryTeamName = "Saha Teknolojileri ve İletişim Ekibi",
                SecondaryTeamName = "Bilgi Teknolojileri ve Siber Güvenlik",
                MemberEmails = ["emre.polat@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["MQTT / IoT Edge", "Linux", "Docker / Kubernetes"],
                TagSlugs = ["telemetri", "iot-sensor", "saha-yonetimi"]
            },

            // ── 36. Cevher Flotasyon Köpük Görüntü Analizi ve Reaktif Dozajlama ───
            new()
            {
                Name = "Cevher Flotasyon Köpük Görüntü Analizi",
                Slug = "cevher-zenginlestirme-flotasyon-kopuk-analizi",
                ShortDescription = "Flotasyon hücrelerindeki köpük rengi, kabarcık boyutu ve akış hızını kameralarla analiz edip reaktif kimyasal dozajını ayarlayan sistem.",
                Description = "Köpük dinamiğini görüntü işleme ile ölçerek kollektör ve köpürtücü kimyasalların gereksiz kullanımını önler.",
                Purpose = "Konsantre tenörünü hedeflenen %68 Fe seviyesinde tutarken atığa kaçan metal kaybını en aza indirmek.",
                ProblemSolved = "Operatörün köpüğe göz kararı bakarak kimyasal vanasını açıp kapamasındaki kişisel dalgalanmalar.",
                NonTechnicalDescription = "Cevheri sudan ayıran köpüğün kabarcıklarını kamerayla izleyip kimyasal ilacın tam kıvamında damlatılmasını sağlayan akıllı göz.",
                TechnicalDescription = "Endüstriyel renkli kameralar, Python OpenCV kabarcık segmentasyon algoritması, C# .NET dozaj kontrol modülü.",
                BusinessImpact = "Flotasyon reaktif kimyasal tüketimini %14 azaltmış ve konsantre tenör kararlılığını %98'e çıkarmıştır.",
                TargetAudience = "Cevher Zenginleştirme Mühendisleri, Flotasyon Operatörleri ve Kimyagerler",
                StatusCode = "PILOT",
                CategoryCode = "ARTIFICIAL_INTELLIGENCE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2024, 12, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "ai-computer-vision",
                PrimaryTeamName = "Yapay Zeka ve Görüntü İşleme Ekibi",
                SecondaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                MemberEmails = ["burak.aydin@fictional-demirexport.com", "can.ozkan@fictional-demirexport.com"],
                LocationNames = ["Sivas Kangal Zenginleştirme Tesisi", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "OpenCV", "OPC UA"],
                TagSlugs = ["yapay-zeka", "cevher-kalite", "goruntu-isleme", "otomasyon"]
            },

            // ── 37. Otonom Maden Yolu Sulama ve Toz Kontrolü ─────────────────────
            new()
            {
                Name = "Otonom Maden Yolu Sulama ve Toz Kontrolü",
                Slug = "otonom-yol-sulama-ve-toz-kontrolu",
                ShortDescription = "Açık ocak nakliye yollarındaki nem oranı, hava sıcaklığı ve rüzgara göre sulama arazözlerini otomatik yönlendiren sevk sistemi.",
                Description = "Yol zemin nemini izleyen kablosuz sensörler ile arazözlerin yolu aşırı sulayıp çamur yapmasını veya kuru bırakmasını engeller.",
                Purpose = "Kamyon kaymalarını önlemek, görüşü kapatan tozu bastırmak ve arazöz yakıt/su israfını engellemek.",
                ProblemSolved = "Yolların aşırı sulanması sonucu 150 tonluk kamyonların şev kenarında kızaklayarak kayma riski.",
                NonTechnicalDescription = "Maden yollarının kuruduğunu anlayıp en yakın su kamyonuna 'Bu yola 1 tur su sık' diye otomatik rota çizen akıllı sistem.",
                TechnicalDescription = "LoRaWAN zemin nem sensörleri, arazöz kabin navigasyon tableti, Python rotalama algoritması ve GIS harita arayüzü.",
                BusinessImpact = "Sulama suyu tüketiminde %40 tasarruf sağlarken maden yolu toz kaynaklı görüş kaza riskini sıfırlamıştır.",
                TargetAudience = "Yol Bakım Formenleri, Arazöz Şoförleri ve İSG Denetçileri",
                StatusCode = "ACTIVE",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 5, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "open-pit-mine",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                SecondaryTeamName = "Lojistik ve Filo Optimizasyonu Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com", "onur.simsek@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası", "Kangallı Altın Sahası"],
                TechnologyNames = ["Python / PyTorch", "PostgreSQL", "LoRaWAN", "Flutter"],
                TagSlugs = ["otomasyon", "saha-yonetimi", "cevre-surdurulebilirlik"]
            },

            // ── 38. Maden Sahası Enerji Depolama ve Mikroşebeke Fizibilitesi ──────
            new()
            {
                Name = "Maden Sahası Enerji Depolama ve Mikroşebeke Fizibilitesi",
                Slug = "enerji-depolama-ve-mikrosebeke-optimizasyonu",
                ShortDescription = "Güneş enerjisi santrali (GES) ve LiFePO4 batarya depolama sistemi ile maden sahasının şebekeden bağımsız çalışma simülasyonu.",
                Description = "Gündüz üretilen güneş enerjisinin bataryalara depolanarak puant saatlerde kırıcı ve tesis yükünü beslemesini modeller.",
                Purpose = "Elektrik enerjisi maliyetlerini düşürmek ve elektrik kesintilerinde tesisin kritik ünitelerini devrede tutmak.",
                ProblemSolved = "Fırtına ve kar yağışlarında enterkonnekte şebekede yaşanan kesintilerin maden üretimini durdurması.",
                NonTechnicalDescription = "Madenin yanına kurulacak güneş panelleri ve dev pillerin fabrikayı elektrik kesildiğinde kaç saat çalıştıracağını hesaplayan Ar-Ge projesi.",
                TechnicalDescription = "HOMER Pro mikroşebeke optimizasyonu, Python enerji akış simülatörü, SCADA güç profili veri madenciliği.",
                BusinessImpact = "Yıllık elektrik faturasında potansiyel 650.000$ tasarruf ve karbon kredisi getirisi öngörülmektedir.",
                TargetAudience = "Enerji Yöneticileri, Yatırım Planlama Direktörlüğü ve Tesis Müdürleri",
                StatusCode = "PLANNING",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2025, 5, 1),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "energy-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                SecondaryTeamName = "Ar-Ge ve Dijital Dönüşüm",
                MemberEmails = ["ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["Python / PyTorch", "Power BI"],
                TagSlugs = ["enerji-verimliligi", "cevre-surdurulebilirlik"]
            },

            // ── 39. Hidrojeolojik Yeraltı Suyu İzleme ve Piezometre Ağı ───────────
            new()
            {
                Name = "Hidrojeolojik Yeraltı Suyu İzleme ve Piezometre Ağı",
                Slug = "hidrojeolojik-yeralti-suyu-izleme-agı",
                ShortDescription = "Ocak etrafındaki piezometre kuyularından su basıncı ve seviye verilerini telemetriyle toplayan zemin mekaniği sistemi.",
                Description = "Yeraltı suyu seviyesindeki mevsimsel değişimlerin ocak şev duraylılığına etkisini gerçek zamanlı modelleyen hidrojeoloji yazılımı.",
                Purpose = "Aşırı su basıncı birikimini önceden tespit ederek şev göçmelerini engellemek ve susuzlaştırma kuyularını yönetmek.",
                ProblemSolved = "Zemin altındaki görünmeyen su tablası yükselmelerinin şev basamaklarını sıvılaştırıp kaydırması.",
                NonTechnicalDescription = "Toprağın derinliklerindeki su miktarını ve basıncını ölçen yer altı barometrelerinin verilerini toplayan çevre ve emniyet sistemi.",
                TechnicalDescription = "Titreşen telli (vibrating wire) piezometreler, LoRaWAN veri düğümleri, InfluxDB ve Python interpolasyon modelleri.",
                BusinessImpact = "Şev kayma risklerini en aza indirmiş ve susuzlaştırma pompa enerji maliyetini %18 düşürmüştür.",
                TargetAudience = "Hidrojeologlar, Jeoteknik Uzmanları ve Çevre Denetçileri",
                StatusCode = "ACTIVE",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2023, 8, 10),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "mapping-gis",
                PrimaryTeamName = "CBS ve Jeolojik Modelleme Ekibi",
                SecondaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                MemberEmails = ["selin.yildiz@fictional-demirexport.com", "ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = ["LoRaWAN", "InfluxDB", "Python / PyTorch"],
                TagSlugs = ["cevre-surdurulebilirlik", "cbs-haritalama", "iot-sensor"]
            },

            // ── 40. Mobil Ekipman Lastik Değişim ve Rotasyon Yönetimi ────────────
            new()
            {
                Name = "Mobil Ekipman Lastik Değişim ve Rotasyon Yönetimi",
                Slug = "lastik-degisim-ve-rotasyon-yonetimi",
                ShortDescription = "Dev kaya kamyonu lastiklerinin aşınma profiline göre ön/arka aks rotasyon zamanlamasını yöneten filo bakım sistemi.",
                Description = "Lastik diş derinliği lazer taramaları ve kilometre kayıtlarını birleştirerek lastik ömrünü maksimize eden rotasyon çizelgesi.",
                Purpose = "Ön akstaki yönlendirme lastiklerinin erken aşınmasını önleyerek lastik başı ton-kilometre (TKPH) verimini artırmak.",
                ProblemSolved = "Düzensiz aşınan lastiklerin erken hurdaya ayrılması sonucu oluşan yüksek filo bakım giderleri.",
                NonTechnicalDescription = "Hangi kamyonun hangi tekerleğinin diğer tekerlekle ne zaman yer değiştirmesi gerektiğini hesaplayan lastik tasarruf sistemi.",
                TechnicalDescription = "C# .NET 9 Web API, Vue.js arayüzü, el tipi lazer profil tarayıcı veri aktarımı ve SQL Server.",
                BusinessImpact = "Lastik hurda oranını %18 azaltmış ve yıllık 180.000$ lastik alım tasarrufu sağlamıştır.",
                TargetAudience = "Lastik Atölye Şefleri, Bakım Planlamacıları ve Filo Mühendisleri",
                StatusCode = "COMPLETED",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2022, 6, 1),
                EndDate = new DateOnly(2023, 3, 15),
                IsFeatured = false,
                IsPublished = true,
                CoverImageTheme = "haul-truck",
                PrimaryTeamName = "Lojistik ve Filo Optimizasyonu Ekibi",
                SecondaryTeamName = "Yazılım Geliştirme Ekibi",
                MemberEmails = ["onur.simsek@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası", "Divriği Demir Sahası"],
                TechnologyNames = [".NET 9", "Vue.js", "SQL Server"],
                TagSlugs = ["lojistik-filo", "kestirimci-bakim"]
            },

            // ── 41. Taslak Proje: Otonom Yeraltı Yükleyici (LHD) Geliştirme ───────
            new()
            {
                Name = "Otonom Yeraltı Yükleyici (LHD) Pilot Taslağı",
                Slug = "taslak-otonom-yukleyici-lhd-gelistirme",
                ShortDescription = "Yeraltı galerilerinde uzaktan kumandalı veya otonom çalışan kepçeli yükleyici (LHD) prototip projesi.",
                Description = "Riskli göçük tehlikesi olan arınlarda operatörsüz cevher yükleme yapabilecek otonom navigasyon fizibilitesi.",
                Purpose = "İnsan hayatını riske atmadan tehlikeli yeraltı cevher aynalarından üretim yapılmasını sağlamak.",
                ProblemSolved = "Arın tahkimatı tamamlanmamış bölgelerde operatörlü makine çalıştırma riskleri.",
                NonTechnicalDescription = "Yeraltında insanların giremeyeceği tehlikeli yerlere kendi kendine girip taş yükleyip çıkan uzaktan kumandalı kepçe taslağı.",
                TechnicalDescription = "ROS2 (Robot Operating System), LiDAR SLAM navigasyon, teleoperasyon düşük gecikmeli Wi-Fi 6 arayüzü.",
                StatusCode = "PLANNING",
                CategoryCode = "AUTOMATION",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2026, 1, 15),
                IsFeatured = false,
                IsPublished = false, // DRAFT
                CoverImageTheme = "industrial-automation",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası"],
                TechnologyNames = ["ROS2", "Python / PyTorch", "Docker / Kubernetes"],
                TagSlugs = ["otomasyon", "is-guvenligi", "dijital-ikiz"]
            },

            // ── 42. Taslak Proje: Hidrojen Yakıt Hücreli Kamyon Ar-Ge ─────────────
            new()
            {
                Name = "Hidrojen Yakıt Hücreli Ağır Kamyon Ar-Ge",
                Slug = "taslak-hidrojen-yakit-hucreli-kamyon-ar-gesi",
                ShortDescription = "Dizel maden kamyonlarının sıfır emisyonlu yeşil hidrojen yakıt hücresiyle dönüştürülmesi ön fizibilitesi.",
                Description = "Ağır madencilikte fosil yakıt tüketimini tamamen sıfırlayacak hidrojen tahrik ve batarya hibrit güç mimarisi araştırması.",
                Purpose = "Demir Export 2040 net-sıfır karbon stratejisi kapsamında maden filolarının karbonsuzlaştırılması.",
                ProblemSolved = "Elektrikli bataryaların aşırı ağır olması nedeniyle büyük tonajlı maden kamyonlarında menzil yetersizliği.",
                NonTechnicalDescription = "Egzozundan sadece su buharı çıkaran hidrojenle çalışan çevre dostu dev maden kamyonu geliştirme fikri.",
                TechnicalDescription = "Yakıt hücresi simülasyonu, termal yönetim modellemesi, hidrojen depolama güvenlik analizleri.",
                StatusCode = "PLANNING",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Hybrid,
                StartDate = new DateOnly(2026, 3, 1),
                IsFeatured = false,
                IsPublished = false, // DRAFT
                CoverImageTheme = "haul-truck",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Kangallı Altın Sahası"],
                TechnologyNames = ["Python / PyTorch"],
                TagSlugs = ["cevre-surdurulebilirlik", "enerji-verimliligi", "lojistik-filo"]
            },

            // ── 43. Taslak Proje: Yüksek Hızlı Cevher XRF Ayıklama Robotu ─────────
            new()
            {
                Name = "Yüksek Hızlı Cevher XRF Pnömatik Ayıklama Robotu",
                Slug = "taslak-cevher-xrf-ayiklama-robotu",
                ShortDescription = "Konveyörden akan taşların tenörünü X-ışını ile saniyenin yüzde birinde okuyup pasaları hava jetiyle fırlatan robotik ayırıcı.",
                Description = "Düşük tenörlü cevherleri zenginleştirme tesisine girmeden önce kuru ortamda zenginleştiren pnömatik sensörlü ayıklayıcı.",
                Purpose = "Kırıcı ve değirmenlere boş taş girmesini önleyerek tesis elektrik ve su maliyetini %25 düşürmek.",
                ProblemSolved = "Değersiz pasa taşlarının kırılıp öğütülerek fabrikada gereksiz kapasite işgal etmesi.",
                NonTechnicalDescription = "Banttan hızla geçen taşların içindeki demir oranını röntgenle ölçüp çürük taşları hava tabancasıyla kenara fırlatan akıllı robot.",
                TechnicalDescription = "XRF line-scan spektrometresi, FPGA yüksek hızlı tetikleme kartı, yüksek hızlı pnömatik vana matrisi.",
                StatusCode = "PROOF_OF_CONCEPT",
                CategoryCode = "MINING_TECHNOLOGY",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2025, 6, 1),
                IsFeatured = false,
                IsPublished = false, // DRAFT
                CoverImageTheme = "industrial-equipment",
                PrimaryTeamName = "Süreç Otomasyonu ve Robotik Ekibi",
                MemberEmails = ["can.ozkan@fictional-demirexport.com"],
                LocationNames = ["Divriği Demir Sahası"],
                TechnologyNames = ["C++", "Python / PyTorch", "Linux"],
                TagSlugs = ["otomasyon", "cevher-kalite", "uretim-takibi"]
            },

            // ── 44. Taslak Proje: Blokzincir Tabanlı Maden Tedarik İzlenebilirliği ──
            new()
            {
                Name = "Blokzincir Tabanlı Maden Tedarik İzlenebilirliği",
                Slug = "taslak-blok-zincir-maden-izlenebilirlik",
                ShortDescription = "Üretilen altın ve demir cevherinin ocaktan son sanayi tüketicisine kadar etik ve ESG sertifikalı blokzincir dijital pasaportu.",
                Description = "Sürdürülebilir madencilik kriterleri ve karbon emisyonu ayak izinin değiştirilemez dağıtık defter teknolojisiyle sertifikasyonu.",
                Purpose = "Uluslararası pazarlarda yeşil madencilik ve sorumlu kaynak temini primli fiyat avantajı elde etmek.",
                ProblemSolved = "Yeşil çelik üreticilerinin tedarikçilerden istediği karbon ve menşei ispat dokümanlarının doğrulanabilirlik zorluğu.",
                NonTechnicalDescription = "Çıkardığımız madenin çevreye zarar verilmeden çıkarıldığını kanıtlayan ve müşterilere güven veren dijital maden pasaportu taslağı.",
                TechnicalDescription = "Ethereum uyumlu özel Hyperledger ağı, akıllı sözleşmeler, Node.js API ve QR kodlu ürün sertifikası.",
                StatusCode = "PLANNING",
                CategoryCode = "SOFTWARE",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2026, 2, 1),
                IsFeatured = false,
                IsPublished = false, // DRAFT
                CoverImageTheme = "software-platform",
                PrimaryTeamName = "Yazılım Geliştirme Ekibi",
                MemberEmails = ["ahmet.yilmaz@fictional-demirexport.com"],
                LocationNames = ["Genel Müdürlük (Ankara)"],
                TechnologyNames = ["Node.js", "Next.js", "Docker / Kubernetes"],
                TagSlugs = ["cevre-surdurulebilirlik", "siber-guvenlik", "erp-entegrasyonu"]
            },

            // ── 45. Taslak Proje: Derin Maden Suyu Jeotermal Isı Geri Kazanımı ────
            new()
            {
                Name = "Derin Maden Suyu Jeotermal Isı Geri Kazanımı",
                Slug = "taslak-jeotermal-isi-kazanim-fizibilitesi",
                ShortDescription = "Derin yeraltı ocaklarından pompalanan 42°C sıcaklığındaki drenaj suyunun ısı pompalarıyla idari binaları ısıtması projesi.",
                Description = "Yeraltından tahliye edilen sıcak suyun termal enerjisinin ısı eşanjörleri ile kampüs ve atölye ısıtmasında kullanılması fizibilitesi.",
                Purpose = "Kış aylarında maden kampüsünün kömür ve doğalgaz ısıtma giderlerini sıfıra yaklaştırmak.",
                ProblemSolved = "Drenaj suyunun soğumadan doğaya bırakılması ve aynı anda binaları ısıtmak için fosil yakıt yakılması.",
                NonTechnicalDescription = "Madenin dibinden çıkan sıcak suyla kışın işçi yatakhanelerini ve atölyeleri bedavaya ısıtma projesi.",
                TechnicalDescription = "Isı değiştirici termodinamik hesaplamaları, SCADA debi entegrasyonu, enerji ROI hesaplama aracı.",
                StatusCode = "PLANNING",
                CategoryCode = "RD",
                DevelopmentType = DevelopmentType.Internal,
                StartDate = new DateOnly(2025, 9, 1),
                IsFeatured = false,
                IsPublished = false, // DRAFT
                CoverImageTheme = "energy-monitoring",
                PrimaryTeamName = "Çevre ve Enerji Yönetimi Ekibi",
                MemberEmails = ["ebru.celik@fictional-demirexport.com"],
                LocationNames = ["Balıkesir Manyas Sahası"],
                TechnologyNames = ["Python / PyTorch", "Power BI"],
                TagSlugs = ["enerji-verimliligi", "cevre-surdurulebilirlik"]
            }
        };
    }
}
