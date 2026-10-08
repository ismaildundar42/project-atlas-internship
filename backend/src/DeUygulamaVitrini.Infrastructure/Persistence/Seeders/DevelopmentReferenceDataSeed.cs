using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Seeders;

/// <summary>
/// Development ortamı için referans verileri (Departmanlar, Ekipler, Üyeler, Lokasyonlar, Teknolojiler, Etiketler)
/// idempotent (çoğaltmasız) olarak tohumlar.
/// Yalnızca geliştirme ve demo amaçlıdır.
/// </summary>
public static class DevelopmentReferenceDataSeed
{
    public static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        // ─── 1. Departmanlar ────────────────────────────────────────────────────────
        var departments = new (string Name, string Description)[]
        {
            ("Ar-Ge ve Dijital Dönüşüm", "Ar-Ge teknolojileri, yazılım ve dijital dönüşüm çözümleri"),
            ("Saha Operasyonları", "Maden sahaları operasyon ve üretim direktörlüğü"),
            ("Maden Planlama ve Jeoloji Direktörlüğü", "Açık ocak planlama, jeolojik modelleme ve sondaj analizleri"),
            ("İSG ve Sürdürülebilirlik Direktörlüğü", "İş sağlığı, saha emniyeti, çevre ve karbon salım yönetimi"),
            ("Tesis ve Bakım Yönetimi Direktörlüğü", "Kırıcı-zenginleştirme tesisleri ve ağır iş makinesi bakım direktörlüğü"),
            ("Tedarik Zinciri ve Lojistik Direktörlüğü", "Cevher sevkiyatı, filo yönetimi, kantar ve liman operasyonları"),
            ("Bilgi Teknolojileri ve Siber Güvenlik", "Kurumsal ağ, bulut altyapısı, ERP ve siber güvenlik sistemleri")
        };

        foreach (var (name, desc) in departments)
        {
            if (!await context.Departments.AnyAsync(d => d.Name == name))
            {
                context.Departments.Add(new Department { Name = name, Description = desc });
            }
        }
        await context.SaveChangesAsync();

        var deptDict = await context.Departments.ToDictionaryAsync(d => d.Name, d => d.Id);

        // ─── 2. Ekipler ────────────────────────────────────────────────────────────
        var teams = new (string Name, string Description, string DeptName)[]
        {
            ("Yazılım Geliştirme Ekibi", "Kurum içi web, mobil ve API yazılım çözümleri", "Ar-Ge ve Dijital Dönüşüm"),
            ("Veri Analitiği Ekibi", "Büyük veri analitiği, iş zekası ve raporlama çözümleri", "Ar-Ge ve Dijital Dönüşüm"),
            ("Yapay Zeka ve Görüntü İşleme Ekibi", "Derin öğrenme, computer vision ve kestirimci modeller", "Ar-Ge ve Dijital Dönüşüm"),
            ("IoT ve Otomasyon Ekibi", "Saha sensörleri, telemetri gateway ve SCADA entegrasyonu", "Saha Operasyonları"),
            ("Saha Teknolojileri ve İletişim Ekibi", "Maden içi telsiz, özel LTE, baz istasyonu ve GNSS altyapısı", "Saha Operasyonları"),
            ("Süreç Otomasyonu ve Robotik Ekibi", "Kırıcı-değirmen otomasyonu ve robotik numune alma", "Saha Operasyonları"),
            ("CBS ve Jeolojik Modelleme Ekibi", "3D blok modelleme, fotogrametri ve jeofizik analizler", "Maden Planlama ve Jeoloji Direktörlüğü"),
            ("İSG Dijital Takip Ekibi", "Akıllı KKD, yalnız çalışan takibi ve emniyet sensörleri", "İSG ve Sürdürülebilirlik Direktörlüğü"),
            ("Çevre ve Enerji Yönetimi Ekibi", "Reaktif güç, emisyon, toz bastırma ve su kalitesi takibi", "İSG ve Sürdürülebilirlik Direktörlüğü"),
            ("Kestirimci Bakım ve Güvenilirlik Ekibi", "Titreşim, ultrasonik ve yağ analizleriyle plansız duruş önleme", "Tesis ve Bakım Yönetimi Direktörlüğü"),
            ("Lojistik ve Filo Optimizasyonu Ekibi", "Kamyon rotalama, kantar otomasyonu ve demiryolu sevkiyatı", "Tedarik Zinciri ve Lojistik Direktörlüğü"),
            ("Kurumsal Uygulamalar ve ERP Ekibi", "SAP, LIMS, İK ve kurumsal veri entegrasyonları", "Bilgi Teknolojileri ve Siber Güvenlik")
        };

        foreach (var (name, desc, deptName) in teams)
        {
            if (!await context.Teams.AnyAsync(t => t.Name == name) && deptDict.TryGetValue(deptName, out var deptId))
            {
                context.Teams.Add(new Team { Name = name, Description = desc, DepartmentId = deptId });
            }
        }
        await context.SaveChangesAsync();

        // ─── 3. Üyeler (Tamamen Fictional) ─────────────────────────────────────────
        var teamDict = await context.Teams.ToDictionaryAsync(t => t.Name, t => t.Id);

        var members = new (string FirstName, string LastName, string Title, string Email, string TeamName)[]
        {
            ("Ahmet", "Yılmaz", "Kıdemli Yazılım Mimarı", "ahmet.yilmaz@fictional-demirexport.com", "Yazılım Geliştirme Ekibi"),
            ("Ayşe", "Kaya", "Veri Bilimci", "ayse.kaya@fictional-demirexport.com", "Veri Analitiği Ekibi"),
            ("Mehmet", "Demir", "IoT Sistem Mühendisi", "mehmet.demir@fictional-demirexport.com", "IoT ve Otomasyon Ekibi"),
            ("Zeynep", "Şahin", "Proje Yöneticisi", "zeynep.sahin@fictional-demirexport.com", "Kurumsal Uygulamalar ve ERP Ekibi"),
            ("Burak", "Aydın", "Görüntü İşleme Araştırmacısı", "burak.aydin@fictional-demirexport.com", "Yapay Zeka ve Görüntü İşleme Ekibi"),
            ("Deniz", "Arslan", "Kıdemli Backend Geliştirici", "deniz.arslan@fictional-demirexport.com", "Yazılım Geliştirme Ekibi"),
            ("Selin", "Yıldız", "Jeoloji Yüksek Mühendisi", "selin.yildiz@fictional-demirexport.com", "CBS ve Jeolojik Modelleme Ekibi"),
            ("Can", "Özkan", "Otomasyon & PLC Lideri", "can.ozkan@fictional-demirexport.com", "Süreç Otomasyonu ve Robotik Ekibi"),
            ("Elif", "Demir", "İSG Dijital Çözüm Uzmanı", "elif.demir@fictional-demirexport.com", "İSG Dijital Takip Ekibi"),
            ("Murat", "Koç", "Kestirimci Bakım Mühendisi", "murat.koc@fictional-demirexport.com", "Kestirimci Bakım ve Güvenilirlik Ekibi"),
            ("Ebru", "Çelik", "Çevre & Enerji Mühendisi", "ebru.celik@fictional-demirexport.com", "Çevre ve Enerji Yönetimi Ekibi"),
            ("Onur", "Şimşek", "Lojistik & Filo Operasyon Uzmanı", "onur.simsek@fictional-demirexport.com", "Lojistik ve Filo Optimizasyonu Ekibi"),
            ("Gamze", "Aslan", "Kurumsal ERP Entegratörü", "gamze.aslan@fictional-demirexport.com", "Kurumsal Uygulamalar ve ERP Ekibi"),
            ("Emre", "Polat", "Telemetri & Ağ Uzmanı", "emre.polat@fictional-demirexport.com", "Saha Teknolojileri ve İletişim Ekibi"),
            ("Büşra", "Kurt", "Frontend Geliştirici", "busra.kurt@fictional-demirexport.com", "Yazılım Geliştirme Ekibi"),
            ("Serkan", "Öztürk", "Maden Planlama Uzmanı", "serkan.ozturk@fictional-demirexport.com", "CBS ve Jeolojik Modelleme Ekibi"),
            ("Derya", "Aksoy", "Mobil Uygulama Geliştirici", "derya.aksoy@fictional-demirexport.com", "Yazılım Geliştirme Ekibi"),
            ("Tolga", "Bulut", "DevOps & Bulut Mühendisi", "tolga.bulut@fictional-demirexport.com", "Kurumsal Uygulamalar ve ERP Ekibi"),
            ("Gizem", "Keskin", "İş Zekası & Power BI Uzmanı", "gizem.keskin@fictional-demirexport.com", "Veri Analitiği Ekibi"),
            ("Volkan", "Tekin", "Gömülü Sistemler Geliştiricisi", "volkan.tekin@fictional-demirexport.com", "IoT ve Otomasyon Ekibi"),
            ("Melis", "Güler", "Saha Test & Devreye Alma Mühendisi", "melis.guler@fictional-demirexport.com", "Saha Teknolojileri ve İletişim Ekibi"),
            ("Hakan", "Yavuz", "Kimyasal Analiz & LIMS Uzmanı", "hakan.yavuz@fictional-demirexport.com", "Veri Analitiği Ekibi"),
            ("Pınar", "Doğan", "Veri Tabanı Yöneticisi (DBA)", "pinar.dogan@fictional-demirexport.com", "Kurumsal Uygulamalar ve ERP Ekibi"),
            ("Cemal", "Çetin", "Mekanik Güvenilirlik Şefi", "cemal.cetin@fictional-demirexport.com", "Kestirimci Bakım ve Güvenilirlik Ekibi"),
            ("Sinem", "Erdem", "Teknik Dokümantasyon Sorumlusu", "sinem.erdem@fictional-demirexport.com", "Yazılım Geliştirme Ekibi"),
            ("Kerem", "Vural", "İHA & Drone Operatörü", "kerem.vural@fictional-demirexport.com", "CBS ve Jeolojik Modelleme Ekibi")
        };

        foreach (var (first, last, title, email, teamName) in members)
        {
            var targetTeamId = teamDict.TryGetValue(teamName, out var tid) ? (int?)tid : null;
            var existing = await context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (existing == null)
            {
                context.Members.Add(new Member
                {
                    FirstName = first,
                    LastName = last,
                    Title = title,
                    Email = email,
                    TeamId = targetTeamId
                });
            }
            else if (existing.TeamId == null && targetTeamId.HasValue)
            {
                existing.TeamId = targetTeamId;
            }
        }
        await context.SaveChangesAsync();

        // ─── 4. Lokasyonlar (Fictional / Generic Demo Sahaları) ─────────────────────
        var locations = new (string Name, string Description, LocationType Type)[]
        {
            ("Kangallı Altın Sahası", "Sivas Kangal açık ocak ve liç işletmesi", LocationType.MineSite),
            ("Divriği Demir Sahası", "Sivas Divriği manyetit madeni ve zenginleştirme tesisi", LocationType.MineSite),
            ("Genel Müdürlük (Ankara)", "Merkez ofis ve ana veri merkezi", LocationType.Office),
            ("Balıkesir Manyas Sahası", "Balıkesir Manyas bentonit ve cevher sahası", LocationType.MineSite),
            ("Sivas Kangal Zenginleştirme Tesisi", "Flotasyon, kırıcı ve filtre pres tesisi", LocationType.Facility),
            ("Malatya Hekimhan Peletleme Tesisi", "Demir cevheri peletleme ve kurutma fırınları tesisi", LocationType.Facility),
            ("İzmir Liman ve Lojistik Merkezi", "Cevher ihracat depolama ve vagon kantar terminali", LocationType.Facility),
            ("İstanbul Ar-Ge Merkezi", "Yazılım, IoT laboratuvarı ve inovasyon merkezi", LocationType.Office),
            ("Eskişehir Mihalıççık Sahası", "Krom ve yardımcı mineral ocak operasyonları", LocationType.MineSite),
            ("Kayseri Develi Lojistik Sahası", "Merkezi maden araç bakım ve ikmal deposu", LocationType.Facility)
        };

        foreach (var (name, desc, type) in locations)
        {
            if (!await context.Locations.AnyAsync(l => l.Name == name))
            {
                context.Locations.Add(new Location
                {
                    Name = name,
                    Description = desc,
                    LocationType = type
                });
            }
        }
        await context.SaveChangesAsync();

        // ─── 5. Teknolojiler ───────────────────────────────────────────────────────
        var technologies = new (string Name, TechnologyCategory Category)[]
        {
            (".NET 9", TechnologyCategory.Backend),
            ("ASP.NET Core", TechnologyCategory.Backend),
            ("Node.js", TechnologyCategory.Backend),
            ("Python / PyTorch", TechnologyCategory.AI),
            ("Rust Engine", TechnologyCategory.Backend),
            ("React", TechnologyCategory.Frontend),
            ("Typescript", TechnologyCategory.Frontend),
            ("Next.js", TechnologyCategory.Frontend),
            ("Vue.js", TechnologyCategory.Frontend),
            ("Flutter", TechnologyCategory.Mobile),
            ("React Native", TechnologyCategory.Mobile),
            (".NET MAUI", TechnologyCategory.Mobile),
            ("SQL Server", TechnologyCategory.Database),
            ("PostgreSQL", TechnologyCategory.Database),
            ("InfluxDB", TechnologyCategory.Database),
            ("Redis", TechnologyCategory.Database),
            ("MQTT / IoT Edge", TechnologyCategory.IoT),
            ("OPC UA", TechnologyCategory.IoT),
            ("LoRaWAN", TechnologyCategory.IoT),
            ("TensorFlow", TechnologyCategory.AI),
            ("OpenCV", TechnologyCategory.AI),
            ("Scikit-Learn", TechnologyCategory.AI),
            ("Power BI", TechnologyCategory.Analytics),
            ("Apache Spark", TechnologyCategory.Analytics),
            ("Docker / Kubernetes", TechnologyCategory.DevOps),
            ("GitHub Actions", TechnologyCategory.DevOps),
            ("Azure Cloud", TechnologyCategory.Cloud),
            ("Azure Functions", TechnologyCategory.Cloud)
        };

        foreach (var (name, category) in technologies)
        {
            if (!await context.Technologies.AnyAsync(t => t.Name == name))
            {
                context.Technologies.Add(new Technology { Name = name, Category = category });
            }
        }
        await context.SaveChangesAsync();

        // ─── 6. Etiketler ──────────────────────────────────────────────────────────
        var tags = new (string Name, string Slug)[]
        {
            ("Saha Yönetimi", "saha-yonetimi"),
            ("Yapay Zeka", "yapay-zeka"),
            ("IoT Sensör", "iot-sensor"),
            ("İş Güvenliği", "is-guvenligi"),
            ("Veri Analitiği", "veri-analitigi"),
            ("Otomasyon", "otomasyon"),
            ("Kestirimci Bakım", "kestirimci-bakim"),
            ("Enerji Verimliliği", "enerji-verimliligi"),
            ("Drone ve İHA", "drone-iha"),
            ("CBS ve Haritalama", "cbs-haritalama"),
            ("Lojistik ve Filo", "lojistik-filo"),
            ("Çevre ve Sürdürülebilirlik", "cevre-surdurulebilirlik"),
            ("Görüntü İşleme", "goruntu-isleme"),
            ("Mobil Çözümler", "mobil-cozumler"),
            ("Dijital İkiz", "dijital-ikiz"),
            ("Üretim Takibi", "uretim-takibi"),
            ("Cevher Kalite", "cevher-kalite"),
            ("SCADA ve PLC", "scada-plc"),
            ("ERP Entegrasyonu", "erp-entegrasyonu"),
            ("Vardiya Yönetimi", "vardiya-yonetimi"),
            ("Telemetri", "telemetri"),
            ("Bulut Bilişim", "bulut-bilisim"),
            ("Siber Güvenlik", "siber-guvenlik"),
            ("Jeolojik Modelleme", "jeolojik-modelleme")
        };

        foreach (var (name, slug) in tags)
        {
            if (!await context.Tags.AnyAsync(t => t.Slug == slug))
            {
                context.Tags.Add(new Tag { Name = name, Slug = slug });
            }
        }
        await context.SaveChangesAsync();
    }
}
