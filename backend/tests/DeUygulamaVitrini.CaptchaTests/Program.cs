using System.Net;
using System.Text;
using System.Text.Json;
using DeUygulamaVitrini.API.Controllers;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Auth;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Infrastructure.Services.Captcha;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.CaptchaTests;

public class Program
{
    private static int _passCount = 0;
    private static int _failCount = 0;

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=======================================================================");
        Console.WriteLine("PHASE 21 â€” PROFESSIONAL FIRST-PARTY CAPTCHA CHALLENGE TEST RUNNER");
        Console.WriteLine("=======================================================================");

        // â”€â”€ 1. Code Generation & Alphabet Invariants â”€â”€
        TestCodeGeneration();

        // â”€â”€ 2. Visual Rendering & SVG Invariants â”€â”€
        TestVisualRendering();

        // â”€â”€ 3. Hash Verification & Timing-Attack Invariants â”€â”€
        TestHashVerification();

        // â”€â”€ 4. MemoryCaptchaChallengeStore Single-Use & Concurrency Tests â”€â”€
        await TestChallengeStoreAsync();

        // â”€â”€ 5. CaptchaService End-to-End Lifecycle & Replay Protection â”€â”€
        await TestCaptchaServiceLifecycleAsync();

        // â”€â”€ 6. AuthController Integration & Security Isolation â”€â”€
        await TestAuthControllerIntegrationAsync();

        // â”€â”€ 7. Disabled Configuration Behavior â”€â”€
        await TestCaptchaDisabledBehaviorAsync();

        Console.WriteLine("\n=======================================================================");
        Console.WriteLine($"TOTAL CAPTCHA TESTS: {_passCount + _failCount} | PASSED: {_passCount} | FAILED: {_failCount}");
        Console.WriteLine("=======================================================================");

        return _failCount == 0 ? 0 : 1;
    }

    private static void AssertTrue(string testName, bool condition, string detail = "")
    {
        if (condition)
        {
            _passCount++;
            Console.WriteLine($"  [PASS] {testName} {(string.IsNullOrWhiteSpace(detail) ? "" : $"â€” {detail}")}");
        }
        else
        {
            _failCount++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] {testName} â€” FAILED! {detail}");
            Console.ResetColor();
        }
    }

    private static void TestCodeGeneration()
    {
        Console.WriteLine("\n[1/7] CODE GENERATION & SAFE ALPHABET INVARIANTS:");

        // 1. Safe alphabet does not contain ambiguous characters (0, O, o, 1, I, l, 8, B)
        var ambiguous = new[] { '0', 'O', 'o', '1', 'I', 'l', '8', 'B' };
        var hasAmbiguous = ambiguous.Any(c => CaptchaGenerator.SafeAlphabet.Contains(c));
        AssertTrue("Alphabet Safety â€” Excludes confusing character pairs (0/O, 1/I/l, 8/B)",
            !hasAmbiguous, $"SafeAlphabet={CaptchaGenerator.SafeAlphabet}");

        // 2. Default length is 5
        var code5 = CaptchaGenerator.GenerateCode(5);
        AssertTrue("Code Length â€” Default length produces exactly 5 characters",
            code5.Length == 5, $"Generated='{code5}'");

        // 3. Custom length bounding
        var code8 = CaptchaGenerator.GenerateCode(8);
        AssertTrue("Code Length â€” Custom requested length (8) respected",
            code8.Length == 8, $"Generated='{code8}'");

        // 4. Randomness / non-repeating
        var codes = Enumerable.Range(0, 100).Select(_ => CaptchaGenerator.GenerateCode(5)).ToHashSet();
        AssertTrue("Randomness â€” 100 generated codes exhibit high entropy (at least 95 unique)",
            codes.Count >= 95, $"UniqueCount={codes.Count}/100");
    }

    private static void TestVisualRendering()
    {
        Console.WriteLine("\n[2/7] VISUAL RENDERING & SVG SECURITY INVARIANTS:");

        var code = "7K3PX";
        var svg = CaptchaVisualRenderer.RenderSvg(code);
        var dataUrl = CaptchaVisualRenderer.RenderAsDataUrl(code);

        AssertTrue("SVG Structure â€” Contains valid svg element with viewBox",
            svg.Contains("<svg") && svg.Contains("viewBox=\"0 0 140 48\""));

        AssertTrue("SVG Structure â€” Contains corporate background rectangle & gradients",
            svg.Contains("<rect") && svg.Contains("linearGradient"));

        AssertTrue("SVG Security â€” Contains noise dots and interference paths",
            svg.Contains("<circle") && svg.Contains("<path"));

        AssertTrue("SVG Content â€” Renders all individual code characters in svg text nodes",
            code.All(c => svg.Contains($">{c}<")));

        AssertTrue("Data URL â€” Formats correctly as base64 data URI (data:image/svg+xml;base64,...)",
            dataUrl.StartsWith("data:image/svg+xml;base64,") && dataUrl.Length > 200);

        // Verify base64 decode matches SVG
        var base64Part = dataUrl.Substring("data:image/svg+xml;base64,".Length);
        var decodedSvg = Encoding.UTF8.GetString(Convert.FromBase64String(base64Part));
        AssertTrue("Data URL Integrity â€” Decoded base64 contains valid SVG element with rendered characters",
            decodedSvg.StartsWith("<svg") && decodedSvg.EndsWith("</svg>") && code.All(c => decodedSvg.Contains($">{c}<")));
    }

    private static void TestHashVerification()
    {
        Console.WriteLine("\n[3/7] HASH VERIFICATION & NORMALIZATION INVARIANTS:");

        var challengeId = CaptchaGenerator.GenerateChallengeId();
        var code = "9J2MN";
        var hash = CaptchaGenerator.ComputeHash(code, challengeId, caseSensitive: false);

        AssertTrue("Hash Structure â€” Computes non-empty 64-character SHA256 hex digest",
            !string.IsNullOrWhiteSpace(hash) && hash.Length == 64);

        // Exact match
        AssertTrue("Verification â€” Exact match succeeds",
            CaptchaGenerator.VerifyHash("9J2MN", challengeId, hash, caseSensitive: false));

        // Lowercase input with case-insensitive option
        AssertTrue("Verification â€” Lowercase input succeeds when CaseSensitive=false",
            CaptchaGenerator.VerifyHash("9j2mn", challengeId, hash, caseSensitive: false));

        // Trimmed whitespace
        AssertTrue("Verification â€” Whitespace-padded input ('  9J2MN  ') is trimmed and succeeds",
            CaptchaGenerator.VerifyHash("  9J2MN  ", challengeId, hash, caseSensitive: false));

        // Incorrect code
        AssertTrue("Verification â€” Wrong code ('9J2MX') fails safely",
            !CaptchaGenerator.VerifyHash("9J2MX", challengeId, hash, caseSensitive: false));

        // Wrong challengeId
        var differentChallengeId = CaptchaGenerator.GenerateChallengeId();
        AssertTrue("Verification â€” Valid answer against wrong ChallengeId fails",
            !CaptchaGenerator.VerifyHash("9J2MN", differentChallengeId, hash, caseSensitive: false));

        // Empty / whitespace answer
        AssertTrue("Verification â€” Empty answer fails safely",
            !CaptchaGenerator.VerifyHash("", challengeId, hash, caseSensitive: false));
    }

    private static async Task TestChallengeStoreAsync()
    {
        Console.WriteLine("\n[4/7] MEMORY STORE SINGLE-USE & CONCURRENCY TESTS:");

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryCaptchaChallengeStore(memoryCache, NullLogger<MemoryCaptchaChallengeStore>.Instance);

        var challengeId = "test-store-id-1";
        var data = new CaptchaChallengeData
        {
            ChallengeId = challengeId,
            HashedAnswer = "TESTHASH",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(3)
        };

        // 1. Store
        await store.StoreChallengeAsync(challengeId, data, TimeSpan.FromMinutes(3));
        var retrieved = await store.GetChallengeAsync(challengeId);
        AssertTrue("Store Read â€” Stored challenge is readable before consumption",
            retrieved != null && retrieved.ChallengeId == challengeId);

        // 2. Atomic consume
        var consumed = await store.ConsumeChallengeAsync(challengeId);
        AssertTrue("Store Consume â€” First consumption returns data and flags IsConsumed=true",
            consumed != null && consumed.IsConsumed);

        // 3. Replay prevention (second consume returns null)
        var secondConsumed = await store.ConsumeChallengeAsync(challengeId);
        AssertTrue("Store Replay Defense â€” Second consumption of same ChallengeId returns null",
            secondConsumed == null);

        var afterGet = await store.GetChallengeAsync(challengeId);
        AssertTrue("Store Eviction â€” Consumed challenge is completely removed from cache",
            afterGet == null);

        // 4. Concurrent consumption race test (10 tasks race to consume 1 token)
        var raceId = "race-challenge-id";
        await store.StoreChallengeAsync(raceId, new CaptchaChallengeData
        {
            ChallengeId = raceId,
            HashedAnswer = "RACEHASH",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(3)
        }, TimeSpan.FromMinutes(3));

        var tasks = Enumerable.Range(0, 10).Select(_ => store.ConsumeChallengeAsync(raceId)).ToArray();
        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r != null);
        var nullCount = results.Count(r => r == null);

        AssertTrue("Store Concurrency â€” Exactly 1 consumer wins the race and 9 are rejected (10 concurrent requests)",
            successCount == 1 && nullCount == 9, $"Winners={successCount}, Losers={nullCount}");
    }

    private static async Task TestCaptchaServiceLifecycleAsync()
    {
        Console.WriteLine("\n[5/7] CAPTCHA SERVICE LIFECYCLE & REPLAY PROTECTION:");

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryCaptchaChallengeStore(memoryCache, NullLogger<MemoryCaptchaChallengeStore>.Instance);
        var options = Options.Create(new CaptchaOptions
        {
            Enabled = true,
            Length = 5,
            ExpirationMinutes = 3,
            CaseSensitive = false
        });
        var service = new CaptchaService(store, options, NullLogger<CaptchaService>.Instance);

        // 1. Create Challenge
        var challenge = await service.CreateChallengeAsync();
        AssertTrue("Lifecycle â€” ChallengeId is generated and non-empty",
            !string.IsNullOrWhiteSpace(challenge.ChallengeId));
        AssertTrue("Lifecycle â€” ImageDataUrl is generated and non-empty",
            !string.IsNullOrWhiteSpace(challenge.ImageDataUrl));
        AssertTrue("Lifecycle â€” ExpiresInSeconds is set to 180 (3 min)",
            challenge.ExpiresInSeconds == 180);

        // 2. Answer is NEVER exposed in public response DTO
        var json = JsonSerializer.Serialize(challenge);
        AssertTrue("Information Disclosure â€” Serialized JSON does NOT contain 'answer' or 'code' property",
            !json.Contains("\"answer\"", StringComparison.OrdinalIgnoreCase) &&
            !json.Contains("\"code\"", StringComparison.OrdinalIgnoreCase));

        // 3. Inspect internal hash in store to test successful answer
        var internalData = await store.GetChallengeAsync(challenge.ChallengeId);
        AssertTrue("Store State â€” Challenge is stored in store with valid expiration",
            internalData != null && internalData.ExpiresAt > DateTimeOffset.UtcNow);

        // 4. Missing inputs
        var missingRes1 = await service.ValidateChallengeAsync("", "ABCDE");
        AssertTrue("Validation Gate â€” Missing ChallengeId is rejected",
            !missingRes1.IsValid && missingRes1.FailureReason == "MissingInput");

        var missingRes2 = await service.ValidateChallengeAsync(challenge.ChallengeId, "");
        AssertTrue("Validation Gate â€” Missing Answer is rejected",
            !missingRes2.IsValid && missingRes2.FailureReason == "MissingInput");

        // 5. Wrong Answer Validation (Consumes token immediately to block repeated brute-force)
        var wrongRes = await service.ValidateChallengeAsync(challenge.ChallengeId, "WRONG");
        AssertTrue("Validation Gate â€” Incorrect answer is rejected with sanitized message",
            !wrongRes.IsValid && wrongRes.ErrorMessage != null);

        // 6. Token consumed on failure: Cannot retry against consumed challenge
        var retryRes = await service.ValidateChallengeAsync(challenge.ChallengeId, "WRONG");
        AssertTrue("Replay Defense â€” Attempt to retry against failed challenge is rejected as NotFoundOrConsumed",
            !retryRes.IsValid && retryRes.FailureReason == "NotFoundOrConsumed");

        // 7. Successful challenge validation test
        var customChallengeId = "known-test-id";
        var knownCode = "3K9MP";
        await store.StoreChallengeAsync(customChallengeId, new CaptchaChallengeData
        {
            ChallengeId = customChallengeId,
            HashedAnswer = CaptchaGenerator.ComputeHash(knownCode, customChallengeId, false),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(3)
        }, TimeSpan.FromMinutes(3));

        var validRes = await service.ValidateChallengeAsync(customChallengeId, "3k9mp"); // lowercase
        AssertTrue("Validation Gate â€” Correct answer (case-insensitive) succeeds",
            validRes.IsValid);

        // 8. Single-use: Validated challenge cannot be replayed
        var replayRes = await service.ValidateChallengeAsync(customChallengeId, "3k9mp");
        AssertTrue("Replay Defense â€” Successfully validated challenge CANNOT be replayed (returns false)",
            !replayRes.IsValid && replayRes.FailureReason == "NotFoundOrConsumed");

        // 9. Expiration test
        var expiredId = "expired-challenge-id";
        await store.StoreChallengeAsync(expiredId, new CaptchaChallengeData
        {
            ChallengeId = expiredId,
            HashedAnswer = CaptchaGenerator.ComputeHash("7M4NP", expiredId, false),
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-10) // already expired
        }, TimeSpan.FromSeconds(1));

        var expiredRes = await service.ValidateChallengeAsync(expiredId, "7M4NP");
        AssertTrue("Expiration Gate â€” Expired challenge is rejected safely",
            !expiredRes.IsValid);
    }

    private static async Task TestAuthControllerIntegrationAsync()
    {
        Console.WriteLine("\n[6/7] AUTHCONTROLLER INTEGRATION & SECURITY ISOLATION:");

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryCaptchaChallengeStore(memoryCache, NullLogger<MemoryCaptchaChallengeStore>.Instance);
        var options = Options.Create(new CaptchaOptions
        {
            Enabled = true,
            Length = 5,
            ExpirationMinutes = 3,
            CaseSensitive = false
        });
        var captchaService = new CaptchaService(store, options, NullLogger<CaptchaService>.Instance);

        // Mock UserManager / SignInManager
        var userStore = new MockUserStore();
        var userManager = new MockUserManager(userStore);
        var signInManager = new MockSignInManager(userManager);

        var controller = new AuthController(userManager, signInManager, captchaService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // 1. GET /api/auth/captcha
        var getCaptchaRes = await controller.GetCaptcha(CancellationToken.None);
        var okResult = getCaptchaRes.Result as OkObjectResult;
        AssertTrue("API Contract â€” GET /api/auth/captcha returns 200 OK with CaptchaChallengeResponseDto",
            okResult != null && okResult.Value is CaptchaChallengeResponseDto);

        var challengeDto = (CaptchaChallengeResponseDto)okResult!.Value!;
        AssertTrue("API Contract â€” Response contains ChallengeId and ImageDataUrl without plaintext answer",
            !string.IsNullOrWhiteSpace(challengeDto.ChallengeId) && challengeDto.ImageDataUrl.StartsWith("data:image/svg+xml;base64,"));

        // 2. POST /api/auth/login without CAPTCHA
        var loginMissingCaptcha = await controller.Login(new LoginRequestDto
        {
            Email = "admin@demirexport.com",
            Password = "Password123!",
            CaptchaChallengeId = null,
            CaptchaAnswer = null
        }, CancellationToken.None);

        var badReq1 = loginMissingCaptcha.Result as BadRequestObjectResult;
        AssertTrue("Login Gate â€” Login without CAPTCHA when Enabled=true returns 400 BadRequest with ProblemDetails",
            badReq1 != null && badReq1.Value is ProblemDetails && ((ProblemDetails)badReq1.Value).Detail == "DoÄŸrulama kodu gereklidir.");

        // 3. POST /api/auth/login with wrong CAPTCHA
        var loginWrongCaptcha = await controller.Login(new LoginRequestDto
        {
            Email = "admin@demirexport.com",
            Password = "Password123!",
            CaptchaChallengeId = challengeDto.ChallengeId,
            CaptchaAnswer = "WRONG"
        }, CancellationToken.None);

        var badReq2 = loginWrongCaptcha.Result as BadRequestObjectResult;
        AssertTrue("Login Gate â€” Login with wrong CAPTCHA returns 400 BadRequest and does NOT touch password authentication",
            badReq2 != null && ((ProblemDetails)badReq2.Value!).Title == "DoÄŸrulama HatasÄ±" && signInManager.PasswordSignInCallCount == 0);

        // 4. POST /api/auth/login with valid CAPTCHA but invalid password
        var freshChallengeId = "controller-valid-cap";
        var freshCode = "8P2LM";
        await store.StoreChallengeAsync(freshChallengeId, new CaptchaChallengeData
        {
            ChallengeId = freshChallengeId,
            HashedAnswer = CaptchaGenerator.ComputeHash(freshCode, freshChallengeId, false),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(3)
        }, TimeSpan.FromMinutes(3));

        var loginValidCapWrongPass = await controller.Login(new LoginRequestDto
        {
            Email = "admin@demirexport.com",
            Password = "WrongPassword!",
            CaptchaChallengeId = freshChallengeId,
            CaptchaAnswer = freshCode
        }, CancellationToken.None);

        var badReq3 = loginValidCapWrongPass.Result as BadRequestObjectResult;
        AssertTrue("Credential Protection â€” Valid CAPTCHA proceeds to Identity; wrong credentials return generic failure without revealing email existence",
            badReq3 != null && ((ProblemDetails)badReq3.Value!).Detail == "E-posta adresi veya parola hatalÄ±." && signInManager.PasswordSignInCallCount == 1);

        // 5. POST /api/auth/login with valid CAPTCHA and valid password
        var successChallengeId = "controller-success-cap";
        var successCode = "4X7NQ";
        await store.StoreChallengeAsync(successChallengeId, new CaptchaChallengeData
        {
            ChallengeId = successChallengeId,
            HashedAnswer = CaptchaGenerator.ComputeHash(successCode, successChallengeId, false),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(3)
        }, TimeSpan.FromMinutes(3));

        signInManager.ShouldSucceed = true;
        var loginSuccess = await controller.Login(new LoginRequestDto
        {
            Email = "admin@demirexport.com",
            Password = "CorrectPassword123!",
            CaptchaChallengeId = successChallengeId,
            CaptchaAnswer = successCode
        }, CancellationToken.None);

        var okLogin = loginSuccess.Result as OkObjectResult;
        AssertTrue("Login Success â€” Valid CAPTCHA + valid credentials returns 200 OK AuthUserDto",
            okLogin != null && okLogin.Value is AuthUserDto && ((AuthUserDto)okLogin.Value).Email == "admin@demirexport.com");
    }

    private static async Task TestCaptchaDisabledBehaviorAsync()
    {
        Console.WriteLine("\n[7/7] CAPTCHA DISABLED CONFIGURATION BEHAVIOR:");

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryCaptchaChallengeStore(memoryCache, NullLogger<MemoryCaptchaChallengeStore>.Instance);
        var options = Options.Create(new CaptchaOptions
        {
            Enabled = false // Disabled for corporate SSO or testing
        });
        var captchaService = new CaptchaService(store, options, NullLogger<CaptchaService>.Instance);

        var userStore = new MockUserStore();
        var userManager = new MockUserManager(userStore);
        var signInManager = new MockSignInManager(userManager) { ShouldSucceed = true };

        var controller = new AuthController(userManager, signInManager, captchaService)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // When disabled, login without CAPTCHA challenge or answer succeeds
        var loginRes = await controller.Login(new LoginRequestDto
        {
            Email = "admin@demirexport.com",
            Password = "CorrectPassword123!"
        }, CancellationToken.None);

        var okLogin = loginRes.Result as OkObjectResult;
        AssertTrue("Configurable Bypass â€” When Captcha:Enabled=false, login proceeds directly without requiring CAPTCHA fields",
            okLogin != null && okLogin.Value is AuthUserDto);
    }
}

// â”€â”€ Mock Objects for Unit Testing Controller â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public class MockUserStore : IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
{
    private readonly ApplicationUser _user = new()
    {
        Id = 1,
        UserName = "admin@demirexport.com",
        Email = "admin@demirexport.com",
        FirstName = "Demir",
        LastName = "Admin",
        IsActive = true,
        CanCreateProjects = true
    };

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName?.ToUpperInvariant());
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(_user);
    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(_user);
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(_user);
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email?.ToUpperInvariant());
    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken) => Task.CompletedTask;
    public void Dispose() { }
}

public class MockUserManager : UserManager<ApplicationUser>
{
    public MockUserManager(IUserStore<ApplicationUser> store)
        : base(store, null!, null!, null!, null!, null!, null!, null!, null!)
    {
    }

    public override Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        if (email.Equals("admin@demirexport.com", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ApplicationUser?>(new ApplicationUser
            {
                Id = 1,
                UserName = "admin@demirexport.com",
                Email = "admin@demirexport.com",
                FirstName = "Demir",
                LastName = "Admin",
                IsActive = true,
                CanCreateProjects = true
            });
        }
        return Task.FromResult<ApplicationUser?>(null);
    }

    public override Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return Task.FromResult<IList<string>>(new List<string> { "Admin", "SuperAdmin" });
    }
}

public class MockSignInManager : SignInManager<ApplicationUser>
{
    public int PasswordSignInCallCount { get; private set; }
    public bool ShouldSucceed { get; set; } = false;

    public MockSignInManager(UserManager<ApplicationUser> userManager)
        : base(userManager, new HttpContextAccessor(), new MockClaimsFactory(), null!, null!, null!, null!)
    {
    }

    public override Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        PasswordSignInCallCount++;
        if (ShouldSucceed && password == "CorrectPassword123!")
        {
            return Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success);
        }
        return Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed);
    }

    public override Task SignOutAsync() => Task.CompletedTask;
}

public class MockClaimsFactory : IUserClaimsPrincipalFactory<ApplicationUser>
{
    public Task<System.Security.Claims.ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var identity = new System.Security.Claims.ClaimsIdentity();
        return Task.FromResult(new System.Security.Claims.ClaimsPrincipal(identity));
    }
}
