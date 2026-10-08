using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Seeders;

/// <summary>
/// Proje Kütüphanesi için 20 adet zengin, derinlikli ve tutarlı kurumsal proje tanımı.
/// Tüm metinler madencilik ve kurumsal yazılım terminolojisine uygun olarak hazırlanmıştır.
/// Ölçülmüş operasyonel KPI iddialarından (ör. %30 tasarruf) kaçınılmış, niteliksel iş etkisi hedefleri vurgulanmıştır.
/// </summary>
public static class CuratedProjectDefinitions
{
    public sealed record ProjectBlueprint(
        string Name,
        string Slug,
        string ShortDescription,
        string Description,
        string Purpose,
        string ProblemSolved,
        string NonTechnicalDescription,
        string TechnicalDescription,
        string BusinessImpact,
        string TargetAudience,
        string AccessInstructions,
        string CoverTheme,
        string CategoryCode,
        string StatusCode,
        DevelopmentType DevelopmentType,
        bool IsFeatured,
        bool IsPublished,
        ProjectApprovalStatus ApprovalStatus,
        DateOnly? StartDate,
        DateOnly? EndDate,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int CreatedByUserId,
        int? ReviewedByUserId,
        IReadOnlyList<string> LocationNames,
        IReadOnlyList<string> TechnologyNames,
        IReadOnlyList<string> TeamNames,
        IReadOnlyList<string> TagNames,
        IReadOnlyList<(string Name, string Description, IntegrationType Type)> Integrations,
        IReadOnlyList<(string FullName, string Role)> Members,
        IReadOnlyList<(string Title, string Theme, string Caption)> MediaItems,
        IReadOnlyList<(string Name, string FileName, string Type, string Description)> Documents
    );

    public static IReadOnlyList<ProjectBlueprint> GetProjects() => new List<ProjectBlueprint>
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // 1. Kestirimci Bakım & Titreşim Analizi (IoT)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Ana Konveyör Ekipman Sağlığı ve Kestirimci Bakım Sistemi",
            Slug: "ana-konveyor-ekipman-sagligi-kestirimci-bakim",
            ShortDescription: "Cevher nakil hatlarındaki kritik tahrik grupları ve rulmanların titreşim-sıcaklık verilerini sürekli analiz ederek plansız duruşları önlemeyi amaçlayan kestirimci izleme platformudur.",
            Description: "Madencilik tesislerinde ana bant konveyörleri, cevherin açık ocaktan kırma ve zenginleştirme ünitelerine taşınmasında kesintisiz çalışması gereken en stratejik ekipmanlar arasında yer alır. Tahrik tamburları, redüktörler ve ana rulmanlarda meydana gelen mekanik yorulmalar, erken tespit edilmediğinde tüm tesisin durmasına neden olabilmektedir.\n\nBu proje, konveyör hatlarındaki kritik bileşenlere yerleştirilen yüksek frekanslı piezoelektrik ivmeölçerler ve sıcaklık sensörlerinden toplanan verileri anlık olarak işler. Spektral titreşim analizi (FFT) ve anomali tespit algoritmaları sayesinde mekanik deformasyon başlangıçları operasyonel arızaya dönüşmeden önce bakım ekiplerine uyarı üretilir.\n\nSistem, kestirimci bakım mühendislerinin rulman hasar frekanslarını (BPFO, BPFI, BSF, FTF) otomatik takip etmesine olanak tanırken, periyodik bakım yerine duruma dayalı bakım (CBM) yaklaşımına geçişi destekler.",
            Purpose: "Cevher nakil konveyörlerinde kritik mekanik bileşenlerin sağlık durumunu sürekli izlemek, plansız hat duruşlarını minimize etmek ve bakım faaliyetlerini takvime bağlı periyotlar yerine gerçek aşınma verilerine göre planlamaktır.\n\nSistem; rulman, dişli kutusu ve elektrik motoru arıza paternlerini erkenden yakalayarak bakım ekiplerinin parça temini ve planlı müdahale hazırlıklarını önceden tamamlamasına katkı sağlar.",
            ProblemSolved: "Konveyör sistemlerinde geleneksel olarak yürütülen manuel titreşim ölçümleri ve haftalık gözlemsel kontroller, vardiyalar arasında hızla gelişen rulman çatlaklarını veya yağlama yetersizliklerini yakalamakta yetersiz kalabilmekteydi. Bu durum ani bant kopmalarına, redüktör kilitlenmelerine ve saatler süren üretim kayıplarına yol açmaktaydı.\n\nProje, 7/24 kesintisiz telemetri akışı ve otomatik eşik analizi sağlayarak insan gözlemine dayalı gecikmeleri ortadan kaldırmakta, potansiyel arızaları ilk evrelerinde görünür kılmaktadır.",
            NonTechnicalDescription: "Tesisimizdeki ana taşıyıcı bantların motor ve dişli kutularına takılan akıllı sensörler, ekipmanın çalışırken çıkardığı titreşimleri ve ısınmayı sürekli dinleyen birer dijital doktor gibi görev yapar.\n\nBir rulmanda aşınma veya dengesizlik başladığında, sistem ekipman tamamen durmadan günler önce bakım sorumlularına bilgilendirme iletir. Böylece bakım ekipleri üretimi aksatmadan, planlı duruş saatlerinde gerekli parçayı değiştirir.",
            TechnicalDescription: "Sistem mimarisi, uç nokta sensör katmanından başlayarak merkezi analitik ve görselleştirme katmanına kadar uzanan dağıtık bir yapıya sahiptir. Konveyör tahrik istasyonlarına yerleştirilen endüstriyel titreşim sensörleri, ham zaman serisi verilerini saha kontrol panosundaki IoT Edge cihazına iletir.\n\nIoT Edge ünitesi üzerinde koşan konteynerize veri işleme servisi, saniyede 10 kHz örnekleme frekansıyla toplanan titreşim sinyallerine Hanning pencereleme ve Hızlı Fourier Dönüşümü (FFT) uygulayarak tepe değerleri (RMS, Kurtosis, Crest Factor) hesaplar. Elde edilen özet özellik vektörleri MQTT protokolü üzerinden merkezi InfluxDB zaman serisi veritabanına aktarılır.\n\nArka uçta ASP.NET Core 9 mikroservisi, gelen telemetri verilerini PyTorch tabanlı bir oto-enkoder (autoencoder) anomali modeliyle skorlar. Sapma tespit edildiğinde Redis pub/sub üzerinden WebSocket kanalıyla React tabanlı kullanıcı arayüzüne anlık bildirim fırlatılır. Tüm tarihsel trendler ve spektral şelale grafikleri WebGL destekli görselleştirme bileşenleri ile istemci tarafında render edilir.",
            BusinessImpact: "Kritik nakil hatlarında plansız mekanik duruş riskinin erkenden fark edilmesini sağlar. Yedek parça tedarik süreçlerinin acil sipariş yerine planlı satın alma ile yürütülmesine katkıda bulunur, bakım iş emirlerinin hedefe yönelik açılmasını destekler.",
            TargetAudience: "Kestirimci Bakım Mühendisleri, Mekanik Bakım Şefleri, Tesis Operasyon Sorumluları ve Güvenilirlik Ekipleri.",
            AccessInstructions: "Proje Kütüphanesi arayüzü üzerinden proje detaylarına erişilebilir. Canlı telemetri kokpiti için kurumsal Active Directory hesabı ile Kestirimci Bakım Portalı'na giriş yapılmalıdır.",
            CoverTheme: "predictive-maintenance",
            CategoryCode: "IOT",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 4, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 3, 20, 14, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 3, 20, 14, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Sivas Kangal Zenginleştirme Tesisi" },
            TechnologyNames: new[] { "Python / PyTorch", "MQTT / IoT Edge", "ASP.NET Core", "InfluxDB", "React", "Typescript" },
            TeamNames: new[] { "Kestirimci Bakım ve Güvenilirlik Ekibi", "IoT ve Otomasyon Ekibi", "Veri Analitiği Ekibi" },
            TagNames: new[] { "Kestirimci Bakım", "Yapay Zeka", "IoT Sensör", "Telemetri", "Endüstri 4.0" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Saha IoT Gateway MQTT Broker", "Saha titreşim sensörlerinden gelen telemetri paketlerinin anlık tüketimi", IntegrationType.MessageQueue),
                ("Kurumsal SAP PM Modülü", "Eşik aşımında otomatik arıza bildirim kaydı ve iş emri taslağı oluşturma", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Murat Koç", "Proje Yöneticisi & Kestirimci Bakım Lideri"),
                ("Mehmet Demir", "IoT & Donanım Entegrasyon Mühendisi"),
                ("Ayşe Kaya", "Makine Öğrenmesi & Anomali Modelleme Uzmanı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Sistem Mimari Dokümanı", "Kestirimci_Bakim_Mimari_v2.pdf", "Architecture", "Uçtan uca veri akışı ve FFT filtre parametreleri"),
                ("Sensör Kalibrasyon Kılavuzu", "Sensor_Kalibrasyon_Rehberi.pdf", "UserManual", "Piezoelektrik sensör montaj ve sinyal doğrulama adımları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. Cevher Kalite & Harmanlama Analizi (Madencilik Teknolojileri)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Cevher Harmanlama ve Kalite Analiz Karar Destek Platformu",
            Slug: "cevher-harmanlama-kalite-analiz-platformu",
            ShortDescription: "Açık ocak kademelerinden çıkarılan farklı tenördeki cevherlerin kırma ve besleme öncesinde optimum oranlarda harmanlanmasını sağlayan veri odaklı karar destek yazılımıdır.",
            Description: "Madencilik işletmelerinde zenginleştirme tesisine beslenen tüvenan cevherin tenör ve empürite dalgalanmaları, flotasyon verimini ve reaktif tüketimini doğrudan etkiler. Farklı ocak aynalarından gelen cevherlerin doğru oranlarda harmanlanması, konsantre kalitesinin hedeflenen bantta tutulması için kritik bir zorunluluktur.\n\nBu platform; jeolojik blok modelleri, kuyu karot analizleri, saha XRF ölçümleri ve stok sahası hacim verilerini tek çatı altında toplar. Doğrusal programlama ve kısıt optimizasyon algoritmaları çalıştırarak tesisin günlük hedef tenörünü karşılayacak en uygun harmanlama reçetesini oluşturur.\n\nPlatform üzerinden maden mühendisleri ve tesis metalurjistleri, stok hareketlerini geriye dönük izleyebilir, ayna besleme simülasyonları yapabilir ve günlük besleme planlarını dijital olarak onaylayabilir.",
            Purpose: "Ocak üretim planları ile zenginleştirme tesisi besleme hedefleri arasındaki tenör uyumunu maksimize etmek, ani tenör düşüşlerinin tesis verimine olumsuz etkilerini önceden simüle ederek dengelemektir.\n\nSaha mühendislerine stok seviyeleri ve ayna analizlerine dayalı matematiksel olarak doğrulanmış günlük besleme senaryoları sunmayı amaçlar.",
            ProblemSolved: "Daha önce harmanlama kararları, vardiya amirlerinin tecrübesine ve Excel üzerinde manuel yürütülen statik hesaplamalara dayanmaktaydı. Ocakta yaşanan anlık ayna değişimleri veya geciken laboratuvar sonuçları nedeniyle tesis beslemesinde beklenmedik tenör dalgalanmaları oluşabilmekteydi.\n\nProje, laboratuvar bilgi yönetim sistemi (LIMS) ve kantar kayıtlarıyla entegre çalışarak harman reçetelerinin güncel analizlerle otomatik güncellenmesini sağlar, insana bağlı hesaplama hatalarını ortadan kaldırır.",
            NonTechnicalDescription: "Maden sahamızın farklı noktalarından çıkan cevherlerin zenginlik oranları birbirinden farklıdır. Fabrikamızın en verimli şekilde çalışabilmesi için bu cevherlerin belirli ölçülerde birbirine karıştırılması gerekir.\n\nBu yazılım, hangi ocaktan ve hangi stok yığınından kaç kamyon malzeme alınması gerektiğini hesaplayarak mühendislere en ideal karışım tarifini verir.",
            TechnicalDescription: "Platform, ASP.NET Core Web API arka ucu ve React/TypeScript kullanıcı arayüzü ile geliştirilmiş çok katmanlı bir kurumsal karar destek mimarisidir. Optimizasyon motoru, Python Scikit-Learn ve SciPy kütüphaneleri kullanılarak geliştirilmiş bir karma tamsayılı doğrusal programlama (MILP) servisi olarak çalışır.\n\nİlişkisel veritabanı olarak SQL Server kullanılır; cevher blok modelleri, kuyu numune kimyasal bileşimleri (%Fe, %SiO2, %S, %Al2O3) ve stok sahası poligon geometrileri spatial tiplerle saklanır. Power BI Embedded entegrasyonu sayesinde harman kalite trendleri ve kümülatif tenör dağılımları interaktif gösterge panolarında sunulur.\n\nSaha XRF el analizörlerinden Bluetooth/REST üzerinden aktarılan hızlı numune sonuçları anında veritabanına işlenir ve harman simülatöründe gerçek zamanlı katsayı güncellemesi tetiklenir.",
            BusinessImpact: "Zenginleştirme tesisine beslenen cevher tenörünün kararlı bir bantta kalmasına destek verir. Reaktif ve enerji kullanımının dengelenmesine katkı sağlar, tesis metalurjistlerinin karar süreçlerini hızlandırır.",
            TargetAudience: "Maden Planlama Mühendisleri, Tesis Metalurjistleri, Kalite Kontrol Şefleri ve Stok Sahası Sorumluları.",
            AccessInstructions: "Kurumsal ağ üzerinden web tarayıcısı ile doğrudan erişilebilir. Veri girişi ve reçete onaylama yetkileri kullanıcı rolüne göre Active Directory üzerinden yönetilir.",
            CoverTheme: "laboratory-analysis",
            CategoryCode: "MINING_TECHNOLOGY",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 3, 15),
            EndDate: null,
            CreatedAt: new DateTime(2026, 3, 18, 11, 30, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 3, 18, 11, 30, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Divriği Demir Sahası", "Malatya Hekimhan Peletleme Tesisi" },
            TechnologyNames: new[] { "ASP.NET Core", "SQL Server", "Power BI", "Scikit-Learn", "React", "Typescript" },
            TeamNames: new[] { "Veri Analitiği Ekibi", "Süreç Otomasyonu ve Robotik Ekibi", "Yazılım Geliştirme Ekibi" },
            TagNames: new[] { "Cevher Kalite", "Veri Analitiği", "Üretim Takibi", "Otomasyon" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Laboratuvar Bilgi Yönetim Sistemi (LIMS)", "Kimyasal analiz sonuçlarının otomatik aktarımı", IntegrationType.RestApi),
                ("Kantar Otomasyon Veritabanı", "Stok giriş-çıkış kamyon tonajlarının anlık senkronizasyonu", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Ayşe Kaya", "Veri Analitiği & Algoritma Lideri"),
                ("Hakan Yavuz", "Kimyasal Analiz & Kalite Danışmanı"),
                ("Ahmet Yılmaz", "Kıdemli Yazılım Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Harmanlama Optimizasyon Modeli Teknik Dokümanı", "Harman_Model_Teknik_Rapor.pdf", "TechnicalSpec", "MILP formülasyonu ve kısıt matrisleri")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 3. İSG Yapay Zeka Kamera Güvenlik Sistemi (Yapay Zeka)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Açık Ocak ve Kırma Tesisleri Yapay Zeka Tabanlı İSG Kamera Güvenlik Sistemi",
            Slug: "acik-ocak-yapay-zeka-isg-kamera-guvenlik",
            ShortDescription: "Tesis içi kör noktalar ve kırma eleme ünitelerinde KKD ihlalleri ile tehlikeli bölge girişlerini video analitiği ile tespit eden yapay zeka destekli güvenlik sistemidir.",
            Description: "Ağır sanayi ve madencilik sahalarında iş kazalarının önlenmesi, kural ihlallerinin hızla fark edilmesi ve tehlikeli alanlara yetkisiz yaya girişlerinin engellenmesi ile doğrudan ilişkilidir. Geniş açık ocak sahaları ve gürültülü kırma tesislerinde saha denetçilerinin tüm kör noktaları 7/24 fiziki olarak gözetlemesi mümkün olmamaktadır.\n\nBu sistem, mevcut endüstriyel IP kameralardan alınan video akışlarını uç bilişim (Edge AI) sunucularında gerçek zamanlı analiz eder. Baret, reflektif yelek ve güvenlik gözlüğü gibi kişisel koruyucu donanım (KKD) takibinin yanı sıra, iş makinesi manevra yarıçapı içine giren yayaları ve yasaklı tehlike hatlarını (geofence) tespit eder.\n\nKural ihlali oluştuğunda kontrol odasındaki operatör ekranında sesli ve görsel ikaz oluşturulur, ilgili saha formenine mobil bildirim iletilir ve olayın kısa video kesiti otomatik olarak İSG olay kütüphanesine kaydedilir.",
            Purpose: "Saha çalışanlarının kişisel koruyucu donanım kullanım uyumunu desteklemek, hareketli iş makineleri ile yayalar arasındaki tehlikeli yakınlaşmaları anında tespit ederek olası kazaları önlemektir.\n\nİSG ekiplerine cezalandırıcı değil, önleyici ve eğitim odaklı somut görsel risk haritaları sağlamayı amaçlar.",
            ProblemSolved: "Klasik güvenlik kameraları yalnızca kaza veya olay sonrasında geriye dönük kayıt izleme amacıyla kullanılabilmekte, anlık tehlike önleme işlevini yerine getirememekteydi. Manuel kamera izleme ise operatör dikkat dağınıklığı nedeniyle kritik anların kaçırılmasına yol açmaktaydı.\n\nProje, derin öğrenme tabanlı nesne algılama modelleriyle video analizini otomatikleştirerek saniyeler içinde uyarı üretir, riskli durumların gözden kaçmasını engeller.",
            NonTechnicalDescription: "Sahamızdaki güvenlik kameralarına eklenen yapay zeka gözü, çalışanlarımızın baret ve yeleklerini takıp takmadığını ve iş makinelerine tehlikeli şekilde yaklaşıp yaklaşmadığını sürekli kontrol eder.\n\nBir çalışanımız kırma makinesinin tehlikeli bölgesine girdiğinde sistem anında kontrol odasını uyararak olası tehlikelerin büyümeden önlenmesine yardımcı olur.",
            TechnicalDescription: "Mimari, RTSP akışlarını toplayan bir video dağıtım katmanı, GPU hızlandırıcılı derin öğrenme çıkarım motoru ve olay yönetim mikroservislerinden meydana gelir. IP kameralardan gelen H.264/H.265 video akışları, NVIDIA TensorRT ile optimize edilmiş YOLOv8 tabanlı nesne algılama hattına yönlendirilir.\n\nModel; insan, baret, reflektif yelek, ağır iş makinesi ve konveyör hattı sınıflarını eşzamanlı tanır. Çıkarım hattında tespit edilen nesneler OpenCV tabanlı ByteTrack algoritması ile kareler boyunca izlenir; önceden tanımlanmış poligon tehlike bölgeleriyle uzamsal çakışma (IoU) hesaplanır.\n\nİhlal tetiklendiğinde olay metaverisi JSON formatında RabbitMQ kuyruğuna yazılır, Docker üzerinde çalışan olay işleme servisi tarafından işlenerek ASP.NET Core arka ucuna ve React tabanlı canlı izleme konsoluna SignalR ile gönderilir. Olay anına ait 10 saniyelik video klibi depolama sunucusuna arşivlenir.",
            BusinessImpact: "Açık ocak ve kırma tesislerinde İSG kurallarına uyum görünürlüğünü artırır. Güvenlik denetçilerinin kör noktalara yönelik saha aksiyonlarını hızlandırır, çalışan eğitimleri için gerçek olay kayıtları sağlar.",
            TargetAudience: "İSG Uzmanları, Tesis Emniyet Şefleri, Saha Formenleri ve Tesis Kontrol Odası Operatörleri.",
            AccessInstructions: "Kontrol odası canlı alarm paneli üzerinden operatör ekranından takip edilir. İSG yönetim raporlama arayüzüne İSG ve Yönetim rolleri erişebilir.",
            CoverTheme: "safety-monitoring",
            CategoryCode: "ARTIFICIAL_INTELLIGENCE",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Hybrid,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 5, 10),
            EndDate: null,
            CreatedAt: new DateTime(2026, 3, 15, 9, 15, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 3, 15, 9, 15, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Divriği Demir Sahası" },
            TechnologyNames: new[] { "Python / PyTorch", "OpenCV", "Docker / Kubernetes", "ASP.NET Core", "React", "Typescript" },
            TeamNames: new[] { "Yapay Zeka ve Görüntü İşleme Ekibi", "İSG Dijital Takip Ekibi" },
            TagNames: new[] { "İş Güvenliği", "Görüntü İşleme", "Yapay Zeka", "Otomasyon" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Saha CCTV VMS Sunucusu", "RTSP kamera akışlarının merkezi video yönetim sunucusundan çekilmesi", IntegrationType.ExternalService),
                ("Kurumsal SMS / E-Posta Bildirim Servisi", "Yüksek öncelikli alan ihlallerinde saha şeflerine anlık SMS iletimi", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Zeynep Şahin", "Yapay Zeka & Görüntü İşleme Lideri"),
                ("Elif Demir", "İSG Dijital Çözüm Uzmanı"),
                ("Ahmet Yılmaz", "Sistem Entegrasyon Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Kamera Konumlandırma ve Alan Tanımlama Rehberi", "ISG_Kamera_Kurulum_Klavuzu.pdf", "InstallationGuide", "Kamera açıları ve sanal güvenlik bölgesi çizim kuralları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 4. Ağır Maden Araçları Telemetri & Filo Yönetimi (IoT)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Ağır Maden Araçları Telemetri ve Dinamik Filo Yönetim Sistemi",
            Slug: "maden-araclari-telemetri-dinamik-filo-yonetimi",
            ShortDescription: "Kaya kamyonları, ekskavatörler ve loderlerin yakıt, hız, motor parametreleri ve devir sürelerini gerçek zamanlı izleyen telemetri platformudur.",
            Description: "Açık ocak madenciliğinde birim maliyetin en önemli kalemlerinden birini dekapaj ve cevher taşıma operasyonları oluşturur. Onlarca ağır kamyon ve yükleyicinin ocak içi rota verimliliği, bekleme süreleri ve motor yüklenme koşulları operasyonel başarının temel belirleyicisidir.\n\nBu proje; iş makinelerinin CAN-bus (J1939) veri hattına bağlanan endüstriyel telemetri cihazları ve yüksek hassasiyetli GNSS modülleri aracılığıyla araçların anlık konumunu, yakıt tüketim oranını, motor sıcaklığını, vites durumunu ve yükleme-boşaltma çevrim sürelerini kaydeder.\n\nToplanan telemetri verileri, saha şeflerinin hangi ekskavatör önünde kamyon kuyruğu oluştuğunu, hangi yolların aşırı yakıt sarfiyatına yol açtığını ve rölantide gereksiz bekleyen araçları tek ekrandan görmesini sağlar.",
            Purpose: "Açık ocak taşıma operasyonlarında döngü sürelerini optimize etmek, aşırı yakıt tüketimine ve rölantide beklemeye neden olan darboğazları belirleyerek filo görünürlüğünü artırmaktır.\n\nOperatör sürüş alışkanlıklarını analiz ederek güvenli ve ekonomik sürüş standartlarının yerleşmesine katkıda bulunur.",
            ProblemSolved: "Önceki çalışma modelinde kamyon sefer sayıları puantörler tarafından manuel defter kayıtlarıyla tutulmakta, yakıt tüketimleri ise vardiya sonu sayaç okumalarıyla kaba olarak hesaplanabilmekteydi. Bu durum ekskavatör-kamyon eşleşmelerindeki anlık dengesizliklerin fark edilmesini engelliyordu.\n\nSistem, otomatik çevrim tespiti ve canlı rota haritası sunarak manuel kayıt ihtiyacını ortadan kaldırır ve vardiya süresince dinamik filo yönlendirmesi yapılabilmesine imkan tanır.",
            NonTechnicalDescription: "Ocakta çalışan dev kaya kamyonları ve kepçelerin içine yerleştirilen akıllı cihazlar, araçların nerede olduğunu, ne kadar yakıt yaktığını ve kaç sefer yaptığını anlık olarak merkeze aktarır.\n\nSaha yöneticileri, hangi kamyonun nerede beklediğini harita üzerinden canlı izleyerek iş akışını anında düzenleyebilir.",
            TechnicalDescription: "Sistem; araç üstü donanım katmanı, kablosuz iletişim altyapısı, telemetri yutma (ingestion) servisi ve harita tabanlı kullanıcı arayüzünden oluşur. Araç içine monte edilen sağlamlaştırılmış (rugged) telemetri kutusu, J1939 protokolü ile ECU motor parametrelerini okur ve konum bilgisiyle birleştirir.\n\nMaden çanağı içindeki özel Wi-Fi / LoRaWAN baz istasyonları üzerinden merkezi MQTT broker'a iletilen paketler, yüksek hacimli yazma operasyonları için InfluxDB zaman serisi motorunda saklanır. Güncel araç durumları ve son koordinatlar ise hızlı sorgulama için Redis bellek veritabanında tutulur.\n\nArka uç ASP.NET Core 9 servisi; coğrafi sınır (geofence) poligonları içine giriş-çıkış zamanlarını hesaplayarak otomatik yükleme, taşıma, döküm ve dönüş evrelerini (cycle detection) belirler. İstemci tarafında Leaflet ve OpenLayers tabanlı harita üzerinde araç ikonları ve hız vektörleri canlı olarak güncellenir.",
            BusinessImpact: "Açık ocak taşıma döngülerindeki bekleme ve rölanti sürelerinin görünür kılınmasını sağlar. Saha şeflerinin ekskavatör ve kamyon dağılımlarını anlık verilere göre yapmasını destekler, filo kullanım etkinliğini artırır.",
            TargetAudience: "Maden Saha Şefleri, Nakliye ve Filo Formenleri, Ağır Ekipman Bakım Sorumluları ve İşletme Müdürleri.",
            AccessInstructions: "Filo Operasyon Merkezi ekranlarından ve yetkili tabletlerden web tabanlı olarak kullanılır. Kullanıcı bazlı ayna ve lokasyon yetkilendirmesi uygulanır.",
            CoverTheme: "haul-truck",
            CategoryCode: "IOT",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 2, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 3, 10, 16, 45, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 3, 10, 16, 45, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Divriği Demir Sahası", "Kangallı Altın Sahası", "Balıkesir Manyas Sahası" },
            TechnologyNames: new[] { "MQTT / IoT Edge", "LoRaWAN", "ASP.NET Core", "Redis", "React", "Typescript" },
            TeamNames: new[] { "Lojistik ve Filo Optimizasyonu Ekibi", "IoT ve Otomasyon Ekibi", "Saha Teknolojileri ve İletişim Ekibi" },
            TagNames: new[] { "Lojistik ve Filo", "Telemetri", "Saha Yönetimi", "IoT Sensör" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Saha Yakıt İstasyonu Otomasyonu", "Araç bazlı yakıt ikmal miktarlarının CAN-bus telemetrisiyle doğrulanması", IntegrationType.Database),
                ("Kantar Otomasyon Sistemi", "Boş/dolu kantar geçişleri ile sefer eşleşmesi", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Onur Şimşek", "Filo Operasyon & Telemetri Uzmanı"),
                ("Mustafa Çelik", "Saha İletişim & Ağ Mühendisi"),
                ("Ahmet Yılmaz", "Arka Uç Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("CAN-bus Telemetri Protokol Kılavuzu", "Filo_Telemetri_Entegrasyon.pdf", "TechnicalSpec", "J1939 mesaj haritası ve CAN frame formatları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 5. Tesis Enerji Tüketimi ve Güç Kalitesi İzleme (Veri Analitiği)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Kurumsal Tesis Enerji Tüketimi ve Güç Kalitesi İzleme Portalı",
            Slug: "kurumsal-tesis-enerji-izleme-portali",
            ShortDescription: "Tesislerdeki yüksek gerilim trafoları, değirmenler ve ana panolardan anlık enerji, güç faktörü ve harmonik verilerini toplayan enerji izleme sistemidir.",
            Description: "Zenginleştirme ve peletleme tesislerinde bilyalı değirmenler, kırıcılar ve yüksek güçlü pompalar önemli miktarda elektrik enerjisi tüketir. Enerji tüketiminin birim cevher üretimi başına takip edilmesi, güç faktörünün (cos φ) regülasyon sınırlarında tutulması ve reaktif ceza risklerinin engellenmesi kritik bir operasyonel hedeftir.\n\nBu portal; tesis genelindeki ana dağıtım panolarında, kompanzasyon ünitelerinde ve orta gerilim trafolarında yer alan enerji analizörlerinden Modbus TCP ve OPC UA protokolleri üzerinden veri toplar. Aktif-reaktif güç, akım-gerilim dengesizlikleri, toplam harmonik distorsiyon (THD) ve tüketim trendleri anlık olarak kaydedilir.\n\nSistem, pik tüketim saatlerinde yük kaydırma önerileri sunar, kompanzasyon panosu kademe arızalarını tespit eder ve tesis yöneticilerine vardiya bazlı spesifik enerji tüketim göstergeleri sağlar.",
            Purpose: "Tesis bazında elektrik enerjisi tüketimini ve güç kalitesi parametrelerini şeffaf biçimde izlemek, reaktif güç cezası risklerini ortadan kaldırmak ve birim ton başına enerji sarfiyatını düşürmeye yönelik aksiyonları desteklemektir.\n\nEnerji yöneticilerine anlık aşırı yüklenme ve harmonik bozulmalarda erken ikaz mekanizması sağlamayı hedefler.",
            ProblemSolved: "Farklı binalara ve sahalara dağılmış yüzlerce elektrik sayacının manuel olarak okunması zaman almakta, kompanzasyon panolarındaki kondansatör arızaları ancak ay sonu elektrik faturalarında reaktif ceza görüldüğünde fark edilebilmekteydi.\n\nProje, merkezi otomatik veri toplama altyapısı kurarak güç kalitesi anomalilerini anında raporlar ve reaktif sınır aşımlarını gerçekleştiği anda alarm olarak iletir.",
            NonTechnicalDescription: "Fabrikamızdaki dev değirmenlerin ve elektrik panolarının ne kadar elektrik harcadığını ve elektriği ne kadar temiz kullandığını 24 saat kaydeden bir sistemdir.\n\nElektrik şebekesinde bir dengesizlik olduğunda veya gereksiz yere enerji harcandığında sorumluları uyararak ceza ödenmesini ve makinelerin zarar görmesini engeller.",
            TechnicalDescription: "Mimari, endüstriyel haberleşme katmanı, zaman serisi veri ambarı ve analitik gösterge paneli bileşenlerinden oluşur. Sahadaki Siemens, Schneider ve Janitza enerji analizörleri, endüstriyel Ethernet ağı üzerinden OPC UA sunucusuna ve Modbus TCP gateway cihazlarına bağlanır.\n\nASP.NET Core tabanlı veri toplama servisi, yapılandırılmış periyotlarla (1 saniye anlık, 15 dakika uzlaşma) sayaç kayıtlarını çeker ve InfluxDB veritabanına yazar. Reaktif oran (% indüktif, % kapasitif) hesaplamaları bellek içinde mikroservis katmanında yürütülür ve yasal eşik limitleri aşıldığında anında bildirim üretilir.\n\nKullanıcı arayüzü React ve TypeScript ile inşa edilmiştir; Power BI Embedded bileşenleri sayesinde geçmişe dönük spesifik tüketim (kWh/ton) korelasyonları üretim tonajı ile birleştirilerek sunulur.",
            BusinessImpact: "Kompanzasyon arızalarından kaynaklanabilecek reaktif ceza risklerini önlemeye yardımcı olur. Tesis enerji yöneticilerinin yüksek tüketimli ekipmanları ve enerji optimizasyonu fırsatlarını kolayca tespit etmesini sağlar.",
            TargetAudience: "Enerji Yöneticileri, Tesis Elektrik Bakım Şefleri, Fabrika Müdürleri ve Sürdürülebilirlik Komitesi.",
            AccessInstructions: "Kurumsal intranet üzerinden Enerji Portalı linkinden erişilir. E-posta ve SMS uyarı listeleri kullanıcı profilinden yapılandırılabilir.",
            CoverTheme: "energy-monitoring",
            CategoryCode: "DATA_ANALYTICS",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 1, 10),
            EndDate: null,
            CreatedAt: new DateTime(2026, 3, 5, 10, 20, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 3, 5, 10, 20, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Sivas Kangal Zenginleştirme Tesisi", "Malatya Hekimhan Peletleme Tesisi", "Genel Müdürlük (Ankara)" },
            TechnologyNames: new[] { "OPC UA", "InfluxDB", "ASP.NET Core", "Power BI", "React", "Typescript" },
            TeamNames: new[] { "Çevre ve Enerji Yönetimi Ekibi", "IoT ve Otomasyon Ekibi", "Kurumsal Uygulamalar ve ERP Ekibi" },
            TagNames: new[] { "Enerji Verimliliği", "Enerji Optimizasyonu", "Çevre ve Sürdürülebilirlik", "Endüstri 4.0" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Tesis SCADA Enerji Dağıtım Panoları", "OPC UA üzerinden trafo ve jeneratör telemetrisi", IntegrationType.ExternalService),
                ("ERP Maliyet Muhasebesi Modülü", "Aylık enerji tüketim tutarlarının maliyet merkezlerine dağıtımı", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Ebru Çelik", "Çevre & Enerji Yönetimi Lideri"),
                ("Can Özkan", "Otomasyon & PLC Uzmanı"),
                ("Gizem Keskin", "İş Zekası & Power BI Geliştirici")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Enerji İzleme Sistemi Altyapı Şeması", "Enerji_Izleme_Altyapi_v1.pdf", "Architecture", "Trafo merkezleri sayaç haberleşme topolojisi")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 6. Su Rejimi ve Çevresel İzleme Telemetri Ağı (IoT)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Yeraltı ve Açık Ocak Su Rejimi ve Çevresel İzleme Telemetri Ağı",
            Slug: "su-rejimi-cevresel-izleme-telemetri-agi",
            ShortDescription: "Maden sahalarındaki piezometre kuyuları, drenaj kanalları ve deşarj noktalarında su seviyesi, debi ve pH parametrelerini takip eden kablosuz telemetri ağıdır.",
            Description: "Madencilik faaliyetlerinde yeraltı su seviyelerinin kontrolü, şev stabilitesinin korunması ve çevre mevzuatına tam uyumlu deşarj yönetimi için hayati öneme sahiptir. Geniş açık ocak etrafında ve atık barajı çevresinde yer alan onlarca gözlem kuyusunun düzenli izlenmesi gerekir.\n\nBu proje; güneş enerjili ve pilli LoRaWAN uç düğümleri ile donatılmış hidrostatik seviye sensörleri, ultrasonik debimetreler, pH ve iletkenlik problarından veri toplar. Kilometrelerce alana yayılan sensörlerden gelen ölçümler merkezi istasyonda birleştirilerek harita üzerinde görselleştirilir.\n\nSistem, yeraltı su tablasındaki ani yükselmeleri ve mevsimsel drenaj değişikliklerini analiz ederek şev kayması risklerine karşı erken uyarı üretir, arıtma ve deşarj noktalarında su kalitesi parametrelerinin yasal limitler içinde kalmasını sağlar.",
            Purpose: "Açık ocak çevresi ve atık depolama tesislerindeki su seviyeleri ile çevresel deşarj kalitesini uzaktan, sürekli ve güvenilir biçimde izlemek; jeoteknik riskleri erkenden tespit etmektir.\n\nÇevre ve saha mühendislerine mevzuata uygunluk raporlamalarında kullanılacak kesintisiz tarihsel veri tabanı sunmayı amaçlar.",
            ProblemSolved: "Zorlu kış şartlarında ve engebeli arazi yapısında gözlem kuyularına personelin fiziki olarak giderek manuel iskandil sarkıtması iş güvenliği riski taşımakta ve veri sıklığını haftalık/aylık düzeyde sınırlamaktaydı.\n\nSistem, LoRaWAN kablosuz teknolojisiyle saatlik otomatik ölçüm alarak zorlu hava koşullarından bağımsız kesintisiz veri akışı sağlar ve personel riskini ortadan kaldırır.",
            NonTechnicalDescription: "Maden sahamızın etrafındaki su kuyularına ve dere yataklarına yerleştirilen kablosuz sensörler, yeraltı suyu seviyesini ve temizliğini saat başı ölçerek merkeze bildirir.\n\nToprak altındaki su seviyesi tehlikeli şekilde yükseldiğinde veya su kalitesinde bir değişiklik olduğunda mühendislerimiz arazide dolaşmaya gerek kalmadan bilgisayar ekranından durumu görür.",
            TechnicalDescription: "Sistem, düşük güç tüketimli geniş alan ağı (LoRaWAN) topolojisi üzerine kuruludur. Sahadaki piezometre kuyularına indirilen 4-20mA çıkışlı paslanmaz gövdeli hidrostatik basınç transmiterleri ve analog/dijital çevirici LoRaWAN düğümleri (node), SX1262 alıcı-verici yongalarıyla 868 MHz bandında yayın yapar.\n\nMaden sahasının hakim tepelerine yerleştirilen güneş panelli endüstriyel LoRaWAN ağ geçitleri (gateway), gelen paketleri 4G/LTE hücresel modem üzerinden MQTT formatında merkezi ChirpStack LoRaWAN sunucusuna aktarır.\n\nVeri akışı, ASP.NET Core 9 arka uç servisi tarafından consume edilir; sensör kalibrasyon katsayıları ve su tablası kot formülleri uygulanarak InfluxDB zaman serisi veritabanına yazılır. Kullanıcı arayüzünde OpenLayers kütüphanesi ile maden haritası üzerinde kuyu katmanları, su seviyesi izohips eğrileri ve trend grafikleri çizdirilir.",
            BusinessImpact: "Maden şev güvenliğini destekleyen yeraltı su verilerinin kesintisiz toplanmasını sağlar. Çevre mevzuatı izleme ve raporlama süreçlerini otomatikleştirir, saha ekiplerinin ölçüm için araziye çıkış sıklığını ve iş sağlığı risklerini azaltır.",
            TargetAudience: "Çevre Mühendisleri, Hidrojeoloji Uzmanları, Jeoteknik Ekipler ve Maden Emniyet Şefleri.",
            AccessInstructions: "Çevre ve Su Yönetim Portalı üzerinden intranet üzerinden erişilir. Harita katmanları ve kuyu detayları kullanıcı yetkisine göre görüntülenir.",
            CoverTheme: "environmental-monitoring",
            CategoryCode: "IOT",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 11, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 2, 28, 14, 10, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 2, 28, 14, 10, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Eskişehir Mihalıççık Sahası" },
            TechnologyNames: new[] { "LoRaWAN", "InfluxDB", "ASP.NET Core", "React", "Typescript" },
            TeamNames: new[] { "Çevre ve Enerji Yönetimi Ekibi", "Saha Teknolojileri ve İletişim Ekibi" },
            TagNames: new[] { "Çevre ve Sürdürülebilirlik", "IoT Sensör", "Telemetri", "Saha Yönetimi" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("ChirpStack LoRaWAN Network Server", "Uç sensör düğümlerinden gelen şifreli radyo paketlerinin çözümlenmesi", IntegrationType.RestApi),
                ("CBS Jeoloji Veritabanı", "Kuyu koordinatları ve jeolojik stratigrafi katmanlarının senkronizasyonu", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Ebru Çelik", "Çevre & Sürdürülebilirlik Mühendisi"),
                ("Emre Polat", "Telemetri & Kablosuz Ağ Uzmanı"),
                ("Burak Aydın", "Hidrojeoloji ve CBS Danışmanı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Su İzleme Ağı Kurulum ve Kalibrasyon Prosedürü", "Su_Izleme_Sensor_Proseduru.pdf", "Manual", "Kuyu seviye sensörü sıfırlama ve bakım yönergesi")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 7. 3B Jeolojik Modelleme & Maden Planlama (Madencilik Teknolojileri)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "3 Boyutlu Jeolojik Modelleme ve Sayısal Maden Planlama Entegrasyonu",
            Slug: "3b-jeolojik-modelleme-sayisal-maden-planlama",
            ShortDescription: "Sondaj verileri, litoloji katmanları ve yapısal jeoloji haritalarını 3 boyutlu voksel modellerine dönüştürerek rezerv tahminlerini güncelleyen CBS platformudur.",
            Description: "Maden kaynak ve rezervlerinin doğru modellenmesi, uzun vadeli ocak tasarımından günlük üretim planlamasına kadar tüm madencilik yatırımlarının temelini oluşturur. Sahadan gelen yeni karot analizleri ve yapısal jeoloji ölçümlerinin mevcut modellere hızlıca entegre edilmesi gerekir.\n\nBu proje; arama ve geliştirme sondajlarından elde edilen tahlil sonuçlarını, jeofizik logları ve fay düzlemi ölçümlerini 3 boyutlu uzayda birleştirir. Jeoistatistiksel yöntemler (Ordinary Kriging, Inverse Distance Weighting) kullanarak tenör dağılımını blok modelleri üzerinde hesaplar ve rezerv kestirimlerini günceller.\n\nPlatform; maden planlama mühendislerinin optimum nihai ocak sınırlarını belirlemesine, dekapaj oranlarını (stripping ratio) analiz etmesine ve farklı tenör kesme (cut-off grade) stratejilerini simüle etmesine olanak tanır.",
            Purpose: "Sondaj ve ayna jeolojisi verilerini hızlıca sayısal blok modellerine aktarmak, rezerv kestirim hassasiyetini artırmak ve ocak üretim tasarım süreçlerini desteklemektir.\n\nJeoloji ve maden planlama ekipleri arasında ortak veri standardı ve üç boyutlu görsel doğrulama ortamı oluşturmayı hedefler.",
            ProblemSolved: "Farklı yazılımlar ve dosya formatları arasında veri aktarımı sırasında yaşanan kayıplar, jeoloji modellerinin güncellenmesini aylar süren hantal bir sürece dönüştürmekteydi. Güncel olmayan modellerle yapılan üretim planları ise ocakta beklenmeyen tenör sapmalarına yol açabiliyordu.\n\nProje, PostgreSQL/PostGIS tabanlı merkezi jeolojik veri ambarı ve otomatik enterpolasyon boru hatları kurarak model güncelleme süresini haftalardan günlere indirir.",
            NonTechnicalDescription: "Maden arama kuyularından alınan kayaç örneklerinin tahlil sonuçları bu sistemde birleştirilerek yer altındaki maden damarlarının 3 boyutlu renkli haritası çıkarılır.\n\nBu sayede mühendislerimiz yerin altını kazmadan önce nerede ne kadar kaliteli maden olduğunu bilgisayar ortamında şeffaf bir şekilde görebilir.",
            TechnicalDescription: "Platform; PostgreSQL/PostGIS uzamsal veritabanı, Python tabanlı jeoistatistiksel hesaplama motoru ve WebGL destekli üç boyutlu tarayıcı görüntüleyicisinden oluşur. Sondaj kuyu yörüngeleri, litoloji aralıkları ve tahlil segmentleri (assay intervals) OGC standartlarına uygun 3D spatial tablolarda indekslenir.\n\nJeoistatistik analiz hattı; Python üzerinde GSTools, PyVista ve SciPy kütüphaneleri kullanılarak geliştirilmiştir. Kullanıcı arayüzünden seçilen parametrelerle yarı-variogram modellemesi (küresel, üstel, Gauss) yapılır ve 10x10x5 metrelik blok matrislerine tenör kestirimi atanır.\n\nWeb arayüzünde Three.js tabanlı özel 3D renderer ile milyonlarca blok, izo-yüzey (isosurface) ve fay kırıkları 60 FPS hızında taranabilir; katmanlar kesit düzlemleri (cross-section) ile dinamik olarak dilimlenebilir.",
            BusinessImpact: "Rezerv tahmin doğruluğunu destekler, ocak planlama ekiplerinin güncel jeolojik verilere anında erişmesini sağlar. Arama sondajı lokasyonlarının hedefe yönelik planlanmasına katkıda bulunur.",
            TargetAudience: "Arama ve Maden Jeologları, Maden Planlama Mühendisleri ve Jeoistatistik Uzmanları.",
            AccessInstructions: "CBS ve Jeoloji Portalı üzerinden kurumsal yetkilendirme ile masaüstü tarayıcılardan erişilir. 3B görselleştirme için WebGL 2.0 destekli modern tarayıcı gereklidir.",
            CoverTheme: "mapping-gis",
            CategoryCode: "MINING_TECHNOLOGY",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Hybrid,
            IsFeatured: true,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 8, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 2, 22, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 2, 22, 9, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Genel Müdürlük (Ankara)", "Divriği Demir Sahası", "Kangallı Altın Sahası" },
            TechnologyNames: new[] { "PostgreSQL", "Python / PyTorch", "ASP.NET Core", "React", "Typescript" },
            TeamNames: new[] { "CBS ve Jeolojik Modelleme Ekibi", "Yazılım Geliştirme Ekibi" },
            TagNames: new[] { "Jeolojik Modelleme", "CBS ve Haritalama", "Veri Analitiği", "Yapay Zeka" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Maden Planlama Yazılımı Dosya Arayüzü", "Saha DXF ve CSV blok model dosyalarının içe/dışa aktarımı", IntegrationType.FileTransfer),
                ("LIMS Veritabanı", "Sondaj karot kimyasal analiz sonuçlarının otomatik aktarımı", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Burak Aydın", "Kıdemli Jeoloji & CBS Uzmanı"),
                ("Serkan Öztürk", "Maden Planlama Mühendisi"),
                ("Ahmet Yılmaz", "Veri Tabanı & Sistem Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Jeoistatistiksel Kestirim Metodolojisi", "Jeoloji_Model_Metodoloji_v1.pdf", "Whitepaper", "Kriging parametreleri ve variogram optimizasyon rehberi")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 8. Otonom İHA ile Sayısal Hacim & Stok Takibi (Madencilik Teknolojileri)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Otonom İHA / Drone ile Sayısal Hacim Hesaplama ve Stok Takip Sistemi",
            Slug: "otonom-iha-sayisal-hacim-hesaplama-stok-takip",
            ShortDescription: "İnsansız hava araçlarından alınan yüksek çözünürlüklü fotogrametrik görüntülerle açık ocak dekapaj hacimlerini ve stok yığınlarını hesaplayan haritalama platformudur.",
            Description: "Maden sahalarında ay sonu hakediş ölçümleri, stok sahası envanter denetimleri ve açık ocak kazı-dolgu hacimlerinin hesaplanması klasik topoğrafik yöntemlerle günler sürebilmektedir. Geniş sahaların hızlı ve yüksek doğrulukla haritalanması kritik bir gereksinimdir.\n\nBu sistem; otonom uçuş rotaları izleyen RTK/PPK özellikli İHA'ların topladığı hava fotoğraflarını fotogrametri hattında işleyerek santimetre hassasiyetinde sayısal yüzey modelleri (DSM) ve ortomozaik haritalar üretir. İki uçuş dönemi arasındaki yüzey farkı diferansiyel grid analizi ile taranarak kazılan veya yığılan hacim metreküp cinsinden otomatik çıkarılır.\n\nSistem; stok yığınlarının malzeme yoğunluk katsayıları ile tonaj karşılığını hesaplar, ocak şev açılarının tasarım limitlerine uygunluğunu denetler ve CBS katmanları üzerinden yöneticilere raporlar.",
            Purpose: "Açık ocak kazı ilerlemelerini ve stok sahası malzeme miktarlarını hızlı, güvenli ve yüksek hassasiyetle ölçmek; geleneksel topoğrafya sürelerini dramatik biçimde kısaltmaktır.\n\nMaden operasyonlarına güncel ortofoto haritalar ve şev güvenlik analizleri sağlamayı hedefler.",
            ProblemSolved: "Klasik total station ve GPS jalonu ile dik şev kenarlarında ve hareketli stok yığınları üzerinde ölçüm yapmak topoğraflar için ciddi kayma ve yuvarlanma riskleri barındırmaktaydı. Ayrıca ölçüm noktası sıklığının sınırlı olması nedeniyle hacim hesaplarında hata payı yükselmekteydi.\n\nProje, milyonlarca noktadan oluşan yoğun nokta bulutları (point cloud) üzerinden hesaplama yaparak ölçüm doğruluğunu yükseltir ve insanı tehlikeli araziden uzak tutar.",
            NonTechnicalDescription: "Sahamızın üzerinde otomatik olarak uçan kameralı dronlar, arazinin ve stoktaki maden tepelerinin binlerce fotoğrafını çeker.\n\nBilgisayar sistemi bu fotoğrafları birleştirerek arazinin 3 boyutlu dijital ikizini oluşturur ve stokta kaç ton cevher bulunduğunu dakikalar içinde hesaplar.",
            TechnicalDescription: "Sistem mimarisi; İHA uçuş görev planlama modülü, fotogrametrik nokta bulutu işleme servisi, hacim hesaplama algoritması ve CBS harita sunucusundan oluşur. Drone tarafından kaydedilen jeoreferanslı geotag EXIF verilerine sahip fotoğraflar, SFM (Structure from Motion) algoritmasıyla işlenir.\n\nİşlem hattı; OpenCV ve PDAL kütüphaneleriyle optimize edilmiş yoğun nokta bulutu sınıflandırması yapar (bitki örtüsü, iş makinesi ve zemin ayrıştırma). Elde edilen sayısal yükseklik modelleri (DEM), PostGIS raster eklentisi ile veritabanına yüklenir.\n\nİki DEM katmanı arasındaki diferansiyel hacim entegrali (cut/fill volume integration) paralel CPU çekirdekleri üzerinde hesaplanır. Harita arayüzünde MapLibre GL ile dinamik yükseklik renk profilleri ve kontur eğrileri katman bazında görüntülenir.",
            BusinessImpact: "Ay sonu kübaj ve stok envanter hesaplama süreçlerini hızlandırır. Ölçüm ekiplerinin dik ayna ve yığın kenarlarında ölçüm yapma zorunluluğunu ortadan kaldırarak iş güvenliğine katkı sağlar.",
            TargetAudience: "Harita ve Topoğrafya Mühendisleri, Maden Planlama Şefleri, Muhasebe & Stok Denetçileri.",
            AccessInstructions: "Maden CBS Haritalama Portalı üzerinden yetkili kullanıcı girişiyle kullanılır. Uçuş ham verileri ve ortofotolar proje depolama alanında arşivlenir.",
            CoverTheme: "drone-surveying",
            CategoryCode: "MINING_TECHNOLOGY",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 9, 15),
            EndDate: null,
            CreatedAt: new DateTime(2026, 2, 15, 15, 30, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 2, 15, 15, 30, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Divriği Demir Sahası", "Malatya Hekimhan Peletleme Tesisi", "Kangallı Altın Sahası" },
            TechnologyNames: new[] { "Python / PyTorch", "OpenCV", "PostgreSQL", "ASP.NET Core", "React" },
            TeamNames: new[] { "CBS ve Jeolojik Modelleme Ekibi", "Yapay Zeka ve Görüntü İşleme Ekibi" },
            TagNames: new[] { "Drone ve İHA", "CBS ve Haritalama", "Üretim Takibi", "Veri Analitiği" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Maden CBS Veritabanı", "Ortofoto ve sayısal yükseklik modellerinin merkezi CBS katmanlarına yayını", IntegrationType.Database),
                ("ERP Stok Yönetimi", "Hesaplanan stok tonajlarının malzeme kartları envanter kaydına işlenmesi", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Kerem Vural", "İHA Pilotu & Fotogrametri Uzmanı"),
                ("Burak Aydın", "CBS & Harita Koordinatörü"),
                ("Zeynep Şahin", "Görüntü İşleme Mühendisi")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("İHA Uçuş Emniyeti ve Ölçüm Standartları", "IHA_Ucus_Proseduru_v2.pdf", "Manual", "RTK baz istasyonu kurulum ve hava sahası izin adımları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 9. Zenginleştirme Tesisi SCADA Veri Toplama ve Merkezi Operasyon Kokpiti (Otomasyon)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Zenginleştirme Tesisi SCADA Veri Toplama ve Merkezi Operasyon Kokpiti",
            Slug: "zenginlestirme-tesisi-scada-merkezi-operasyon-kokpiti",
            ShortDescription: "Flotasyon, liç ve manyetik ayırıcı ünitelerindeki PLC ve SCADA verilerini tek ekranda konsolide eden merkezi endüstriyel operasyon panosudur.",
            Description: "Zenginleştirme tesislerinde kırma, öğütme, sınıflandırma, flotasyon ve susuzlaştırma gibi ardışık fazlar karmaşık bir endüstriyel otomasyon ağı tarafından yönetilir. Farklı yıllarda kurulan ünitelerin farklı PLC markaları ve izole SCADA sistemleri kullanması, tesis genelinde bütünsel süreç kontrolünü güçleştirebilmektedir.\n\nBu proje; Siemens S7, Schneider Modicon ve Allen-Bradley kontrolcülerinden standart OPC UA ve Modbus köprüleri üzerinden saniyelik süreç verilerini (debi, basınç, yoğunluk, pH, seviye, motor akımları) çeker. Dağıtık tesis verilerini tek bir merkezi veri katmanında toplar.\n\nOperatörler ve proses metalurjistleri; tesisin anlık malzeme akışını, kritik vana pozisyonlarını ve kümülatif üretim metriklerini gecikmesiz animasyonlu mimik diyagramlar üzerinde izleyebilir, tarihsel süreç trendlerini karşılaştırabilir.",
            Purpose: "Farklı otomasyon adacıklarına bölünmüş tesis süreç verilerini tek bir merkezde toplamak, kontrol odası operatörleri ve proses mühendisleri için anlık bütünsel süreç görünürlüğü sağlamaktır.\n\nSüreç sapmalarında erken müdahale imkanı sunarak ürün kalitesinde standardizasyonu desteklemeyi amaçlar.",
            ProblemSolved: "Daha önce farklı tesis binalarındaki izole operatör panelleri (HMI) nedeniyle bir ünitedeki tıkanıklık veya debi düşüşü diğer üniteler tarafından ancak süreç bozulduktan sonra fark edilebilmekteydi. Tarihsel verilerin kaydedilmemesi kök neden analizlerini zorlaştırıyordu.\n\nSistem, tesis genelini kapsayan birleşik bir kokpit kurarak üniteler arası senkronizasyonu sağlar ve tüm süreç değişkenlerini yüksek hızda arşivler.",
            NonTechnicalDescription: "Fabrikamızın farklı bölümlerindeki makinelerin, pompaların ve vanaların durumunu dev kontrol odasındaki tek bir ekranda canlı olarak gösteren dijital kumanda panelidir.\n\nBir boruda basınç düştüğünde veya bir tank taştığında sistem operatörleri anında uyararak üretimin aksamasını engeller.",
            TechnicalDescription: "Sistem mimarisi; endüstriyel haberleşme katmanı (Kepware / OPC UA Server), yüksek hızlı mesaj omurgası, bellek içi önbellek ve Web tabanlı SCADA istemcisinden oluşur. PLC katmanındaki değişkenler OPC UA subscription modeli ile sadece değişim anında (on-change) veya 1 saniyelik periyotlarla okunur.\n\nASP.NET Core 9 SignalR hub servisi, gelen süreç telemetrisini Redis bellek veritabanında günceller ve eşzamanlı bağlı tüm kontrol odası web istemcilerine WebSocket üzerinden mikrosaniye gecikmeyle yayınlar. Tarihsel veri sorguları için SQL Server tabanlı optimize edilmiş indeksli arşiv tabloları kullanılır.\n\nKullanıcı arayüzünde SVG tabanlı vektörel süreç ekipmanları, boru akış animasyonları ve Chart.js / Highcharts kütüphaneleriyle desteklenen çok kanallı trend analiz araçları yer alır.",
            BusinessImpact: "Tesis genelinde süreç şeffaflığı ve kontrol odası karar hızını artırır. Üniteler arası debi ve yoğunluk dengesizliklerinin erkenden fark edilmesine yardımcı olarak kararlı üretime katkı sağlar.",
            TargetAudience: "Tesis Kontrol Odası Operatörleri, Proses Metalurjistleri, Otomasyon Mühendisleri ve Üretim Şefleri.",
            AccessInstructions: "Tesis kontrol odası konsollarından ve yetkili mühendislik iş istasyonlarından web tarayıcısı üzerinden erişilir. Endüstriyel ağ segmentasyonu ile korunur.",
            CoverTheme: "control-room",
            CategoryCode: "AUTOMATION",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 1, 20),
            EndDate: null,
            CreatedAt: new DateTime(2026, 2, 8, 11, 15, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 2, 8, 11, 15, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Sivas Kangal Zenginleştirme Tesisi", "Malatya Hekimhan Peletleme Tesisi" },
            TechnologyNames: new[] { "OPC UA", "ASP.NET Core", "Redis", "SQL Server", "React", "Typescript" },
            TeamNames: new[] { "Süreç Otomasyonu ve Robotik Ekibi", "IoT ve Otomasyon Ekibi" },
            TagNames: new[] { "SCADA ve PLC", "Otomasyon", "Üretim Takibi", "Endüstri 4.0" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Tesis PLC Ağı (Siemens & Schneider)", "OPC UA endüstriyel haberleşme protokolü ile süreç verisi akışı", IntegrationType.ExternalService),
                ("Laboratuvar LIMS Arayüzü", "Saatlik nem ve tenör ölçümlerinin süreç ekranlarında gösterimi", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Can Özkan", "Otomasyon & SCADA Lideri"),
                ("Mehmet Demir", "Endüstriyel İletişim Uzmanı"),
                ("Büşra Kurt", "Frontend UI/UX Geliştirici")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("SCADA Etiket Adlandırma ve Haberleşme Standartları", "SCADA_Tag_Listesi_v2.pdf", "TechnicalSpec", "OPC UA node listesi ve alarm eşik değerleri tablosu")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 10. Mobil Saha Denetim ve Vardiya Operasyonları Yönetimi (Yazılım)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Mobil Saha Denetim ve Vardiya Operasyonları Yönetim Uygulaması",
            Slug: "mobil-saha-denetim-vardiya-yonetim-uygulamasi",
            ShortDescription: "Saha formenleri ve bakım teknisyenlerinin vardiya kontrolleri, ekipman denetimleri ve görev takiplerini çevrimdışı destekli yürüttüğü mobil platformdur.",
            Description: "Açık ocak ve zenginleştirme tesislerinde vardiya başlangıcı ekipman kontrolleri, yağlama formları, bant kontrol turları ve güvenlik denetimleri operasyonel sürekliliğin temel adımlarıdır. Sahada internet çekmeyen kör noktalarda formların kağıtla doldurulması bilgi akışını geciktirebilmektedir.\n\nBu uygulama; .NET MAUI teknolojisi ile geliştirilmiş, tablet ve sağlamlaştırılmış (rugged) el terminallerinde çalışan çevrimdışı öncelikli (offline-first) bir mobil saha asistanıdır. Saha teknisyenleri QR kod veya NFC etiketlerini okutarak ekipman başına özel kontrol listelerini doldurur, fotoğraf ekler ve sesli not bırakabilir.\n\nCihaz internet kapsama alanına girdiğinde sahadan toplanan tüm denetim verileri merkezi SQL Server veritabanına senkronize edilir, acil aksiyon gerektiren bulgular bakım şeflerine anlık bildirim olarak iletilir.",
            Purpose: "Saha denetim ve vardiya kontrol süreçlerini kağıtsız ortama taşımak, ekipman kontrollerinin sahada gerçekten yapıldığını NFC/QR ve GPS doğrulamasıyla güvence altına almaktır.\n\nVardiya devir teslimlerinde bilgi kaybını önlemeyi ve ekipman arıza bildirimlerinin hızla açılmasını sağlamayı hedefler.",
            ProblemSolved: "Daha önce kağıt formlarla yapılan kontroller vardiya sonunda ofise dönüldüğünde sisteme girilmekte veya gecikmeli olarak dosyalanmaktaydı. Sahada tespit edilen kritik bir yağ kaçağı veya mekanik gevşeklik saatler sonra bakım ekibine ulaşabiliyordu.\n\nUygulama, çevrimdışı çalışan ve ilk bağlantıda senkronize olan yapısıyla sahadaki arıza tespitini bakım ekibinin ekranına dakikalar içinde ulaştırır.",
            NonTechnicalDescription: "Saha çalışanlarımızın ve ustalarımızın ellerindeki tabletlerden kullandıkları pratik bir mobil uygulamadır. Makinelerin üzerindeki barkodları okutarak kontrol listelerini doldururlar ve gördükleri aksaklıkların fotoğrafını çekip sisteme yüklerler.\n\nİnternet çekmeyen çukurlarda bile çalışan uygulama, internete bağlanınca tüm bilgileri otomatik olarak merkeze gönderir.",
            TechnicalDescription: "Mobil uygulama .NET MAUI (C#) ile geliştirilmiş olup iOS ve Android tabanlı cihazlarda yerel (native) performansla çalışır. Çevrimdışı veri tutarlılığı için cihaz üzerinde SQLite veritabanı kullanılır; kullanıcı girdileri yerel SQLite tablosuna yazılır ve senkronizasyon kuyruğuna (sync queue) eklenir.\n\nCihaz ağ bağlantısını tespit ettiğinde (Connectivity API), arka planda çalışan senkronizasyon servisi bekleyen kayıtları REST API üzerinden ASP.NET Core arka ucuna taşır. Çatışma çözümü (conflict resolution) 'sunucu zaman damgası önceliği' politikasıyla yürütülür.\n\nKamera entegrasyonu ile çekilen fotoğraflar cihaz üzerinde sıkıştırılır (WebP formatında) ve sunucuya multipart/form-data ile yüklenerek CDN depolama dizinine aktarılır. Kullanıcı rolleri ve denetim şablonları JWT token tabanlı yetkilendirme ile dinamik olarak sunucudan çekilir.",
            BusinessImpact: "Saha kontrol listelerinin zamanında ve yerinde doldurulmasını sağlar. Vardiya devirlerinde tespit edilen ekipman kusurlarının bakım ekiplerine iletilme süresini kısaltır, kağıt kullanımını ortadan kaldırır.",
            TargetAudience: "Saha Formenleri, Mekanik ve Elektrik Bakım Teknisyenleri, Yağlama Ekipleri ve Vardiya Amirleri.",
            AccessInstructions: "Kurumsal Mobil Cihaz Yönetimi (MDM) portalı üzerinden şirket tabletlerine yüklenir. Kullanıcılar şirket sicil numarası ve PIN ile giriş yapar.",
            CoverTheme: "mobile-field-worker",
            CategoryCode: "SOFTWARE",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 2, 10),
            EndDate: null,
            CreatedAt: new DateTime(2026, 2, 1, 13, 40, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 2, 1, 13, 40, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Divriği Demir Sahası", "Balıkesir Manyas Sahası" },
            TechnologyNames: new[] { ".NET MAUI", "ASP.NET Core", "SQL Server", "React", "Typescript" },
            TeamNames: new[] { "Yazılım Geliştirme Ekibi", "İSG Dijital Takip Ekibi" },
            TagNames: new[] { "Mobil Çözümler", "Saha Yönetimi", "Vardiya Yönetimi", "Otomasyon" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Kurumsal Bakım Yönetimi (SAP PM)", "Mobil formlardan onaylanan arıza bildirimlerinin doğrudan SAP PM bildirimine dönüştürülmesi", IntegrationType.RestApi),
                ("Merkezi Kimlik Doğrulama (SSO)", "Kullanıcı sicil ve şifre doğrulaması için LDAP entegrasyonu", IntegrationType.ExternalService)
            },
            Members: new (string, string)[]
            {
                ("Derya Aksoy", "Mobil Uygulama Lideri"),
                ("Ahmet Yılmaz", "Arka Uç & Veritabanı Mimarı"),
                ("Melis Güler", "Saha Devreye Alma & Test Mühendisi")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Mobil Saha Uygulaması Kullanım Kılavuzu", "Mobil_Saha_Kullanici_Rehberi.pdf", "UserManual", "Çevrimdışı mod kullanımı ve NFC etiket okutma adımları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 11. Kurumsal SAP ERP ve Saha Sistemleri Entegrasyon Katmanı (Yazılım)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Kurumsal SAP ERP ve Saha Üretim Sistemleri Entegrasyon Katmanı",
            Slug: "kurumsal-sap-erp-saha-uretim-entegrasyonu",
            ShortDescription: "Saha kantar, üretim sayaçları ve ambar hareketlerini kurumsal SAP ERP modülleriyle çift yönlü ve hataya dayanıklı senkronize eden servis mimarisidir.",
            Description: "Büyük ölçekli madencilik işletmelerinde operasyonel saha verileri (üretilen tonaj, tüketilen patlayıcı, kamyon seferleri, yedek parça çıkışları) ile kurumsal kaynak planlama (SAP S/4HANA) finans ve lojistik modülleri arasında kusursuz bir veri köprüsü bulunmalıdır.\n\nBu proje; .NET 9 mimarisi üzerinde koşan, yüksek işlem hacimli (high-throughput), kuyruk tabanlı ve idempotent bir kurumsal entegrasyon ara yazılımıdır. Saha kantar sistemlerinden gelen irsaliye kayıtlarını SAP Malzeme Yönetimi (MM) ve Satış Dağıtım (SD) modüllerine otomatik aktarır; üretim miktarlarını Üretim Planlama (PP) ile senkronize eder.\n\nOlası şebeke kesintilerinde işlem kuyruğunu kaybetmeyen 'Transactional Outbox' deseni ve otomatik yeniden deneme (retry) mekanizmaları sayesinde kurumsal finans kayıtlarının mutabakat tutarlılığını garanti altına alır.",
            Purpose: "Saha operasyonel hareketleri ile kurumsal ERP muhasebe ve envanter kayıtları arasındaki veri aktarımını otomatikleştirerek manuel fiş girişlerini ve mutabakat farklarını ortadan kaldırmaktır.\n\nİşletme genelinde malzeme, maliyet ve üretim akışının anlık finansal görünürlüğünü sağlamayı hedefler.",
            ProblemSolved: "Eski sistemde kantar tartım fişleri ve ambar malzeme çıkışları gün sonunda operatörler tarafından Excel dosyalarından SAP ekranlarına manuel olarak girilmekteydi. Bu durum insan kaynaklı yazım hatalarına, faturalandırma gecikmelerine ve stok tutarsızlıklarına neden oluyordu.\n\nEntegrasyon katmanı, sahadaki kantar veya ambar onay anında SAP üzerinde otomatik malzeme belgesi ve irsaliye oluşturarak gecikmeleri sıfıra indirir.",
            NonTechnicalDescription: "Sahadaki kantarlardan geçen kamyonların tartım bilgileri ile ambardan çıkan yedek parçaların kaydını doğrudan şirketin ana muhasebe ve yönetim programı olan SAP'ye otomatik aktaran dijital köprüdür.\n\nBöylece çalışanların elle fatura ve fiş girmesine gerek kalmaz, stoklar ve üretim rakamları her an kuruşu kuruşuna doğru görünür.",
            TechnicalDescription: "Entegrasyon mimarisi; ASP.NET Core 9 Minimal API, RabbitMQ mesaj kuyruğu ve Docker üzerinde çalışan Worker servislerinden meydana gelir. Saha kantar veritabanları ve IoT sayaçlarından gelen hareketler, EF Core Transactional Outbox deseni ile yerel kuyruğa yazılır ve ardından RabbitMQ topic exchange'lerine fırlatılır.\n\nEntegrasyon servisleri, SAP RFC / OData ve SAP .NET Connector (NCo 3.1) kütüphanelerini kullanarak SAP S/4HANA BAPI fonksiyonlarını çağırır (örn. BAPI_GOODSMVT_CREATE, BAPI_OUTB_DELIVERY_CREATE). Tüm istekler idempotent GUID anahtarları ile korunarak mükerrer kayıt oluşumu engellenir.\n\nPolly kütüphanesi ile üstel geri çekilmeli yeniden deneme (exponential backoff retry) ve devre kesici (circuit breaker) desenleri uygulanır. Başarısız olan işlemler Dead Letter Queue (DLQ) içine alınarak web tabanlı Entegrasyon Yönetim Paneli üzerinden hata detayıyla birlikte yeniden tetiklenebilir.",
            BusinessImpact: "Saha irsaliye ve üretim verilerinin SAP ERP sistemine aktarım hızını artırır. Manuel veri girişi ihtiyacını ve insan hatalarını azaltır, ay sonu finansal mutabakat süreçlerinin hızlanmasına katkı sağlar.",
            TargetAudience: "ERP Sistem Yöneticileri, Muhasebe ve Finans Uzmanları, Saha Kantar Sorumluları ve Ambar Yöneticileri.",
            AccessInstructions: "Entegrasyon Yönetim Paneli üzerinden kurumsal Admin ve ERP Danışmanı rolleriyle erişilir. Servis uç noktaları mTLS ve API Key ile korunur.",
            CoverTheme: "software-platform",
            CategoryCode: "SOFTWARE",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 1, 5),
            EndDate: null,
            CreatedAt: new DateTime(2026, 1, 25, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 1, 25, 10, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Genel Müdürlük (Ankara)", "Sivas Kangal Zenginleştirme Tesisi" },
            TechnologyNames: new[] { ".NET 9", "ASP.NET Core", "SQL Server", "Docker / Kubernetes", "React" },
            TeamNames: new[] { "Kurumsal Uygulamalar ve ERP Ekibi", "Yazılım Geliştirme Ekibi" },
            TagNames: new[] { "ERP Entegrasyonu", "Mikroservis", "Bulut Bilişim", "Otomasyon" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("SAP S/4HANA ERP Sistemi", "OData ve BAPI fonksiyonları ile MM, SD ve PP modüllerine çift yönlü senkronizasyon", IntegrationType.RestApi),
                ("Kantar Otomasyon SQL Veritabanı", "Tartım hareketlerinin Change Data Capture (CDC) ile izlenmesi", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Gamze Aslan", "Kurumsal ERP Entegrasyon Lideri"),
                ("Ahmet Yılmaz", "Kıdemli Yazılım Mimarı"),
                ("Tolga Bulut", "DevOps & Altyapı Mühendisi")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("SAP Entegrasyon Sözleşmesi ve BAPI Haritası", "SAP_Entegrasyon_Spesifikasyonu.pdf", "TechnicalSpec", "Veri alanları eşleştirmesi ve hata kodları tablosu")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 12. Maden Sahaları İSG Risk Analizi ve Ramak Kala Bildirim Platformu (Yazılım)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Maden Sahaları İSG Risk Analizi ve Ramak Kala Dijital Bildirim Platformu",
            Slug: "isg-risk-analizi-ramak-kala-dijital-bildirim",
            ShortDescription: "Saha çalışanlarının tehlikeli durum ve ramak kala olaylarını anında fotoğraflayıp bildirdiği, düzeltici-önleyici faaliyetlerin (DÖF) takip edildiği kurumsal portaldır.",
            Description: "Madencilik sektöründe sıfır iş kazası hedefine ulaşmanın en etkili yolu, kazaya dönüşmemiş tehlikeli durumların ve ramak kala olaylarının erkenden raporlanması ve kök nedenlerinin ortadan kaldırılmasıdır. Çalışanların kolayca bildirim yapabilmesi ve aksiyonların şeffaf izlenmesi güvenlik kültürünü pekiştirir.\n\nBu platform; mobil uyumlu web arayüzü ve saha kioskları üzerinden çalışanların gördükleri riskli durumları saniyeler içinde fotoğraflı olarak kaydetmesine imkan tanır. Yapay zeka tabanlı metin kategorizasyonu ile gelen bildirimler ilgili risk kategorisine (düşme tehlikesi, elektrik, trafik, malzeme düşmesi) otomatik ayrıştırılır.\n\nİSG uzmanları ve saha yöneticileri, açılan DÖF (Düzeltici ve Önleyici Faaliyet) kayıtlarını ilgili birim şeflerine atar; termin tarihleri, tamamlanma kanıt fotoğrafları ve risk matrisi skorları tek bir panelden yönetilir.",
            Purpose: "Saha genelinde ramak kala ve tehlike bildirim kültürünü yaygınlaştırmak, İSG aksiyonlarının takibini dijitalleştirerek gecikmiş veya kapatılmamış riskleri görünür kılmaktır.\n\nYönetime proaktif İSG analizleri ve risk yoğunluk haritaları sunmayı hedefler.",
            ProblemSolved: "Eski yöntemle sarı-kırmızı kağıt kutularına atılan bildirim formları haftalık toplanmakta, kimin sorumlu olduğu ve hangi aksiyonun alındığı çalışanlar tarafından takip edilememekteydi. Bu durum bildirim motivasyonunu düşürmekteydi.\n\nPlatform, bildirimi yapan çalışana SMS/mobil bildirim ile sürecin durumunu (Örn: 'Bildirdiğiniz korkuluk onarılmıştır') ileterek geri bildirim döngüsünü kapatır ve güven ortamını güçlendirir.",
            NonTechnicalDescription: "Sahada çalışan herkesin cep telefonundan veya sahadaki dokunmatik ekranlardan tehlikeli gördüğü bir durumu (örneğin gevşek bir taş, kırık bir merdiven) fotoğrafını çekerek bildirebildiği sistemdir.\n\nBildirim yapıldığında İSG ekipleri hemen bilgilendirilir, tamir edildiğinde ise bildirimi yapan kişiye bilgi verilir.",
            TechnicalDescription: "Platform, ASP.NET Core 9 arka ucu ve React/TypeScript tabanlı duyarlı (responsive) web arayüzü ile geliştirilmiştir. Kullanıcı deneyimi hızlı form tamamlama üzerine optimize edilmiştir; cihaz kamerası doğrudan HTML5 Media Capture API ile entegre çalışır.\n\nGelen bildirimlerin metin içeriği, arka uçta hafif bir TF-IDF ve Scikit-Learn sınıflandırıcısı tarafından taranarak risk alanı ve öncelik skoru önerisinde bulunur. SQL Server üzerinde çalışan ilişkisel şema; olay detayları, atanan aksiyon sorumluları, termin süreleri ve DÖF onay adımlarını tutar.\n\nİnteraktif gösterge panelinde Chart.js ile 5x5 L-Tipi Risk Matrisi görselleştirmesi sunulur; açık/kapalı DÖF oranları, ortalama kapatma süreleri ve departman bazlı bildirim sayıları canlı olarak izlenir.",
            BusinessImpact: "İş sağlığı ve güvenliği kültürünü güçlendirir, saha çalışanlarının risk bildirim süreçlerine katılımını destekler. Düzeltici faaliyetlerin termin tarihlerine uygun biçimde kapatılma oranlarını artırır.",
            TargetAudience: "Tüm Saha Çalışanları, İSG Uzmanları, Saha Emniyet Şefleri, Departman Yöneticileri.",
            AccessInstructions: "Tüm çalışanlar şirket sicil numaraları ile web tarayıcısından veya saha tabletlerinden doğrudan giriş yapabilir.",
            CoverTheme: "safety-monitoring",
            CategoryCode: "SOFTWARE",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 12, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 1, 18, 14, 20, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 1, 18, 14, 20, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Divriği Demir Sahası", "Eskişehir Mihalıççık Sahası", "Genel Müdürlük (Ankara)" },
            TechnologyNames: new[] { "ASP.NET Core", "React", "Typescript", "SQL Server" },
            TeamNames: new[] { "İSG Dijital Takip Ekibi", "Yazılım Geliştirme Ekibi" },
            TagNames: new[] { "İş Güvenliği", "Saha Yönetimi", "Mobil Çözümler", "Veri Analitiği" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Kurumsal E-Posta / Bildirim Ağ Geçidi", "Atanan DÖF görevlerinin sorumlulara otomatik e-posta ile hatırlatılması", IntegrationType.RestApi),
                ("İK Personel Veritabanı", "Çalışan departman ve unvan bilgilerinin otomatik doldurulması", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Elif Demir", "İSG Dijital Süreç Uzmanı"),
                ("Büşra Kurt", "Frontend Geliştirici"),
                ("Ahmet Yılmaz", "Yazılım Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("DÖF Yönetimi ve Ramak Kala Prosedürü", "ISG_DOF_Proseduru_v3.pdf", "Procedure", "Tehlike derecelendirme ve aksiyon kapatma kriterleri")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 13. RFID ve Otomatik Kantar Entegre Sevkiyat Sistemi (Otomasyon)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Maden Stok Sahaları RFID ve Otomatik Kantar Entegre Sevkiyat Sistemi",
            Slug: "rfid-otomatik-kantar-sevkiyat-sistemi",
            ShortDescription: "Cevher ve konsantre sevkiyatında kamyonların RFID etiketleri, otomatik plaka tanıma ve kantar sensörleriyle insansız tartılmasını sağlayan lojistik yazılımıdır.",
            Description: "Maden işletmelerinden limanlara veya demir-çelik fabrikalarına yapılan cevher ve pelet sevkiyatlarında kantar istasyonları operasyonel darboğaz oluşturabilmektedir. Kamyon tartım sürelerinin uzaması saha çıkışlarında uzun araç kuyruklarına yol açabilmektedir.\n\nBu sistem; kantar giriş ve çıkışlarına yerleştirilen uzun menzilli UHF RFID antenleri, optik plaka tanıma (ANPR) kameraları, trafik ışıkları ve bariyer otomasyonu ile tam entegre çalışır. Kamyon kantara çıktığında sürücü araçtan inmeden plaka ve RFID doğrulanır, stabil ağırlık algılandığında tartım otomatik kaydedilerek bariyer açılır.\n\nSistem, kantar tartım verilerini doğrudan irsaliye ve SAP sevkiyat modülü ile eşleştirerek kaçak veya hatalı yükleme risklerini engeller, kantar işlem süresini önemli ölçüde hızlandırır.",
            Purpose: "Cevher sevkiyat kantar süreçlerini otomatikleştirmek, sürücülerin araçtan inme zorunluluğunu kaldırarak kantar çevrim süresini hızlandırmak ve tartım güvenliğini maksimize etmektir.\n\nLojistik birimlerine gerçek zamanlı sevkiyat ve vagon/kamyon yükleme raporlaması sağlamayı amaçlar.",
            ProblemSolved: "Klasik manuel kantar sistemlerinde kantar görevlisinin plaka ve malzeme türünü elle yazması, sürücünün imza için kulübeye girmesi gibi adımlar araç başına dakikalar sürmekte ve insan hatasına açık bulunmaktaydı.\n\nProje; RFID, plaka tanıma ve ağırlık stabilizasyon algoritması ile tartım sürecini tamamen otomatikleştirerek sürücü temasını ve yazım hatalarını ortadan kaldırır.",
            NonTechnicalDescription: "Maden taşıyan kamyonların kantar tartımını otoyollardaki HGS sistemi gibi otomatik yapan düzeneğidir. Kamyon kantara yanaştığında sistem plakayı ve araçtaki etiketi otomatik okur, ağırlığı ölçer ve yeşil ışık yakarak bariyeri açar.\n\nŞoförün araçtan inmesine gerek kalmaz, kapıdaki kamyon kuyrukları hızla erir.",
            TechnicalDescription: "Mimari; endüstriyel kantar terminali (indikatör), UHF RFID okuyucu donanım denetleyicisi, ANPR kamera servisi ve ASP.NET Core yönetim katmanından oluşur. Kantar indikatöründen RS-232 / Modbus RTU protokolü ile gelen ağırlık akışı, 'ağırlık kararlılık filtresi' (weight stability algorithm) ile süzülür.\n\nUHF RFID okuyuculardan (EPC Gen2) ve kameralardan gelen plaka eşleşmesi doğrulandığında, SQL Server üzerinde tartım fişi kaydı oluşturulur. Fotoğraf anı plaka ve kantar görüntüsüyle arşivlenir.\n\nBariyer açma ve trafik ışığı komutları endüstriyel dijital I/O modülleri üzerinden tetiklenir. React tabanlı kantar izleme konsolunda operatörler birden fazla kantar hattını canlı kamera görüntüsü ve kantar ağırlığıyla eşzamanlı izleyebilir.",
            BusinessImpact: "Kantar geçiş sürelerini hızlandırır, nakliye kamyonlarının kapıda bekleme sürelerini azaltır. Sevkiyat irsaliye ve tartım kayıtlarının şeffaflığını ve veri güvenliğini artırır.",
            TargetAudience: "Lojistik ve Sevkiyat Sorumluları, Kantar Operatörleri, Satış Operasyon Ekipleri ve Güvenlik Şefleri.",
            AccessInstructions: "Kantar kulübesi terminallerinden ve lojistik ofisi web konsolundan erişilir. Tartım düzeltme yetkileri yalnızca Lojistik Yöneticisi rolüne açıktır.",
            CoverTheme: "warehouse-logistics",
            CategoryCode: "AUTOMATION",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Hybrid,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 10, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 1, 10, 11, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 1, 10, 11, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "İzmir Liman ve Lojistik Merkezi", "Kayseri Develi Lojistik Sahası", "Divriği Demir Sahası" },
            TechnologyNames: new[] { "ASP.NET Core", "MQTT / IoT Edge", "SQL Server", "React", "Typescript" },
            TeamNames: new[] { "Lojistik ve Filo Optimizasyonu Ekibi", "IoT ve Otomasyon Ekibi" },
            TagNames: new[] { "Lojistik ve Filo", "Otomasyon", "Saha Yönetimi", "Endüstri 4.0" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Kantar Donanım İndikatörleri (Baykon / Tunaylar)", "Seri port ve Modbus protokolü üzerinden ağırlık verisi okuma", IntegrationType.ExternalService),
                ("SAP SD Satış & Sevkiyat Modülü", "Otomatik mal çıkış belgesi ve kantar irsaliyesi oluşturulması", IntegrationType.RestApi)
            },
            Members: new (string, string)[]
            {
                ("Onur Şimşek", "Lojistik & Sevkiyat Lideri"),
                ("Can Özkan", "Otomasyon & Donanım Mühendisi"),
                ("Ahmet Yılmaz", "Sistem Yazılım Mimarı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Otomatik Kantar Kullanım ve Kalibrasyon Kılavuzu", "Otomatik_Kantar_Klavuz_v2.pdf", "Manual", "İndikatör sıfırlama ve RFID etiket eşleştirme adımları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 14. Yedek Parça ve Kritik Ekipman Ambar Stok Optimizasyonu (Veri Analitiği)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Yedek Parça ve Kritik Ekipman Ambar Stok Optimizasyonu Modülü",
            Slug: "yedek-parca-ambar-stok-optimizasyonu",
            ShortDescription: "Madencilik tesislerindeki kritik yedek parça envanterini arıza geçmişi ve tedarik sürelerine göre analiz ederek optimum stok seviyelerini belirleyen sistemdir.",
            Description: "Maden işletmelerinde değirmen astarları, kırıcı çeneleri, konveyör redüktörleri ve ağır iş makinesi motor parçaları gibi kalemlerin ambarda bulunmaması üretimin günlerce durmasına yol açabilirken; aşırı stok tutulması da yüksek işletme sermayesi maliyeti yaratır.\n\nBu modül; geçmiş iş emri kayıtları, ekipman çalışma saatleri, arıza frekansları ve tedarikçi teslim sürelerini (lead time) makine öğrenmesi modelleriyle analiz eder. Her yedek parça için dinamik emniyet stoğu (safety stock), sipariş verme noktası (ROP) ve ekonomik sipariş miktarı (EOQ) önerileri hesaplar.\n\nSistem; kritiklik analizi (ABC-XYZ matrisi) yaparak 'olmazsa olmaz' A-grubu parçaların tedarik risklerini erkenden raporlar ve satın alma ekiplerine proaktif sipariş önerileri sunar.",
            Purpose: "Kritik yedek parçaların stokta bulunmama riskini minimize ederken, atıl ve aşırı stok maliyetlerini kontrol altında tutmak; satın alma planlamasını veri odaklı hale getirmektir.\n\nBakım ve ambar ekipleri arasında ortak malzeme ihtiyaç görünürlüğü oluşturmayı amaçlar.",
            ProblemSolved: "Daha önce sipariş noktaları statik asgari-azami stok seviyeleriyle manuel yönetilmekte, tedarik sürelerindeki küresel uzamalar veya ekipman çalışma temposundaki artışlar stok seviyelerine zamanında yansıtılamamaktaydı.\n\nSistem, dinamik talep tahmin algoritmalarıyla stok parametrelerini mevsimsel ve operasyonel şartlara göre otomatik günceller.",
            NonTechnicalDescription: "Makinelerimiz bozulduğunda gerekli yedek parçanın depoda mutlaka hazır bulunmasını, ancak depolarda da gereksiz milyonlarca liralık malzemenin boş yere beklememesini sağlayan akıllı stok yönetim aracıdır.\n\nHangi parçadan ne zaman ve kaç adet sipariş edilmesi gerektiğini hesaplayarak satın alma birimini uyarır.",
            TechnicalDescription: "Modül, ASP.NET Core 9 web servisi ve Python tabanlı analitik mikroservis mimarisiyle çalışır. Veri ambarı olarak SQL Server kullanılır; SAP MM malzeme hareketleri (201, 261, 101 hareket kodları) ve SAP PM bakım iş emirleri tarihsel olarak indekslenir.\n\nStok tahmin motoru; Python üzerinde Prophet ve Scikit-Learn kütüphaneleriyle parça bazlı aralıklı talep (intermittent demand - Croston yöntemi) modelleri çalıştırır. Malzeme lead time varyansları Poisson dağılımı ile simüle edilerek güven aralığında emniyet stoğu seviyesi hesaplanır.\n\nPower BI Embedded gösterge panellerinde ABC/XYZ malzeme matrisi, stok devir hızı trendleri ve kritik sipariş önerileri interaktif filtrelerle sunulur.",
            BusinessImpact: "Kritik yedek parçaların tedarik süresi risklerinin önceden fark edilmesini destekler. Ambar stok devir hızının optimize edilmesine ve atıl stok maliyetlerinin kontrol altında tutulmasına katkı sağlar.",
            TargetAudience: "Ambar Yöneticileri, Satın Alma Uzmanları, Bakım Planlama Mühendisleri ve Finans Direktörleri.",
            AccessInstructions: "Kurumsal ERP portalı ve Satın Alma Karar Destek panosu üzerinden Active Directory yetkilendirmesi ile erişilir.",
            CoverTheme: "industrial-equipment",
            CategoryCode: "DATA_ANALYTICS",
            StatusCode: "ACTIVE_DEVELOPMENT",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 4, 1),
            EndDate: null,
            CreatedAt: new DateTime(2026, 1, 4, 15, 10, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 1, 4, 15, 10, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Divriği Demir Sahası", "Kangallı Altın Sahası", "Genel Müdürlük (Ankara)" },
            TechnologyNames: new[] { "ASP.NET Core", "SQL Server", "Power BI", "React", "Typescript" },
            TeamNames: new[] { "Kurumsal Uygulamalar ve ERP Ekibi", "Veri Analitiği Ekibi", "Kestirimci Bakım ve Güvenilirlik Ekibi" },
            TagNames: new[] { "ERP Entegrasyonu", "Veri Analitiği", "Otomasyon", "Kestirimci Bakım" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("SAP MM Malzeme Yönetimi", "Tarihsel malzeme tüketimleri ve satınalma siparişlerinin senkronizasyonu", IntegrationType.RestApi),
                ("Kestirimci Bakım Modülü", "Rulman ve motor sağlık skorlarına göre beklenen parça değişim tahminleri", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Gamze Aslan", "ERP Süreç Danışmanı"),
                ("Ayşe Kaya", "Veri Bilimci"),
                ("Murat Koç", "Bakım Planlama Temsilcisi")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Stok Optimizasyon Algoritması Metodoloji Raporu", "Stok_Optimizasyon_Rapor_v1.pdf", "TechnicalSpec", "Croston talep tahmini ve emniyet stoğu formülasyonları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 15. Sondaj Karot Görüntülerinden Otomatik Mineral ve Çatlak Sınıflandırma (Yapay Zeka)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Sondaj Karot Görüntülerinden Otomatik Mineral ve Çatlak Sınıflandırma",
            Slug: "sondaj-karot-otomatik-mineral-catlak-siniflandirma",
            ShortDescription: "Karot sandığı yüksek çözünürlüklü fotoğraflarını derin öğrenme ile analiz ederek kayaç türü, RQD çatlak indeksi ve alterasyon bölgelerini tespit eden Ar-Ge projesidir.",
            Description: "Maden arama ve jeoteknik sondajlarında çıkarılan karot numunelerinin jeologlar tarafından sandık sandık incelenmesi, litoloji loglarının yazılması ve RQD (Kaya Kalite Göstergesi) çatlak ölçümlerinin yapılması yoğun emek gerektiren bir süreçtir.\n\nBu proje; standart ışıklandırmalı karot tarama kabininde çekilen fotoğrafları bilgisayarlı görü ve anlamsal segmentasyon (semantic segmentation) modelleri ile işler. Karot parçalarının uzunluklarını, kırık sıklıklarını, alterasyon zonlarını ve mineral damarlarını piksel bazında sınıflandırır.\n\nSistem, jeologların loglama süresini önemli ölçüde kısaltırken dijital karot kütüphanesi oluşturarak karotların yıllar sonra bile yüksek çözünürlükte incelenebilmesini sağlar.",
            Purpose: "Karot loglama süreçlerinde standartlaşma sağlamak, RQD çatlak ölçümlerini otomatikleştirmek ve arama jeologlarına yapay zeka destekli ön sınıflandırma desteği sunmaktır.\n\nTüm karot arşivini yüksek çözünürlüklü dijital ortama aktararak fiziksel bozulma risklerine karşı korumayı hedefler.",
            ProblemSolved: "Fiziksel karot sandıkları zamanla hava şartlarından etkilenerek ufalanabilmekte veya nem kaybıyla renk değişimine uğrayabilmektedir. Ayrıca farklı jeologların gözlemsel loglama kriterlerindeki sübjektif yorum farkları veri tutarlılığını etkileyebilmekteydi.\n\nProje, nesnel dijital görüntü işleme modelleriyle standart bir sınıflandırma referansı oluşturur.",
            NonTechnicalDescription: "Yer altından çıkarılan silindir şeklindeki taş örneklerinin (karot) fotoğraflarını yapay zekaya inceleterek taşın cinsi ve üzerindeki çatlakların otomatik olarak tespit edilmesini sağlayan akıllı sistemdir.\n\nJeoloji mühendislerimizin günlerce süren taş inceleme ve ölçüm işlerini dakikalar seviyesine indirir.",
            TechnicalDescription: "Görüntü işleme hattı, PyTorch üzerinde geliştirilmiş Mask R-CNN ve U-Net tabanlı segmentasyon mimarilerinden oluşur. Karot sandığı fotoğraflarındaki satır ayırıcı çıtalar tespit edilerek her karot sırası ayrı ayrı kırpılır ve perspektif düzeltmesi (affine transformation) uygulanır.\n\nSegmentasyon modeli; masif kayaç, kırıklı zon, kil alterasyonu ve damar dolgusu sınıflarını ayırır. 10 cm'den uzun sağlam karot parçaları otomatik ölçülerek RQD = (Toplam Sağlam Parça Boyu / Sandık Boyu) * 100 formülüyle sayısal RQD skoru türetilir.\n\nÇıkarım servisi Docker konteyneri içinde GPU hızlandırmasıyla çalışır. Web arayüzünde OpenSeadragon kütüphanesi kullanılarak gigapiksel çözünürlükteki karot fotoğrafları üzerinde yakınlaştırma ve katman açma/kapama imkanı sunulur.",
            BusinessImpact: "Arama jeolojisi loglama hızını artırır ve standartlaştırır. Karot numunelerinin fiziksel aşınma öncesinde dijital arşivlenmesini sağlar, jeoteknik şev tasarımı için hızlı RQD verisi üretir.",
            TargetAudience: "Arama Jeologları, Jeoteknik Mühendisleri, Maden Arama Yöneticileri ve Ar-Ge Ekipleri.",
            AccessInstructions: "Ar-Ge Dijital Karot Portalı üzerinden yetkili jeoloji kullanıcılarına açıktır. Yüksek çözünürlüklü tarama kabini donanımıyla entegre çalışır.",
            CoverTheme: "ai-computer-vision",
            CategoryCode: "ARTIFICIAL_INTELLIGENCE",
            StatusCode: "PILOT",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 6, 1),
            EndDate: null,
            CreatedAt: new DateTime(2025, 12, 28, 9, 30, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 12, 28, 9, 30, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "İstanbul Ar-Ge Merkezi", "Kangallı Altın Sahası" },
            TechnologyNames: new[] { "Python / PyTorch", "OpenCV", "Docker / Kubernetes", "React", "Typescript" },
            TeamNames: new[] { "Yapay Zeka ve Görüntü İşleme Ekibi", "CBS ve Jeolojik Modelleme Ekibi" },
            TagNames: new[] { "Yapay Zeka", "Görüntü İşleme", "Jeolojik Modelleme", "Veri Analitiği" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Karot Tarama Kabini Donanım Arayüzü", "Kamera tetikleme ve kalibre edilmiş aydınlatma senkronizasyonu", IntegrationType.ExternalService),
                ("CBS Jeoloji Veritabanı", "Sondaj kuyu derinlik loglarına RQD ve litoloji verilerinin yazılması", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Zeynep Şahin", "Yapay Zeka Proje Lideri"),
                ("Burak Aydın", "Jeoloji & Karot Uzmanı"),
                ("Büşra Kurt", "Web Arayüz Geliştirici")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Karot Görüntü İşleme ve RQD Hesaplama Algoritması", "Karot_Yapay_Zeka_Algoritma.pdf", "TechnicalSpec", "Segmentasyon eğitim parametreleri ve doğrulama metrikleri")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 16. Maden Atıkları Barajı ve Liç Sahası Jeoteknik Kararlılık Erken Uyarı Sistemi (IoT)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Maden Atıkları Barajı ve Liç Sahası Jeoteknik Kararlılık Erken Uyarı Sistemi",
            Slug: "atik-baraji-lic-sahasi-jeoteknik-erken-uyari",
            ShortDescription: "Atık barajı gövdesi ve liç yığınlarındaki inklinometre, oturma hücreleri ve sızıntı debilerini sürekli analiz eden jeoteknik güvenlik platformudur.",
            Description: "Madencilik operasyonlarında atık barajları (tailings dams) ve yığın liçi sahalarının jeoteknik kararlılığı, hem çevre emniyeti hem de işletme güvenliği açısından en yüksek öneme sahip risk unsurlarından biridir. Gövde içindeki boşluk suyu basıncı değişimleri ve mikroskobik zemin deformasyonları kesintisiz izlenmelidir.\n\nBu sistem; baraj kretine ve şevlerine yerleştirilen çok noktalı otomatik inklinometreler, piezometreler, oturma plakaları ve sızıntı toplama hendeklerindeki debimetrelerden LoRaWAN ve fiber optik hatlar üzerinden telemetri toplar. Sayısal zemin mekaniği modelleriyle entegre çalışarak deformasyon hızlarını analiz eder.\n\nKritik sınır eşiklerine yaklaşıldığında kademeli erken uyarı mekanizması (Sarı, Turuncu, Kırmızı alarm) devreye girer; acil durum eylem planı çerçevesinde İSG ve çevre yöneticilerine otomatik bildirimler gönderilir.",
            Purpose: "Atık depolama ve liç tesislerinde gövde stabilitesini ve zemin hareketlerini 7/24 kesintisiz izlemek, olası jeoteknik riskleri aylar öncesinden tespit ederek önleyici tahkimat kararlarını desteklemektir.\n\nUluslararası atık yönetimi standartlarına (GISTM) tam uyumlu izleme altyapısı sağlamayı amaçlar.",
            ProblemSolved: "Periyodik olarak ayda bir yapılan manuel ölçümler, mevsimsel aşırı yağışlar sonrasında gelişen hızlı su tablası yükselmelerini ve kılcal şev kaymalarını zamanında yakalamak için yetersiz kalabilmekteydi.\n\nSistem, saatlik sürekli telemetri ölçümleri ile insan erişiminin zor olduğu kritik gövde noktalarında kesintisiz gözetim sağlar.",
            NonTechnicalDescription: "Maden atık barajlarımızın ve yığınlarımızın kayma veya çatlama yapıp yapmadığını toprağın altına yerleştirilen hassas dijital teraziler ve sensörlerle gece gündüz takip eden güvenlik kalkanıdır.\n\nToprakta milimetrik bir oynama veya su sızıntısı olduğunda sistem tehlike büyümeden mühendisleri anında haberdar eder.",
            TechnicalDescription: "Mimari; zorlu çevre koşullarına dayanıklı IP68 sensör düğümleri, sahra güneş panelleri, LoRaWAN ağ geçitleri ve merkezi jeoteknik karar destek servisinden oluşur. İnklinometre dizilerinden gelen eksenel eğim (tilt) verileri 24-bit ADC hassasiyetiyle toplanır.\n\nASP.NET Core 9 arka uç servisi, ham açısal sapma verilerini kümülatif yanal yer değiştirme (lateral displacement) profiline dönüştürür. InfluxDB zaman serisi motoru sızıntı debileri ve piezometrik basınç trendlerini kaydeder.\n\nKullanıcı arayüzünde baraj gövdesinin 3B kesiti üzerinde renk kodlu güvenlik faktörü (Factor of Safety - FoS) konturları dinamik olarak güncellenir. Yasal limit aşımlarında SMS, e-posta ve sesli saha sireni tetikleme arayüzleri mevcuttur.",
            BusinessImpact: "Atık depolama tesislerinde jeoteknik riskleri sürekli gözetim altında tutarak çevre ve insan güvenliğini en üst düzeyde korur. Uluslararası madencilik standartlarına uyumu belgeler.",
            TargetAudience: "Jeoteknik Mühendisleri, Çevre ve Atık Barajı Sorumluları, Tesis Emniyet Direktörleri.",
            AccessInstructions: "Jeoteknik Güvenlik Merkezi panosundan intranet üzerinden erişilir. Kritik alarm eşikleri yalnızca Baş Jeoteknik Mühendisi tarafından güncellenebilir.",
            CoverTheme: "open-pit-mine",
            CategoryCode: "IOT",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Hybrid,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 7, 1),
            EndDate: null,
            CreatedAt: new DateTime(2025, 12, 20, 16, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 12, 20, 16, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Kangallı Altın Sahası", "Divriği Demir Sahası" },
            TechnologyNames: new[] { "LoRaWAN", "MQTT / IoT Edge", "InfluxDB", "ASP.NET Core", "React" },
            TeamNames: new[] { "Çevre ve Enerji Yönetimi Ekibi", "IoT ve Otomasyon Ekibi", "CBS ve Jeolojik Modelleme Ekibi" },
            TagNames: new[] { "Çevre ve Sürdürülebilirlik", "İş Güvenliği", "IoT Sensör", "Telemetri" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Meteoroloji İstasyonu Telemetrisi", "Saha anlık yağış ve sıcaklık verilerinin jeoteknik modellerle korelasyonu", IntegrationType.RestApi),
                ("Saha Acil Durum Siren Sistemi", "Kırmızı alarm durumunda saha fiziksel sesli ikaz sisteminin tetiklenmesi", IntegrationType.ExternalService)
            },
            Members: new (string, string)[]
            {
                ("Ebru Çelik", "Çevre & Jeoteknik Güvenlik Sorumlusu"),
                ("Burak Aydın", "Jeoteknik Modelleme Uzmanı"),
                ("Emre Polat", "Saha Telemetri Mühendisi")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Atık Barajı İzleme ve Acil Eylem Planı", "Atik_Baraji_Acil_Eylem_v2.pdf", "Procedure", "Alarm kademeleri ve tahliye protokolleri dokümanı")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 17. Ağır İş Makineleri Yağ Spektrometri Analiz Platformu (Veri Analitiği)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Ağır İş Makineleri Hidrolik ve Şanzıman Yağ Spektrometri Analiz Platformu",
            Slug: "agir-ekipman-yag-spektrometri-analiz-platformu",
            ShortDescription: "İş makinelerinden periyodik alınan motor ve hidrolik yağ numunelerinin spektrometre element değerlerini analiz ederek aşınma trendlerini çıkaran sistemdir.",
            Description: "Maden kamyonları, ekskavatörler ve loderlerin motor, şanzıman ve cer dişlilerinde kullanılan yağlar; aşınan metal parçacıklarını bünyesinde toplar. Yağdaki demir (Fe), bakır (Cu), kurşun (Pb), krom (Cr) ve silisyum (Si) ppm değerleri mekanik organlardaki aşınmanın doğrudan göstergesidir.\n\nBu platform; maden laboratuvarında ve akredite dış laboratuvarlarda yapılan ICP-OES spektrometri, viskozite ve partikül sayım sonuçlarını araç bazında arşivler. Zaman serisi aşınma eğrileri üzerinden anormal element artışlarını (örneğin bronz burç aşınmasına işaret eden yüksek Cu/Pb oranı) tespit eder.\n\nSistem, mekanik bakım şeflerine 'bu kamyonun şanzımanında aşınma başladı, ilk bakımda kontrol edin' şeklinde erken teşhis önerileri sunar.",
            Purpose: "Ağır iş makinelerinde yağ analiz verilerini dijitalleştirerek mekanik aşınmaları erken evrede teşhis etmek, büyük motor ve şanzıman revizyon maliyetlerini önlemektir.\n\nMadeni yağ değişim periyotlarını sabit saat yerine yağın gerçek kimyasal ömrüne göre optimize etmeyi amaçlar.",
            ProblemSolved: "Daha önce PDF formatında laboratuvardan gelen yağ raporları bakım şeflerinin e-posta kutularında kaybolabilmekte, geçmiş 5 numunenin trend karşılaştırması manuel olarak yapılamamaktaydı.\n\nPlatform, tüm laboratuvar sonuçlarını araç kimlik kartına otomatik işleyerek element bazında artış trendlerini grafiklendirir ve kritik limit aşımlarında alarm üretir.",
            NonTechnicalDescription: "İş makinelerimizden düzenli olarak kan tahlili gibi yağ numuneleri alınır. Bu program, yağın içindeki mikroskobik metal parçacıklarını inceleyerek motorun veya şanzımanın neresinde aşınma başladığını gösterir.\n\nBöylece dev bir iş makinesi yolda kalmadan önce hangi dişlisinin eskidiği anlaşılır ve erkenden tamir edilir.",
            TechnicalDescription: "Platform, ASP.NET Core 9 arka ucu, SQL Server ilişkisel veri ambarı ve Python Scikit-Learn tabanlı trend analiz modülünden oluşur. Laboratuvarlardan gelen ASTM formatındaki LIMS analiz dosyaları otomatik parser servisi ile ayrıştırılır.\n\nHer araç bileşeni (Dizel Motor, Ön Diferansiyel, Arka Diferansiyel, Hidrolik Tank, Şanzıman) için element sınır eşikleri dinamik olarak yapılandırılır. İstatistiksel regresyon algoritmaları, aşınma hızının (ppm/çalışma saati) normal dağılım dışına çıktığı durumları z-skoru ile tespit eder.\n\nKullanıcı arayüzünde Power BI ve React bileşenleri ile araç bazlı element trend grafikleri, yağ viskozite değişim eğrileri ve kritik uyarı matrisleri sunulur.",
            BusinessImpact: "Ağır ekipmanlarda katastrofik motor ve şanzıman arızalarının erkenden fark edilmesini sağlar. Yağ değişim süreçlerinin optimizasyonuna ve madeni yağ sarfiyatının kontrolüne katkıda bulunur.",
            TargetAudience: "Ağır Ekipman Bakım Şefleri, Yağlama Mühendisleri, Filo Yöneticileri ve Güvenilirlik Ekipleri.",
            AccessInstructions: "Kestirimci Bakım ve Yağ Analiz Portalı üzerinden kurumsal giriş ile kullanılır. Saha bakım atölyelerindeki tabletlerden doğrudan araç geçmişi sorgulanabilir.",
            CoverTheme: "data-analytics",
            CategoryCode: "DATA_ANALYTICS",
            StatusCode: "ACTIVE",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2024, 11, 15),
            EndDate: null,
            CreatedAt: new DateTime(2025, 12, 14, 10, 45, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 12, 14, 10, 45, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Divriği Demir Sahası", "Kangallı Altın Sahası", "Balıkesir Manyas Sahası" },
            TechnologyNames: new[] { "Scikit-Learn", "Python / PyTorch", "ASP.NET Core", "SQL Server", "Power BI" },
            TeamNames: new[] { "Kestirimci Bakım ve Güvenilirlik Ekibi", "Veri Analitiği Ekibi" },
            TagNames: new[] { "Kestirimci Bakım", "Veri Analitiği", "Lojistik ve Filo", "Otomasyon" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Dış Akredite Yağ Laboratuvarı API'si", "Laboratuvardan onaylanan ICP spektrometri sonuçlarının otomatik içe aktarımı", IntegrationType.RestApi),
                ("Filo Telemetri Sistemi", "Araç çalışma saati ve kilometre verilerinin otomatik senkronizasyonu", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Murat Koç", "Kestirimci Bakım Mühendisi"),
                ("Cemal Çetin", "Mekanik Güvenilirlik Şefi"),
                ("Ayşe Kaya", "Veri Bilimci")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Maden Ekipmanları Yağ Analiz Limit Tablosu", "Yag_Analiz_Limit_Standartlari.pdf", "TechnicalSpec", "Ekipman tiplerine göre kabul edilebilir elementel kirlilik sınırları")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 18. Konveyör Bant Termal Kamera Sıcaklık ve Hız İzleme (Otomasyon)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Tesis İçi Konveyör Bant Termal Kamera Sıcaklık ve Hız İzleme Düzeneği",
            Slug: "konveyor-bant-termal-kamera-sicaklik-izleme",
            ShortDescription: "Cevher nakil hatlarında sürtünme kaynaklı aşırı ısınma, bant kayması ve yırtılma risklerini termal video analitiği ile denetleyen pilot sistemdir.",
            Description: "Bant konveyörlerinde sıkışan taşlar veya kilitlenen rulo yatakları, kauçuk bant üzerinde yoğun sürtünmeye ve yüksek sıcaklıklara neden olarak yangın ve boyuna yırtılma riskleri doğurur. Geleneksel halatlı acil durdurma telleri yalnızca olay geliştikten sonra müdahaleye izin verir.\n\nBu pilot proje; kritik konveyör istasyonlarına kurulan radyometrik termal kameralar ve optik hız enkoderleri ile çalışır. Termal kamera, bant yüzeyini ve rulo yataklarını sürekli tarayarak noktasal sıcaklık artışlarını (hotspot) derecesi derecesine ölçer.\n\nSistem, sürtünme kaynaklı sıcaklık artışı tespit ettiğinde önce kontrol odasını uyarır; kritik yangın eşiğine ulaşıldığında ise PLC hattına doğrudan acil durdurma sinyali göndererek bandı korumaya alır.",
            Purpose: "Konveyör hatlarında sürtünme kaynaklı yangın ve mekanik bant yırtılması risklerini termal görüntüleme ile başlangıç anında tespit ederek ekipman güvenliğini sağlamaktır.\n\nKritik nakil hatlarında yangın önleme standartlarını yükseltmeyi ve ekipman hasarını engellemeyi hedefler.",
            ProblemSolved: "Gözle görülmeyen rulo kilitlenmeleri veya bant altına sıkışan sivri kaya parçaları, dakikalar içinde yüzlerce metrelik kauçuk bandı boydan boya yırtabilmekte veya yangın çıkarabilmekteydi.\n\nSistem, termal anomaliyi saniyeler içinde yakalayarak hasarın büyümesini engeller.",
            NonTechnicalDescription: "Taşıyıcı bantların üzerine takılan termal kameralar, bantta aşırı ısınan veya sürtünen bir nokta olduğunda bunu sıcağa duyarlı gözleriyle hemen fark eder.\n\nBant tutuşmadan veya yırtılmadan önce sistemi durdurarak büyük yangın ve tamir masraflarının önüne geçer.",
            TechnicalDescription: "Sistem donanımı; FLIR/Hikvision endüstriyel radyometrik termal kameralar, optik hız enkoderleri ve saha Edge AI kontrol panosundan oluşur. Termal kameradan gelen radyometrik video akışında piksel bazlı sıcaklık matrisleri OpenCV ile işlenir.\n\nBant hareket halindeyken arka plan sıcaklığı filtrelenir ve hareketli yüzey üzerindeki tepe sıcaklık noktaları izlenir. Sıcaklık artış hızı (dT/dt) hesaplanarak ani sürtünme anomalileri statik sıcaklık değişimlerinden ayırt edilir.\n\nKritik eşik aşıldığında saha PLC'sine optokuplörlü kuru kontak (dry contact) üzerinden acil durdurma komutu gönderilir. Eşzamanlı olarak MQTT ile olay kaydı ve termal görüntü çerçevesi merkezi sunucuya iletilir.",
            BusinessImpact: "Konveyör hatlarında yangın ve bant yırtılma risklerine karşı proaktif güvenlik sağlar. Bant değişim ve plansız onarım maliyetlerinin önlenmesine katkıda bulunur.",
            TargetAudience: "Tesis Yangın Güvenlik Sorumluları, Mekanik Bakım Ekipleri, Otomasyon Mühendisleri.",
            AccessInstructions: "Tesis SCADA ve Yangın İzleme odası ekranlarından takip edilir. Pilot test logları Ar-Ge test sunucusunda depolanır.",
            CoverTheme: "industrial-automation",
            CategoryCode: "AUTOMATION",
            StatusCode: "PILOT",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 7, 1),
            EndDate: null,
            CreatedAt: new DateTime(2025, 12, 5, 13, 20, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 12, 5, 13, 20, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "Sivas Kangal Zenginleştirme Tesisi" },
            TechnologyNames: new[] { "OpenCV", "MQTT / IoT Edge", "ASP.NET Core", "React" },
            TeamNames: new[] { "IoT ve Otomasyon Ekibi", "Yapay Zeka ve Görüntü İşleme Ekibi", "Kestirimci Bakım ve Güvenilirlik Ekibi" },
            TagNames: new[] { "Görüntü İşleme", "Kestirimci Bakım", "Otomasyon", "İş Güvenliği" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Tesis Acil Stop Güvenlik Devresi", "Kritik sıcaklık limitinde konveyör ana kontaktörüne doğrudan acil durdurma sinyali", IntegrationType.ExternalService),
                ("Tesis SCADA Sistemi", "Konveyör termal sıcaklık profilinin canlı izleme ekranına aktarılması", IntegrationType.MessageQueue)
            },
            Members: new (string, string)[]
            {
                ("Mehmet Demir", "IoT & Termal Donanım Lideri"),
                ("Zeynep Şahin", "Görüntü İşleme Uzmanı"),
                ("Murat Koç", "Kestirimci Bakım Danışmanı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Termal Bant İzleme Pilot Test Raporu", "Termal_Bant_Pilot_Raporu.pdf", "TestReport", "Tetikleme eşikleri ve tepki süresi doğrulama testleri")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 19. Maden Rehabilitasyonu ve Bitkilendirme Süreçleri Uydu / Drone Takip (Ar-Ge)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Maden Rehabilitasyonu ve Bitkilendirme Süreçleri Uydu / Drone Takip Sistemi",
            Slug: "maden-rehabilitasyon-bitkilendirme-uydu-takip",
            ShortDescription: "Tamamlanan maden döküm sahalarında yürütülen ağaçlandırma ve toprak rehabilitasyonu çalışmalarını multispektral uydu ve İHA verileriyle izleyen Ar-Ge çalışmasıdır.",
            Description: "Madencilik faaliyetleri sona eren açık ocak basamakları ve pasa döküm sahalarının doğaya yeniden kazandırılması (rehabilitasyon), kurumsal sürdürülebilirlik ilkelerinin ayrılmaz bir parçasıdır. Dikilen fidanların tutma oranları ve bitki örtüsü gelişiminin geniş arazilerde yıllar boyunca izlenmesi gerekmektedir.\n\nBu Ar-Ge projesi; Sentinel-2 ve Landsat uydu görüntüleri ile multispektral kameralı drone uçuşlarından elde edilen verileri birleştirir. NDVI (Normalleştirilmiş Fark Bitki Örtüsü İndeksi) ve NDRE indekslerini hesaplayarak rehabilitasyon alanlarındaki yeşillenme yoğunluğunu ve kuraklık stresini haritalandırır.\n\nPlatform, çevre mühendislerine hangi parsellerde ek fidan dikimi veya sulama desteği gerektiğini gösterir; rehabilitasyon ilerleme raporlarını Orman Genel Müdürlüğü ve çevre standartlarına uygun formatta üretir.",
            Purpose: "Rehabilitasyon yapılan maden sahalarında bitkilendirme başarısını ve biyolojik çeşitlilik gelişimini uzaktan algılama yöntemleriyle bilimsel olarak izlemektir.\n\nSürdürülebilirlik raporlamaları için doğrulanabilir, zaman serisi uydu ve drone verisi sağlamayı amaçlar.",
            ProblemSolved: "Geniş sahalarda binlerce dönüm arazinin gözlemsel olarak yürünerek denetlenmesi yetersiz kalmakta, kurumuş ağaç alanları veya toprak erozyonu başlangıçları geç fark edilebilmekteydi.\n\nSistem, multispektral indeks haritalarıyla bitki sağlığını arazinin her metrekaresi için renklendirerek erken müdahale imkanı sunar.",
            NonTechnicalDescription: "Madencilik bittikten sonra ağaçlandırdığımız eski çalışma alanlarının uzaydaki uydulardan ve dronlardan çekilen özel fotoğraflarla takip edilmesidir.\n\nDiktiğimiz ağaçların ne kadar büyüdüğünü, nerelerin kuruduğunu veya sulamaya ihtiyaç duyduğunu renkli haritalarla gösterir.",
            TechnicalDescription: "Mimari; Copernicus Open Access Hub uydu veri çekme boru hattı, multispektral raster işleme servisi ve CBS görselleştirme katmanından oluşur. Sentinel-2 L2A seviyesi alt-kırmızı (RedEdge) ve yakın-kızılötesi (NIR) bantları otomatik olarak indirilir.\n\nPython GDAL ve Rasterio kütüphaneleriyle bulut maskeleme (cloud masking) uygulanır ve NDVI = (NIR - RED) / (NIR + RED) formülüyle 10 metrelik piksel çözünürlüğünde bitki örtüsü katmanları üretilir. Drone uçuşlarından gelen 5 bantlı multispektral TIF dosyaları ile yüksek çözünürlüklü parsel doğrulaması yapılır.\n\nZaman serisi NDVI değişim matrisleri PostgreSQL/PostGIS veritabanında saklanır ve Power BI / Web Harita arayüzünde yıllık yeşillenme trend grafikleri olarak sunulur.",
            BusinessImpact: "Maden sahalarının doğaya yeniden kazandırılması sürecinde bilimsel izleme altyapısı sağlar. Çevre yatırımlarının başarısını uydu verileriyle belgeler, sürdürülebilirlik raporlamasını güçlendirir.",
            TargetAudience: "Çevre ve Sürdürülebilirlik Mühendisleri, Orman Mühendisleri, Kurumsal İletişim Ekipleri ve Üst Yönetim.",
            AccessInstructions: "Ar-Ge ve Sürdürülebilirlik Portalı üzerinden erişilir. Harita katmanları halka açık kurumsal raporlarda özet formatta paylaşılabilir.",
            CoverTheme: "environmental-monitoring",
            CategoryCode: "RD",
            StatusCode: "PROOF_OF_CONCEPT",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: true,
            ApprovalStatus: ProjectApprovalStatus.Approved,
            StartDate: new DateOnly(2025, 5, 1),
            EndDate: null,
            CreatedAt: new DateTime(2025, 11, 25, 11, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 11, 25, 11, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: 1,
            LocationNames: new[] { "İstanbul Ar-Ge Merkezi", "Balıkesir Manyas Sahası", "Eskişehir Mihalıççık Sahası" },
            TechnologyNames: new[] { "Python / PyTorch", "PostgreSQL", "Power BI", "React" },
            TeamNames: new[] { "Çevre ve Enerji Yönetimi Ekibi", "CBS ve Jeolojik Modelleme Ekibi" },
            TagNames: new[] { "Çevre ve Sürdürülebilirlik", "CBS ve Haritalama", "Drone ve İHA", "Veri Analitiği" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Copernicus Sentinel-2 API", "Açık kaynak multispektral uydu görüntülerinin otomatik çekilmesi", IntegrationType.ExternalService),
                ("Kurumsal CBS Harita Sunucusu", "Rehabilitasyon parsel sınırları ve kadastro katmanlarının senkronizasyonu", IntegrationType.Database)
            },
            Members: new (string, string)[]
            {
                ("Ebru Çelik", "Çevre & Sürdürülebilirlik Lideri"),
                ("Burak Aydın", "Uzaktan Algılama ve CBS Uzmanı"),
                ("Kerem Vural", "Multispektral Drone Pilotu")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Maden Sahaları Rehabilitasyon İzleme Raporu", "Rehabilitasyon_Izleme_ArGe_v1.pdf", "Whitepaper", "Uydu indeksleri ve saha doğrulama ölçümleri karşılaştırması")
            }
        ),

        // ─────────────────────────────────────────────────────────────────────────────
        // 20. Tesis Döner Fırın ve Kırıcı Titreşim Anomali Tespiti (Ar-Ge - Taslak / Gizli)
        // ─────────────────────────────────────────────────────────────────────────────
        new(
            Name: "Tesis Döner Fırın ve Kırıcı Titreşim Anomali Tespiti Ar-Ge Projesi",
            Slug: "doner-firin-kirici-titresim-anomali-tespiti",
            ShortDescription: "Yüksek sıcaklıklı peletleme döner fırınlarında termal genleşme ve mekanik eksen kaçıklıklarını Rust tabanlı yüksek hızlı sinyal işleme ile analiz eden yeni nesil Ar-Ge çalışmasıdır.",
            Description: "Peletleme tesislerinde döner fırınlar (rotary kilns) ve ağır primer kırıcılar, aşırı sıcaklık ve mekanik yük altında çalışan devasa ekipmanlardır. Fırın gövdesindeki ovalleşmeler, ring kaymaları ve eksen kaçıklıkları geleneksel titreşim yöntemleriyle tespit edilmesi en güç mekanik problemler arasındadır.\n\nBu Ar-Ge çalışması; yüksek sıcaklığa dayanıklı lazer mesafe sensörleri, temassız takometreler ve kablosuz ivmeölçerlerden gelen saniyede 50 kHz frekanslı ham sinyalleri işlemek üzere Rust diliyle yazılmış özel bir uç analiz motoru geliştirmeyi hedefler.\n\nProje kapsamında geliştirilen hafif anomali modelleri; fırın dinamik eksen sapmalarını milisaniyeler içinde tespit ederek gövde çatlamalarını ve refrakter tuğla dökülmelerini önceden kestirmeyi amaçlamaktadır.",
            Purpose: "Yüksek sıcaklık ve ağır yük altında çalışan döner ekipmanlarda gelişmiş sinyal işleme ve yüksek hızlı Rust çekirdeği ile anomali tespit altyapısı geliştirmektir.\n\nPeletleme tesislerinde refrakter ve gövde hasarı kaynaklı uzun süreli fırın duruşlarını engellemeyi hedefler.",
            ProblemSolved: "Döner fırınların yavaş devirli (1-5 RPM) ve devasa boyutlu olması, standart titreşim algoritmalarının yetersiz kalmasına neden olmaktaydı. Fırın eksen kaçıklıkları ancak refrakter tuğlalar dökülüp gövde kızardığında fark edilebilmekteydi.\n\nAr-Ge çalışması, çoklu lazer ve yüksek frekanslı sinyal füzyonu ile eksenel sapmaları erken aşamada tespit eder.",
            NonTechnicalDescription: "Pelet fabrikamızdaki dev döner fırınların yamulmasını veya çatlamasını önlemek için geliştirilmekte olan yüksek teknolojili bir erken uyarı ve lazerli ölçüm projesidir.\n\nFırının dönüşündeki en ufak bir eksen kaymasını anında yakalayarak fırın tuğlalarının dökülmesini önlemeyi amaçlar.",
            TechnicalDescription: "Sistem mimarisi; Rust dili ile geliştirilmiş ultra düşük gecikmeli bir gömülü sinyal işleme çekirdeği (embedded DSP core), InfluxDB zaman serisi deposu ve PyTorch anomali tespit servisinden oluşur. Lazer mesafe sensörlerinden gelen yüksek hızlı seri veri akışı Rust çekirdeğinde ring buffer üzerinde işlenir.\n\nSinyal hattında dijital Kalman filtreleme ve dalgacık dönüşümü (Wavelet Transform) uygulanarak fırın turu bazında dinamik ovalleşme profili (crank & ovality profile) çıkarılır. Elde edilen anomali skorları merkezi Ar-Ge sunucusuna taşınır.\n\nBu kayıt şu anda Ar-Ge planlama aşamasında olup henüz kütüphanede yayınlanmamış taslak (Draft) statüsündedir; yetkilendirme ve güvenlik testleri için taslak referans olarak tutulmaktadır.",
            BusinessImpact: "Peletleme fırınlarında refrakter tuğla dökülme ve fırın duruş risklerini azaltmaya yönelik yeni nesil Ar-Ge yetkinliği kazandırır.",
            TargetAudience: "Ar-Ge Mühendisleri, Peletleme Tesisi Bakım Şefleri ve İleri Titreşim Analistleri.",
            AccessInstructions: "Henüz taslak aşamasındadır. Yalnızca Ar-Ge proje yöneticileri ve sistem yöneticileri (Admin) tarafından görüntülenebilir.",
            CoverTheme: "excavator",
            CategoryCode: "RD",
            StatusCode: "PLANNING",
            DevelopmentType: DevelopmentType.Internal,
            IsFeatured: false,
            IsPublished: false,
            ApprovalStatus: ProjectApprovalStatus.Draft,
            StartDate: new DateOnly(2026, 1, 15),
            EndDate: null,
            CreatedAt: new DateTime(2025, 11, 15, 14, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2025, 11, 15, 14, 0, 0, DateTimeKind.Utc),
            CreatedByUserId: 1,
            ReviewedByUserId: null,
            LocationNames: new[] { "Malatya Hekimhan Peletleme Tesisi", "İstanbul Ar-Ge Merkezi" },
            TechnologyNames: new[] { "Python / PyTorch", "Rust Engine", "InfluxDB" },
            TeamNames: new[] { "Kestirimci Bakım ve Güvenilirlik Ekibi", "Yapay Zeka ve Görüntü İşleme Ekibi" },
            TagNames: new[] { "Kestirimci Bakım", "Yapay Zeka", "Otomasyon", "Endüstri 4.0" },
            Integrations: new (string, string, IntegrationType)[]
            {
                ("Yüksek Sıcaklık Lazer Sensör Arayüzü", "RS-485 seri hat üzerinden saniyede 50k örnekleme ile mesafe profili", IntegrationType.ExternalService)
            },
            Members: new (string, string)[]
            {
                ("Volkan Tekin", "Gömülü Sistemler & Rust Geliştirici"),
                ("Murat Koç", "Titreşim Analiz Danışmanı"),
                ("Zeynep Şahin", "Yapay Zeka Araştırmacısı")
            },
            MediaItems: Array.Empty<(string, string, string)>(),
            Documents: new (string, string, string, string)[]
            {
                ("Döner Fırın Dinamik Ölçüm Ar-Ge Planı", "Doner_Firin_ArGe_Plani.pdf", "TechnicalSpec", "Lazer sensör dizilimi ve sinyal işleme matematik modeli")
            }
        )
    };
}
