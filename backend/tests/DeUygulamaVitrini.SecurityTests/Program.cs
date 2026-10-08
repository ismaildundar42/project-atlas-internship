using System.Security.Claims;
using System.Text;
using DeUygulamaVitrini.API.Controllers;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Application.DTOs.Projects;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using DeUygulamaVitrini.Infrastructure.Persistence;
using DeUygulamaVitrini.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace DeUygulamaVitrini.SecurityTests;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "DeUygulamaVitrini";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Testing";
}

public class StubProjectService : IProjectService
{
    public Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(ProjectQueryParameters parameters, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<ProjectListItemDto>(new List<ProjectListItemDto>(), 1, 10, 0));

    public Task<ProjectDetailDto?> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult<ProjectDetailDto?>(null);

    public Task<ProjectDetailDto?> GetProjectBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => Task.FromResult<ProjectDetailDto?>(null);
}

public class StubAiSummaryService : IProjectAiSummaryService
{
    public Task<ProjectAiSummaryResponseDto> GenerateSummaryAsync(int projectId, string? language, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
        => Task.FromResult(new ProjectAiSummaryResponseDto { ProjectId = projectId, ProjectName = "Test" });
}

public class Program
{
    private static int _passCount = 0;
    private static int _failCount = 0;

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=======================================================================");
        Console.WriteLine("SEC-001: PRIVATE PROJECT DOCUMENT ACCESS HARDENING SECURITY REGRESSION");
        Console.WriteLine("=======================================================================");

        try
        {
            // ─── 1. Target Database Isolation Verification ───────────────────────────
            TestDatabaseIsolationInvariant();

            // ─── 2. Storage Separation & Traversal Defense Tests ───────────────────────
            await TestStorageSeparationAndTraversalDefenseAsync();

            // ─── 3. Controller Authorization & Document Streaming Tests ───────────────
            await TestControllerAuthorizationAndStreamingAsync();

            // ─── 4. Mismatched Document & Missing File Safety Tests ───────────────────
            await TestMismatchedAndMissingFileSafetyAsync();

            Console.WriteLine("\n=======================================================================");
            Console.WriteLine($"SECURITY REGRESSION TEST RESULTS: {_passCount} PASSED, {_failCount} FAILED");
            Console.WriteLine("=======================================================================");

            return _failCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL RUNTIME EXCEPTION] {ex}");
            Console.ResetColor();
            return 1;
        }
    }

    private static void AssertTest(string testName, bool condition, string detail = "")
    {
        if (condition)
        {
            _passCount++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  [PASS] ");
            Console.ResetColor();
            Console.WriteLine(testName);
        }
        else
        {
            _failCount++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  [FAIL] ");
            Console.ResetColor();
            Console.WriteLine($"{testName} => {detail}");
        }
    }

    private static void TestDatabaseIsolationInvariant()
    {
        Console.WriteLine("\n--- 1. DATABASE ISOLATION VERIFICATION ---");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options, new DeUygulamaVitrini.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor());

        // Verify In-Memory database provider is active, not SQL Server DeUygulamaVitriniDb
        AssertTest(
            "Test execution uses isolated In-Memory provider (NOT DeUygulamaVitriniDb)",
            context.Database.ProviderName?.Contains("InMemory") == true,
            $"Expected InMemory provider, got: {context.Database.ProviderName}");
    }

    private static async Task TestStorageSeparationAndTraversalDefenseAsync()
    {
        Console.WriteLine("\n--- 2. STORAGE SEPARATION & PATH TRAVERSAL DEFENSE ---");

        var tempRoot = Path.Combine(Path.GetTempPath(), "DeUygulamaVitrini_SecTest_" + Guid.NewGuid().ToString("N"));
        var webRoot = Path.Combine(tempRoot, "wwwroot");
        var appDataRoot = Path.Combine(tempRoot, "App_Data");

        Directory.CreateDirectory(webRoot);
        Directory.CreateDirectory(appDataRoot);

        try
        {
            var env = new TestWebHostEnvironment
            {
                WebRootPath = webRoot,
                ContentRootPath = tempRoot
            };

            var storageService = new LocalFileStorageService(env);

            // A. Document Upload must go to App_Data (outside wwwroot)
            var sampleDocBytes = Encoding.UTF8.GetBytes("%PDF-1.4 sample private technical specification document content");
            using var docStream = new MemoryStream(sampleDocBytes);
            var docUploadResult = await storageService.SaveFileAsync(
                docStream,
                "private_architecture_v1.pdf",
                "application/pdf",
                "projects/10/documents",
                isPrivate: true);

            AssertTest(
                "Document upload marked as private",
                docUploadResult.IsPrivate);

            var physicalDocPath = storageService.GetPrivatePhysicalFilePath(docUploadResult.FileUrl);
            AssertTest(
                "Private document physical path resolved under App_Data/uploads (outside wwwroot)",
                !string.IsNullOrEmpty(physicalDocPath) &&
                physicalDocPath.StartsWith(Path.Combine(tempRoot, "App_Data")),
                $"Physical path: {physicalDocPath}");

            AssertTest(
                "Private document is NOT inside wwwroot",
                physicalDocPath != null && !physicalDocPath.StartsWith(webRoot));

            AssertTest(
                "Private document bytes exist and match original stream",
                physicalDocPath != null && File.Exists(physicalDocPath) &&
                File.ReadAllText(physicalDocPath) == "%PDF-1.4 sample private technical specification document content");

            // B. Public Media Upload must go to wwwroot (publicly accessible showcase media)
            var sampleImgBytes = Encoding.UTF8.GetBytes("<svg>sample open-pit-mine icon</svg>");
            using var imgStream = new MemoryStream(sampleImgBytes);
            var imgUploadResult = await storageService.SaveFileAsync(
                imgStream,
                "open-pit-mine.svg",
                "image/svg+xml",
                "projects/10/media",
                isPrivate: false);

            AssertTest(
                "Media upload marked as public (isPrivate: false)",
                !imgUploadResult.IsPrivate);

            var physicalImgPath = storageService.GetPhysicalFilePath(imgUploadResult.FileUrl, isPrivate: false);
            AssertTest(
                "Public media physical path resolved under wwwroot/uploads",
                !string.IsNullOrEmpty(physicalImgPath) &&
                physicalImgPath.StartsWith(webRoot),
                $"Physical path: {physicalImgPath}");

            // C. Path Traversal Defense on Relative Path Resolution
            var traversalAttempt1 = storageService.GetPrivatePhysicalFilePath("../../../appsettings.json");
            AssertTest(
                "Path traversal attempt (../../../appsettings.json) safely blocked and returned null",
                traversalAttempt1 == null,
                $"Expected null, got: {traversalAttempt1}");

            var traversalAttempt2 = storageService.GetPrivatePhysicalFilePath("..\\..\\..\\Windows\\win.ini");
            AssertTest(
                "Windows backslash traversal attempt (..\\..\\..\\win.ini) safely blocked and returned null",
                traversalAttempt2 == null,
                $"Expected null, got: {traversalAttempt2}");

            var traversalAttempt3 = storageService.GetPrivatePhysicalFilePath("/uploads/../../secret.txt");
            AssertTest(
                "Slash prefix traversal attempt (/uploads/../../secret.txt) safely blocked and returned null",
                traversalAttempt3 == null,
                $"Expected null, got: {traversalAttempt3}");

            // D. File Promotion for Temp Files
            var tempDocBytes = Encoding.UTF8.GetBytes("%PDF-1.4 temp uploaded document");
            using var tempDocStream = new MemoryStream(tempDocBytes);
            var tempUpload = await storageService.SaveFileAsync(
                tempDocStream,
                "temp_spec.pdf",
                "application/pdf",
                "projects/temp/documents",
                isPrivate: true);

            var promotedDocUrl = storageService.PromoteFile(tempUpload.FileUrl, 42, isPrivate: true);
            AssertTest(
                "PromoteFile updates temp URL to target projectId",
                promotedDocUrl.Contains("/projects/42/documents/"));

            var promotedPhysicalPath = storageService.GetPrivatePhysicalFilePath(promotedDocUrl);
            AssertTest(
                "Promoted file exists at new destination under App_Data",
                promotedPhysicalPath != null && File.Exists(promotedPhysicalPath));
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); } catch { }
        }
    }

    private static async Task TestControllerAuthorizationAndStreamingAsync()
    {
        Console.WriteLine("\n--- 3. CONTROLLER AUTHORIZATION & DOCUMENT STREAMING ---");

        var tempRoot = Path.Combine(Path.GetTempPath(), "DeUygulamaVitrini_CtrlTest_" + Guid.NewGuid().ToString("N"));
        var webRoot = Path.Combine(tempRoot, "wwwroot");
        var appDataRoot = Path.Combine(tempRoot, "App_Data");

        Directory.CreateDirectory(webRoot);
        Directory.CreateDirectory(appDataRoot);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options, new DeUygulamaVitrini.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor());

        // Seed Category & Status
        var category = new ProjectCategory { Id = 1, Name = "Yazılım", Code = "software" };
        var status = new ProjectStatus { Id = 1, Name = "Aktif", Code = "active" };
        context.ProjectCategories.Add(category);
        context.ProjectStatuses.Add(status);

        // Seed Published & Approved Project
        var pubProject = new Project
        {
            Id = 100,
            Name = "Saha Takip Sistemi",
            Slug = "saha-takip-sistemi",
            ShortDescription = "Açık ocak takip sistemi",
            StatusId = 1,
            CategoryId = 1,
            IsPublished = true,
            ApprovalStatus = ProjectApprovalStatus.Approved,
            CreatedByUserId = 10 // Owned by User 10
        };

        // Seed Draft / Unapproved Project
        var draftProject = new Project
        {
            Id = 200,
            Name = "Gizli Otonom Kamyon Taslağı",
            Slug = "gizli-otonom-kamyon-taslagi",
            ShortDescription = "Henüz onaylanmamış Ar-Ge taslağı",
            StatusId = 1,
            CategoryId = 1,
            IsPublished = false,
            ApprovalStatus = ProjectApprovalStatus.Draft,
            CreatedByUserId = 10 // Owned by User 10
        };

        context.Projects.AddRange(pubProject, draftProject);

        // Seed Documents
        var pubDoc = new ProjectDocument
        {
            Id = 501,
            ProjectId = 100,
            Name = "Saha Takip Teknik Mimari",
            FileName = "saha_takip_mimari.pdf",
            FileUrl = "/uploads/projects/100/documents/saha_takip_mimari.pdf",
            DocumentType = "application/pdf"
        };

        var draftDoc = new ProjectDocument
        {
            Id = 502,
            ProjectId = 200,
            Name = "Gizli Otonom Algoritma Raporu",
            FileName = "otonom_algoritma_gizli.pdf",
            FileUrl = "/uploads/projects/200/documents/otonom_algoritma_gizli.pdf",
            DocumentType = "application/pdf"
        };

        context.ProjectDocuments.AddRange(pubDoc, draftDoc);
        await context.SaveChangesAsync();

        var env = new TestWebHostEnvironment
        {
            WebRootPath = webRoot,
            ContentRootPath = tempRoot
        };

        var storageService = new LocalFileStorageService(env);

        // Write sample files to private storage
        var pubDocDiskDir = Path.Combine(appDataRoot, "uploads", "projects", "100", "documents");
        Directory.CreateDirectory(pubDocDiskDir);
        File.WriteAllText(Path.Combine(pubDocDiskDir, "saha_takip_mimari.pdf"), "%PDF-1.4 Saha Takip Mimari Content");

        var draftDocDiskDir = Path.Combine(appDataRoot, "uploads", "projects", "200", "documents");
        Directory.CreateDirectory(draftDocDiskDir);
        File.WriteAllText(Path.Combine(draftDocDiskDir, "otonom_algoritma_gizli.pdf"), "%PDF-1.4 Gizli Otonom Algoritma Content");

        var stubProjectService = new StubProjectService();
        var stubAiSummaryService = new StubAiSummaryService();

        var controller = new ProjectsController(
            stubProjectService,
            stubAiSummaryService,
            context,
            storageService);

        // A. Scenario 1: Authenticated Normal User (User 99) downloading Published & Approved Project Document
        SetControllerUser(controller, userId: 99, role: "User");
        var res1 = await controller.DownloadDocument(100, 501, CancellationToken.None);
        AssertTest(
            "Authenticated user downloading Published & Approved project document => 200 OK (FileStreamResult)",
            res1 is FileStreamResult);

        if (res1 is FileStreamResult fileRes1)
        {
            AssertTest(
                "Response has correct Content-Type: application/pdf",
                fileRes1.ContentType == "application/pdf");
            AssertTest(
                "Response has sanitized FileDownloadName: saha_takip_mimari.pdf",
                fileRes1.FileDownloadName == "saha_takip_mimari.pdf");
            AssertTest(
                "Response has X-Content-Type-Options: nosniff",
                controller.Response.Headers["X-Content-Type-Options"] == "nosniff");
            fileRes1.FileStream.Dispose();
        }

        // B. Scenario 2: Authenticated Normal User (User 99, NOT owner, NOT admin) downloading Draft Project Document
        SetControllerUser(controller, userId: 99, role: "User");
        var res2 = await controller.DownloadDocument(200, 502, CancellationToken.None);
        AssertTest(
            "Unauthorized user downloading Draft/Unapproved project document => 403 Forbidden",
            res2 is ObjectResult objRes && objRes.StatusCode == StatusCodes.Status403Forbidden);

        // C. Scenario 3: Anonymous User (UserId = 0, no auth) downloading Draft Project Document
        SetControllerUser(controller, userId: 0, role: "");
        var res3 = await controller.DownloadDocument(200, 502, CancellationToken.None);
        AssertTest(
            "Anonymous user attempting to access Draft project document => 403 Forbidden (or 401 at auth gate)",
            res3 is ObjectResult objRes3 && objRes3.StatusCode == StatusCodes.Status403Forbidden);

        // D. Scenario 4: Project Owner (User 10) downloading their own Draft Project Document
        SetControllerUser(controller, userId: 10, role: "User");
        var res4 = await controller.DownloadDocument(200, 502, CancellationToken.None);
        AssertTest(
            "Project Owner (Creator) downloading Draft project document => 200 OK (FileStreamResult)",
            res4 is FileStreamResult);
        if (res4 is FileStreamResult fileRes4)
        {
            fileRes4.FileStream.Dispose();
        }

        // E. Scenario 5: Admin User (User 999 with Admin role) downloading Draft Project Document
        SetControllerUser(controller, userId: 999, role: AppRoles.Admin);
        var res5 = await controller.DownloadDocument(200, 502, CancellationToken.None);
        AssertTest(
            "Admin user downloading Draft project document => 200 OK (FileStreamResult)",
            res5 is FileStreamResult);
        if (res5 is FileStreamResult fileRes5)
        {
            fileRes5.FileStream.Dispose();
        }

        // F. Scenario 6: SuperAdmin User (User 888 with SuperAdmin role) downloading Draft Project Document
        SetControllerUser(controller, userId: 888, role: AppRoles.SuperAdmin);
        var res6 = await controller.DownloadDocument(200, 502, CancellationToken.None);
        AssertTest(
            "SuperAdmin user downloading Draft project document => 200 OK (FileStreamResult)",
            res6 is FileStreamResult);
        if (res6 is FileStreamResult fileRes6)
        {
            fileRes6.FileStream.Dispose();
        }
    }

    private static async Task TestMismatchedAndMissingFileSafetyAsync()
    {
        Console.WriteLine("\n--- 4. MISMATCHED DOCUMENT ID & MISSING FILE SAFETY ---");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options, new DeUygulamaVitrini.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor());

        var pubProject = new Project
        {
            Id = 300,
            Name = "Enerji İzleme Portali",
            Slug = "enerji-izleme-portali",
            ShortDescription = "Tesis enerji izleme",
            StatusId = 1,
            CategoryId = 1,
            IsPublished = true,
            ApprovalStatus = ProjectApprovalStatus.Approved,
            CreatedByUserId = 1
        };

        var doc1 = new ProjectDocument
        {
            Id = 601,
            ProjectId = 300,
            Name = "Mevcut Doküman",
            FileName = "enerji_dokuman.pdf",
            FileUrl = "/uploads/projects/300/documents/enerji_dokuman.pdf",
            DocumentType = "application/pdf"
        };

        context.Projects.Add(pubProject);
        context.ProjectDocuments.Add(doc1);
        await context.SaveChangesAsync();

        var env = new TestWebHostEnvironment
        {
            WebRootPath = Path.GetTempPath(),
            ContentRootPath = Path.GetTempPath()
        };

        var storageService = new LocalFileStorageService(env);
        var stubProjectService = new StubProjectService();
        var stubAiSummaryService = new StubAiSummaryService();

        var controller = new ProjectsController(
            stubProjectService,
            stubAiSummaryService,
            context,
            storageService);

        SetControllerUser(controller, userId: 1, role: "User");

        // A. Non-existent Project ID
        var res1 = await controller.DownloadDocument(9999, 601, CancellationToken.None);
        AssertTest(
            "Non-existent Project ID => 404 NotFound",
            res1 is NotFoundObjectResult);

        // B. Non-existent Document ID
        var res2 = await controller.DownloadDocument(300, 8888, CancellationToken.None);
        AssertTest(
            "Non-existent Document ID => 404 NotFound",
            res2 is NotFoundObjectResult);

        // C. Mismatched Document ID (Document exists, but belongs to another project)
        var otherProject = new Project
        {
            Id = 400,
            Name = "Başka Proje",
            Slug = "baska-proje",
            ShortDescription = "Başka bir proje",
            StatusId = 1,
            CategoryId = 1,
            IsPublished = true,
            ApprovalStatus = ProjectApprovalStatus.Approved
        };
        context.Projects.Add(otherProject);
        await context.SaveChangesAsync();

        var res3 = await controller.DownloadDocument(400, 601, CancellationToken.None);
        AssertTest(
            "Mismatched Document ID (Document 601 belongs to project 300, requested on 400) => 404 NotFound",
            res3 is NotFoundObjectResult);

        // D. Document record exists in DB, but physical file is missing from disk
        var res4 = await controller.DownloadDocument(300, 601, CancellationToken.None);
        AssertTest(
            "Physical file missing from storage root => 404 NotFound without stack trace leak",
            res4 is NotFoundObjectResult notFound && notFound.Value is ProblemDetails);
    }

    private static void SetControllerUser(ControllerBase controller, int userId, string role)
    {
        var claims = new List<Claim>();
        if (userId > 0)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, $"user_{userId}"));
        }
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = userId > 0 ? new ClaimsIdentity(claims, "TestCookieAuth") : new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
