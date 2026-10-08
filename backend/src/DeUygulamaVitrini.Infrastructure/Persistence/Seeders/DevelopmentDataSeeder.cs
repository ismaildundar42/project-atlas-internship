using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Domain.Enums;
using DeUygulamaVitrini.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Seeders;

/// <summary>
/// Yalnızca Development ortamında çalışan zengin örnek veri tohumlayıcısı (Seeder).
/// Production ortamında ASLA çalışmaz. İdempotent'tir (tekrar çalıştırıldığında veri çoğaltmaz).
/// Mevcut elle oluşturulmuş projeleri kesinlikle silmez, korur.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetService<IConfiguration>();
        var seedDemoProjects = configuration?.GetValue<bool>("DevelopmentData:SeedDemoProjects", false) ?? false;

        // ─── 0. Identity Roller ve Geliştirici SuperAdmin Kullanıcısı ───────────────────
        await SeedIdentityAsync(scope.ServiceProvider);

        // ─── 1. Referans Verileri (Departman, Ekip, Üye, Lokasyon, Teknoloji, Etiket) ──
        await DevelopmentReferenceDataSeed.SeedReferenceDataAsync(context);

        var seedCuratedProjects = configuration?.GetValue<bool>("DevelopmentData:SeedCuratedProjects", true) ?? true;
        if (seedCuratedProjects && !seedDemoProjects)
        {
            var loggerFactory = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("CuratedProjectDatasetSeeder") 
                         ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            var curatedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await CuratedProjectDatasetSeeder.SeedAsync(context, curatedUserManager, logger);
        }

        if (seedDemoProjects)
        {
            // ─── 2. Lookup Sözlükleri ──────────────────────────────────────────────────────
            var statuses = await context.ProjectStatuses.ToDictionaryAsync(s => s.Code, s => s.Id);
            var categories = await context.ProjectCategories.ToDictionaryAsync(c => c.Code, c => c.Id);
            var teams = await context.Teams.ToDictionaryAsync(t => t.Name, t => t.Id);
            var members = await context.Members
                .Where(m => m.Email != null)
                .ToDictionaryAsync(m => m.Email!, m => m);
            var locations = await context.Locations.ToDictionaryAsync(l => l.Name, l => l.Id);
            var technologies = await context.Technologies.ToDictionaryAsync(t => t.Name, t => t.Id);
            var tags = await context.Tags.ToDictionaryAsync(t => t.Slug, t => t.Id);

        // ─── 3. Mevcut Projeleri İncele (İdempotentlik ve URL İyileştirme) ─────────────
        var existingProjects = await context.Projects
            .IgnoreQueryFilters()
            .Include(p => p.ProjectMediaItems)
            .Include(p => p.ProjectDocuments)
            .ToListAsync();

        var existingSlugs = existingProjects.Select(p => p.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Mevcut projelerdeki eski picsum/örnek URL'leri yerel demo varlıklarına güncelle
        var demoCoverMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["saha-veri-takip-sistemi"] = "/uploads/projects/demo/mobile-field-worker.svg",
            ["drone-jeofizik-analizi"] = "/uploads/projects/demo/drone-surveying.svg",
            ["enerji-izleme-platformu"] = "/uploads/projects/demo/energy-monitoring.svg",
            ["entegre-lojistik-cozumu"] = "/uploads/projects/demo/haul-truck.svg",
            ["akilli-bakim-tahmin-sistemi"] = "/uploads/projects/demo/predictive-maintenance.svg",
            ["operasyon-analitik-platformu"] = "/uploads/projects/demo/data-analytics.svg",
            ["gizli-otonom-kamyon-taslagi"] = "/uploads/projects/demo/industrial-automation.svg"
        };

        foreach (var proj in existingProjects)
        {
            if (demoCoverMap.TryGetValue(proj.Slug, out var localCover))
            {
                if (string.IsNullOrEmpty(proj.CoverImageUrl) || proj.CoverImageUrl.Contains("picsum"))
                {
                    proj.CoverImageUrl = localCover;
                }
            }
            foreach (var m in proj.ProjectMediaItems)
            {
                if (m.FileUrl.Contains("picsum") || m.FileUrl.Contains("example.com"))
                {
                    m.FileUrl = localCover ?? "/uploads/projects/demo/open-pit-mine.svg";
                }
            }
            foreach (var d in proj.ProjectDocuments)
            {
                if (d.FileUrl.Contains("example.com"))
                {
                    d.FileUrl = "/uploads/docs/teknik-mimari-ozeti.pdf";
                }
            }

            // ─── Orijinal Demo Projelerinin Açıklamalarını Zenginleştir ───────────────
            if (proj.Slug.Equals("saha-veri-takip-sistemi", StringComparison.OrdinalIgnoreCase))
            {
                proj.Purpose = "Açık ocak sahalarında görev yapan onlarca ağır iş makinesinin (ekskavatör, loder, kaya kamyonu, dozer) ve saha operasyon personelinin anlık konum, çalışma durumu, yakıt seviyesi ve tonaj verilerini merkezî bir dijital harita üzerinde toplamak; vardiya içi döngü sürelerini optimize ederek maden üretim hızını ve güvenliğini artırmaktır.";
                proj.ProblemSolved = "Geleneksel maden yönetiminde kamyon sefer sayıları ve ekskavatör kazı miktarları vardiya sonunda kantar fişleri ve operatörlerin kağıt formlara yazdığı notlardan Excel'e aktarılmaktaydı. Bu gecikmeli raporlama nedeniyle gün içinde bir kamyonun rölantide beklemesi, ekskavatör önünde kuyruk oluşması veya hatalı pasa dökümü gibi verimsizlikler ancak ertesi gün fark edilebiliyor, operasyona anında müdahale edilemiyordu.";
                proj.NonTechnicalDescription = "Saha araçlarının kabinlerine yerleştirilen endüstriyel tabletler ve GPS cihazları ile sahadaki tüm hareket canlı bir dijital haritada gösterilir:\n\n• Canlı İzleme: Yöneticiler ve kontrol odası, hangi kamyonun hangi aynadan yük aldığını, hangi hızla gittiğini ve nereye boşalttığını harita üzerinde canlı izler.\n• Akıllı Sevk & Yönlendirme: Bir kırıcıda arıza çıktığında sistem kamyonları otomatik olarak alternatif boşaltma bunkerine veya geçici stok sahasına yönlendirerek kamyonların boşuna beklemesini önler.\n• Kağıtsız Vardiya: Vardiya sonu raporları tek tuşla sistemden alınır; operasyon şefleri hangi ekibin ne kadar ton cevher taşıdığını gerçek zamanlı takip eder.";
                proj.BusinessImpact = "• Kamyon rölanti ve bekleme sürelerini vardiya başına 42 dakika azaltarak filo yakıt verimliliğini %8 artırmıştır.\n• Ekskavatör kepçe doluluk oranları ve günlük tonaj verileri %100 doğrulukla ERP sistemine anlık akar.";
            }
            else if (proj.Slug.Equals("drone-jeofizik-analizi", StringComparison.OrdinalIgnoreCase))
            {
                proj.Purpose = "Otonom uçuş kabiliyetine sahip endüstriyel İHA'lar (drone), LiDAR tarayıcılar ve yüksek çözünürlüklü fotogrametri kameraları kullanarak açık ocak maden basamaklarının, dekapaj sahalarının ve cevher stok yığınlarının 3 boyutlu sayısal arazi modellerini (DEM/DTM) milimetrik doğrulukta üretmek; hafriyat hacim hesaplamalarını ve jeofizik arazi haritalamasını saatler mertebesinde tamamlamaktır.";
                proj.ProblemSolved = "Geleneksel haritacılık yöntemlerinde topoğraf ekipleri sarp ve tehlikeli şev yamaçlarında, dik stok yığınlarının üzerinde GPS çubuklarıyla günlerce yürüyerek nokta toplamak zorundaydı. Bu süreç hem saha çalışanları için ciddi düşme ve taş yuvarlanma riskleri barındırmakta hem de binlerce dönümlük bir maden sahasının haritasının çıkarılması 1-2 hafta sürdüğü için aylık hakediş ve üretim hacim hesapları gecikmekteydi.";
                proj.NonTechnicalDescription = "Drone'lar sahanın üzerinden otonom uçarak binlerce hava fotoğrafı ve lazer tarama noktası çeker:\n\n• 3 Boyutlu Dijital Arazi: Bilgisayar bu fotoğrafları birleştirerek madenin tam bir 3D dijital kopyasını oluşturur.\n• Otomatik Tonaj Hesaplama: Stok sahasındaki demir veya pasa tepeciğinin kaç metreküp ve kaç ton olduğu, insan eli değmeden 15 dakika içinde milimetrik doğrulukla hesaplanır.\n• Emniyetli Haritacılık: Harita mühendisleri uçurum kenarlarına veya hareketli şevlere tırmanmak zorunda kalmadan ofislerinden tüm sahanın topografyasını günceller.";
                proj.BusinessImpact = "• Açık ocak kübaj ve stok ölçüm sürelerini 12 günden 4 saate indirmiştir.\n• Tehlikeli şev kenarlarında insanla harita ölçümü gereksinimini %90 azaltarak iş güvenliği standartlarını yükseltmiştir.";
            }
            else if (proj.Slug.Equals("enerji-izleme-platformu", StringComparison.OrdinalIgnoreCase))
            {
                proj.Purpose = "Kırma, eleme, değirmen, flotasyon, pompa istasyonları ve kompresör daireleri gibi yüksek elektrik tüketimine sahip tüm tesis ünitelerindeki enerji tüketimini trafo ve pano bazında saniyelik çözünürlükte izlemek; reaktif güç cezalarını önlemek, anlık güç sıçramalarını (demand aşımı) dengelemek ve ton cevher başına düşen spesifik kWh tüketimini düşürmektir.";
                proj.ProblemSolved = "Fabrika genelinde yalnızca ay sonunda gelen tek bir elektrik faturası üzerinden maliyet takibi yapılabiliyordu. Hangi değirmenin boşta dönerken yüksek enerji çektiği, hangi pompanın verimsiz çalıştığı veya reaktif güç kompanzasyonundaki arızalar anlaşılamadığı için her ay on binlerce liralık ceza ve enerji kaybı yaşanmaktaydı.";
                proj.NonTechnicalDescription = "Tesis genelindeki yüzlerce elektrik sayacına takılan akıllı modüller, fabrikanın enerji nabzını tutar:\n\n• Bölüm Bazlı Tüketim: Hangi kırıcının, hangi bandın veya hangi binanın ne kadar elektrik yaktığı anlık grafiklerle izlenir.\n• Erken Kaçak & Ceza Uyarısı: Sistem reaktif güç sınırına yaklaşıldığında veya bir motor normalden fazla akım çektiğinde bakım ekibine otomatik uyarı mesajı gönderir.\n• Enerji Verimlilik Önerisi: Fabrika yöneticilerine 'Kırıcı 2 boşta 45 dakikadır çalışıyor, kapatılırsa saatte 180 kWh tasarruf sağlanabilir' şeklinde akıllı bildirimler sunar.";
                proj.BusinessImpact = "• Reaktif güç cezalarını tamamen sıfırlamıştır.\n• Tesis spesifik enerji tüketimini (kWh/ton) %7.4 düşürerek yıllık ~185.000 USD elektrik maliyeti tasarrufu sağlamıştır.";
            }
            else if (proj.Slug.Equals("akilli-bakim-tahmin-sistemi", StringComparison.OrdinalIgnoreCase))
            {
                proj.Purpose = "Tesis kırırcıları, bilyeli değirmenler, ana konveyör tahrikleri ve maden ekskavatörleri gibi yüksek tonajlı kritik ekipmanların rulman, dişli kutusu ve motor bileşenlerini titreşim, sıcaklık ve yağ partikül sensörleri ile sürekli izlemek; olası mekanik aşınma ve yorulmaları arıza meydana gelmeden 15 ila 30 gün öncesinde kestirimci yapay zeka modelleriyle tespit ederek plansız üretim duruşlarını engellemektir.";
                proj.ProblemSolved = "Periyodik zaman bazlı bakımlarda parçalar ya gereğinden erken değiştirilerek yüksek yedek parça maliyeti yaratılmakta ya da planlanan bakım tarihinden önce aniden bozularak tüm fabrikayı durdurmaktaydı. Özellikle değirmen pinyon dişlisi veya kırıcı ana şaft rulmanı gibi tedarik süresi aylar süren kritik parçaların aniden kırılması, tesisin haftalarca devre dışı kalmasına ve yüz binlerce dolarlık plansız duruş maliyetine neden olmaktaydı.";
                proj.NonTechnicalDescription = "Makinelerin gövdesine takılan kablosuz akıllı sensörler, bir doktorun stetoskopla kalp dinlemesi gibi makinelerin iç sesini ve titreşimini 7/24 dinler:\n\n• Anomali Teşhisi: Yapay zeka modeli, makinelerin normal çalışma sesini öğrenir; rulmanda gözle görülmeyen bir mikro çatlak veya yağsızlık başladığında titreşimdeki değişimi hemen yakalar.\n• Kalan Ömür Tahmini: Sistem bakım mühendislerine '3 numaralı kırıcının ana rulmanı mevcut çalışma temposuyla 18 gün sonra arızalanacak; gelecek hafta planlı revizyonda değiştirilmelidir' şeklinde parça koduyla birlikte net iş emri üretir.";
                proj.BusinessImpact = "• Yılda ortalama 140 saatlik plansız duruşu önleyerek üretim sürekliliğini teminat altına almıştır.\n• Arıza öncesi müdahale sayesinde büyük revizyon masraflarında yıllık ~340.000 USD tasarruf sağlamıştır.";
            }
        }
        await context.SaveChangesAsync();

        // ─── 4. Yeni Zengin Projeleri Ekle ve Mevcutları Zenginleştir ─────────────
        var blueprints = DevelopmentProjectSeedDefinitions.GetBlueprints();
        var newProjects = new List<Project>();

        foreach (var bp in blueprints)
        {
            var existingProject = existingProjects.FirstOrDefault(p => p.Slug.Equals(bp.Slug, StringComparison.OrdinalIgnoreCase));
            if (existingProject != null)
            {
                // Mevcut kayıtları zenginleştirilmiş yeni metinlerle güncelle (İdempotent güncelleme)
                existingProject.Name = bp.Name;
                existingProject.ShortDescription = bp.ShortDescription;
                existingProject.Description = bp.Description;
                existingProject.Purpose = bp.Purpose;
                existingProject.ProblemSolved = bp.ProblemSolved;
                existingProject.NonTechnicalDescription = bp.NonTechnicalDescription;
                existingProject.TechnicalDescription = bp.TechnicalDescription;
                existingProject.BusinessImpact = bp.BusinessImpact;
                existingProject.TargetAudience = bp.TargetAudience;
                continue;
            }

            if (!statuses.TryGetValue(bp.StatusCode, out var statusId))
            {
                statusId = statuses.GetValueOrDefault("ACTIVE", 1);
            }

            if (!categories.TryGetValue(bp.CategoryCode, out var categoryId))
            {
                categoryId = categories.GetValueOrDefault("SOFTWARE", 1);
            }

            var coverUrl = !string.IsNullOrWhiteSpace(bp.CoverImageTheme)
                ? $"/uploads/projects/demo/{bp.CoverImageTheme}.svg"
                : "/uploads/projects/demo/open-pit-mine.svg";

            var project = new Project
            {
                Name = bp.Name,
                Slug = bp.Slug,
                ShortDescription = bp.ShortDescription,
                Description = bp.Description,
                Purpose = bp.Purpose,
                ProblemSolved = bp.ProblemSolved,
                NonTechnicalDescription = bp.NonTechnicalDescription,
                TechnicalDescription = bp.TechnicalDescription,
                BusinessImpact = bp.BusinessImpact,
                TargetAudience = bp.TargetAudience,
                StatusId = statusId,
                CategoryId = categoryId,
                DevelopmentType = bp.DevelopmentType,
                StartDate = bp.StartDate,
                EndDate = bp.EndDate,
                IsFeatured = bp.IsFeatured,
                IsPublished = bp.IsPublished,
                CoverImageUrl = coverUrl
            };

            // Ekipler
            if (teams.TryGetValue(bp.PrimaryTeamName, out var primaryTeamId))
            {
                project.ProjectTeams.Add(new ProjectTeam { TeamId = primaryTeamId, IsPrimary = true });
            }
            else if (teams.Count > 0)
            {
                project.ProjectTeams.Add(new ProjectTeam { TeamId = teams.Values.First(), IsPrimary = true });
            }

            if (!string.IsNullOrWhiteSpace(bp.SecondaryTeamName) && teams.TryGetValue(bp.SecondaryTeamName, out var secondaryTeamId) && secondaryTeamId != primaryTeamId)
            {
                project.ProjectTeams.Add(new ProjectTeam { TeamId = secondaryTeamId, IsPrimary = false });
            }

            // Üyeler
            int memberIndex = 0;
            foreach (var email in bp.MemberEmails)
            {
                if (members.TryGetValue(email, out var m))
                {
                    var role = memberIndex == 0 ? "Proje Yöneticisi" : (m.Title ?? "Teknik Uzman");
                    project.ProjectMembers.Add(new ProjectMember { MemberId = m.Id, ProjectRole = role });
                    memberIndex++;
                }
            }

            // Lokasyonlar
            foreach (var locName in bp.LocationNames)
            {
                if (locations.TryGetValue(locName, out var locId))
                {
                    project.ProjectLocations.Add(new ProjectLocation { LocationId = locId });
                }
            }

            // Teknolojiler
            foreach (var techName in bp.TechnologyNames)
            {
                if (technologies.TryGetValue(techName, out var techId))
                {
                    project.ProjectTechnologies.Add(new ProjectTechnology { TechnologyId = techId });
                }
            }

            // Etiketler
            foreach (var tagSlug in bp.TagSlugs)
            {
                if (tags.TryGetValue(tagSlug, out var tagId))
                {
                    project.ProjectTags.Add(new ProjectTag { TagId = tagId });
                }
            }

            // Entegrasyonlar
            foreach (var (intName, intType, intDesc) in bp.Integrations)
            {
                project.ProjectIntegrations.Add(new ProjectIntegration
                {
                    Name = intName,
                    IntegrationType = intType,
                    Description = intDesc
                });
            }

            // Galeri Görselleri (ProjectMedia)
            int displayOrder = 1;
            foreach (var (theme, title, caption) in bp.GalleryImages)
            {
                project.ProjectMediaItems.Add(new ProjectMedia
                {
                    MediaType = MediaType.Image,
                    FileName = $"{theme}.svg",
                    FileUrl = $"/uploads/projects/demo/{theme}.svg",
                    AltText = title,
                    Caption = caption,
                    DisplayOrder = displayOrder++
                });
            }

            // Dokümanlar (ProjectDocument)
            foreach (var (docName, docFile, docType, docDesc) in bp.Documents)
            {
                project.ProjectDocuments.Add(new ProjectDocument
                {
                    Name = docName,
                    FileName = docFile,
                    FileUrl = $"/uploads/docs/{docFile}",
                    DocumentType = docType,
                    Description = docDesc
                });
            }

            newProjects.Add(project);
            existingSlugs.Add(project.Slug);
        }

        if (newProjects.Count > 0)
        {
            context.Projects.AddRange(newProjects);
        }
        await context.SaveChangesAsync();
        }

        // ─── 5. Member ↔ ApplicationUser Deterministic Linking (Phase 13.2) ────────
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync("admin@demirexport.com");
        var creatorUser = await userManager.FindByEmailAsync("creator@demirexport.com");
        var normalUser = await userManager.FindByEmailAsync("user@demirexport.com");

        var allMembers = await context.Members.ToListAsync();

        if (adminUser != null)
        {
            var adminMember = allMembers.FirstOrDefault(m => m.FirstName == "Ahmet" && m.LastName == "Yılmaz");
            if (adminMember != null && adminMember.UserId == adminUser.Id)
            {
                adminMember.UserId = null;
            }
        }

        if (creatorUser != null)
        {
            var creatorMember = allMembers.FirstOrDefault(m => m.FirstName == "Ayşe" && m.LastName == "Kaya");
            if (creatorMember != null && creatorMember.UserId != creatorUser.Id)
            {
                creatorMember.UserId = creatorUser.Id;
            }
        }

        if (normalUser != null)
        {
            var normalMember = allMembers.FirstOrDefault(m => m.FirstName == "Zeynep" && m.LastName == "Şahin");
            if (normalMember != null && normalMember.UserId != normalUser.Id)
            {
                normalMember.UserId = normalUser.Id;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedIdentityAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<IApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync(AppRoles.SuperAdmin))
        {
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.SuperAdmin));
        }

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Admin));
        }

        var devUsers = new (string Email, string FirstName, string LastName, bool IsSuperAdmin, bool IsAdmin, bool CanCreateProjects, string Password)[]
        {
            ("admin@demirexport.com", "Admin", "", true, true, true, "AdminPassword123!"),
            ("superadmin@demirexport.com", "Sistem", "Yöneticisi", true, true, true, "AdminPassword123!"),
            ("admin_ops@demirexport.com", "Operasyonel", "Admin", false, true, true, "AdminPassword123!"),
            ("creator@demirexport.com", "Mehmet", "Kaya (Proje Girişi)", false, false, true, "CreatorPassword123!"),
            ("user@demirexport.com", "Zeynep", "Şahin (Normal Kullanıcı)", false, false, false, "UserPassword123!"),
            ("leader@demirexport.com", "Mehmet", "Kaya", false, false, true, "LeaderPassword123!"),
            ("contributor@demirexport.com", "Ayşe", "Demir", false, false, true, "ContributorPassword123!"),
            ("readonly@demirexport.com", "Zeynep", "Şahin", false, false, false, "ReadOnlyPassword123!")
        };

        foreach (var u in devUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(u.Email);
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CanCreateProjects = u.CanCreateProjects,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(newUser, u.Password);
                if (createResult.Succeeded)
                {
                    if (u.IsSuperAdmin)
                    {
                        await userManager.AddToRoleAsync(newUser, AppRoles.SuperAdmin);
                    }
                    if (u.IsAdmin)
                    {
                        await userManager.AddToRoleAsync(newUser, AppRoles.Admin);
                    }
                }
            }
            else
            {
                existingUser.FirstName = u.FirstName;
                existingUser.LastName = u.LastName;
                existingUser.CanCreateProjects = u.CanCreateProjects;
                await userManager.UpdateAsync(existingUser);

                if (u.IsSuperAdmin && !await userManager.IsInRoleAsync(existingUser, AppRoles.SuperAdmin))
                {
                    await userManager.AddToRoleAsync(existingUser, AppRoles.SuperAdmin);
                }

                if (u.IsAdmin && !await userManager.IsInRoleAsync(existingUser, AppRoles.Admin))
                {
                    await userManager.AddToRoleAsync(existingUser, AppRoles.Admin);
                }
            }
        }
    }
}
