using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Admin;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

public class AdminOrganizationService : IAdminOrganizationService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public AdminOrganizationService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    // ─── DEPARTMENTS ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DepartmentAdminDto>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentAdminDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                TeamCount = d.Teams.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateDepartmentAsync(CreateDepartmentRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Departman adı zorunludur.");

        var exists = await _context.Departments.AnyAsync(d => d.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' adında bir departman zaten mevcut.");

        var dept = new Department
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync(cancellationToken);
        return dept.Id;
    }

    public async Task UpdateDepartmentAsync(int id, UpdateDepartmentRequestDto request, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (dept == null)
            throw new KeyNotFoundException("Departman bulunamadı.");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Departman adı zorunludur.");

        var exists = await _context.Departments.AnyAsync(d => d.Id != id && d.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' adında başka bir departman zaten mevcut.");

        dept.Name = name;
        dept.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments.FindAsync(new object[] { id }, cancellationToken);
        if (dept == null)
            throw new KeyNotFoundException("Departman bulunamadı.");

        var hasTeams = await _context.Teams.AnyAsync(t => t.DepartmentId == id, cancellationToken);
        if (hasTeams)
        {
            throw new InvalidOperationException("Bu departman altında tanımlı ekipler bulunduğu için silinemez. Lütfen önce ekipleri başka bir departmana taşıyın veya silin.");
        }

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ─── TEAMS ───────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<TeamAdminDto>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .AsNoTracking()
            .OrderBy(t => t.Department != null ? t.Department.Name : t.Name)
            .ThenBy(t => t.Name)
            .Select(t => new TeamAdminDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department != null ? t.Department.Name : null,
                ProjectCount = t.ProjectTeams.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateTeamAsync(CreateTeamRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ekip adı zorunludur.");

        var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (!deptExists)
            throw new ArgumentException("Geçersiz departman.");

        var exists = await _context.Teams.AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' adında bir ekip zaten mevcut.");

        var team = new Team
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DepartmentId = request.DepartmentId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);
        return team.Id;
    }

    public async Task UpdateTeamAsync(int id, UpdateTeamRequestDto request, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FindAsync(new object[] { id }, cancellationToken);
        if (team == null)
            throw new KeyNotFoundException("Ekip bulunamadı.");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ekip adı zorunludur.");

        var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (!deptExists)
            throw new ArgumentException("Geçersiz departman.");

        var exists = await _context.Teams.AnyAsync(t => t.Id != id && t.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' adında başka bir ekip zaten mevcut.");

        team.Name = name;
        team.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        team.DepartmentId = request.DepartmentId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeamAsync(int id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FindAsync(new object[] { id }, cancellationToken);
        if (team == null)
            throw new KeyNotFoundException("Ekip bulunamadı.");

        var hasProjects = await _context.ProjectTeams.AnyAsync(pt => pt.TeamId == id, cancellationToken);
        if (hasProjects)
        {
            throw new InvalidOperationException("Bu ekip mevcut projelerde görev aldığı için silinemez.");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ─── MEMBERS ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        var members = await _context.Members
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .Include(m => m.ProjectMembers)
            .OrderBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .ToListAsync(cancellationToken);

        // Boşta duran, hiçbir Member'a bağlanmamış ApplicationUser'ları tespit et
        var linkedUserIds = members.Where(m => m.UserId.HasValue).Select(m => m.UserId!.Value).ToHashSet();
        var unlinkedUsers = await _userManager.Users
            .AsNoTracking()
            .Where(u => !linkedUserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var unlinkedUserEmailMap = unlinkedUsers
            .Where(u => !string.IsNullOrWhiteSpace(u.Email))
            .GroupBy(u => u.Email!.Trim().ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        var result = new List<MemberAdminDto>();

        foreach (var m in members)
        {
            var hasAccount = m.User != null;
            var normalizedMemberEmail = m.Email?.Trim().ToLower();

            var matchingUnlinkedUser = (!hasAccount && !string.IsNullOrEmpty(normalizedMemberEmail) && unlinkedUserEmailMap.TryGetValue(normalizedMemberEmail, out var matched))
                ? matched
                : null;

            bool isSuperAdmin = false;
            bool isAdmin = false;

            if (m.User != null)
            {
                var roles = await _userManager.GetRolesAsync(m.User);
                isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
                isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);
            }

            result.Add(new MemberAdminDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Title = m.Title,
                Email = m.Email,
                TeamId = m.TeamId,
                TeamName = m.Team?.Name,
                DepartmentId = m.Team?.DepartmentId,
                DepartmentName = m.Team?.Department?.Name,
                ProjectCount = m.ProjectMembers.Count,

                HasApplicationAccount = hasAccount,
                ApplicationUserId = m.User?.Id,
                ApplicationUserEmail = m.User?.Email,
                ApplicationUserIsActive = m.User?.IsActive,
                CanCreateProjects = m.User?.CanCreateProjects ?? false,
                IsAdmin = isAdmin,
                IsSuperAdmin = isSuperAdmin,
                HasMatchingUnlinkedAccount = matchingUnlinkedUser != null,
                MatchingUnlinkedUserId = matchingUnlinkedUser?.Id
            });
        }

        return result;
    }

    public async Task<int> CreateMemberAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Ad ve soyad zorunludur.");

        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams.FindAsync(new object[] { request.TeamId.Value }, cancellationToken);
            if (team == null)
                throw new ArgumentException("Seçilen ekip bulunamadı.");

            if (request.DepartmentId.HasValue && team.DepartmentId != request.DepartmentId.Value)
                throw new ArgumentException("Seçilen ekip, belirtilen departmana ait değildir.");
        }

        var member = new Member
        {
            FirstName = firstName,
            LastName = lastName,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            TeamId = request.TeamId
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return member.Id;
    }

    public async Task UpdateMemberAsync(int id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members.FindAsync(new object[] { id }, cancellationToken);
        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Ad ve soyad zorunludur.");

        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams.FindAsync(new object[] { request.TeamId.Value }, cancellationToken);
            if (team == null)
                throw new ArgumentException("Seçilen ekip bulunamadı.");

            if (request.DepartmentId.HasValue && team.DepartmentId != request.DepartmentId.Value)
                throw new ArgumentException("Seçilen ekip, belirtilen departmana ait değildir.");
        }

        member.FirstName = firstName;
        member.LastName = lastName;
        member.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        member.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        member.TeamId = request.TeamId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMemberAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members.FindAsync(new object[] { id }, cancellationToken);
        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        if (member.UserId.HasValue)
        {
            throw new InvalidOperationException("Bu kişiye bağlı bir uygulama giriş hesabı bulunmaktadır. Güvenlik gereği, aktif hesabı olan kişiler doğrudan silinemez.");
        }

        var hasProjects = await _context.ProjectMembers.AnyAsync(pm => pm.MemberId == id, cancellationToken);
        if (hasProjects)
        {
            throw new InvalidOperationException("Bu kişi mevcut projelerde görev aldığı için silinemez.");
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ─── MEMBER PROJECT ACCESS & ACCOUNT MANAGEMENT (Phase 13.2 & 19.7) ───────────

    public async Task<MemberAdminDto> UpdateMemberProjectAccessAsync(int memberId, UpdateMemberProjectAccessRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        if (!member.UserId.HasValue || member.User == null)
            throw new InvalidOperationException("Bu kişiye ait bir uygulama hesabı bulunmuyor. Yetki vermeden önce bir hesap oluşturmalı veya bağlamalısınız.");

        var user = await _userManager.FindByIdAsync(member.UserId.Value.ToString());
        if (user == null)
            throw new InvalidOperationException("Bağlı kullanıcı hesabı sistemde bulunamadı.");

        user.CanCreateProjects = request.CanCreateProjects;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Yetki güncellenemedi: {errors}");
        }

        var memberFullName = $"{member.FirstName} {member.LastName}".Trim();
        var action = request.CanCreateProjects ? "ProjectAccessGranted" : "ProjectAccessRevoked";
        var description = request.CanCreateProjects
            ? $"'{memberFullName}' kullanıcısına proje oluşturma yetkisi verildi."
            : $"'{memberFullName}' kullanıcısının proje oluşturma yetkisi kaldırıldı.";

        await _auditLogService.LogAsync(action, "UserAccess", user.Id.ToString(), memberFullName, description, cancellationToken: cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        return new MemberAdminDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Title = member.Title,
            Email = member.Email,
            TeamId = member.TeamId,
            TeamName = member.Team?.Name,
            DepartmentId = member.Team?.DepartmentId,
            DepartmentName = member.Team?.Department?.Name,
            ProjectCount = await _context.ProjectMembers.CountAsync(pm => pm.MemberId == member.Id, cancellationToken),
            HasApplicationAccount = true,
            ApplicationUserId = user.Id,
            ApplicationUserEmail = user.Email,
            ApplicationUserIsActive = user.IsActive,
            CanCreateProjects = user.CanCreateProjects,
            IsAdmin = isAdmin,
            IsSuperAdmin = isSuperAdmin
        };
    }

    public async Task<MemberAdminDto> UpdateMemberAdminRoleAsync(
        int memberId,
        UpdateMemberAdminRoleRequestDto request,
        int actingUserId,
        CancellationToken cancellationToken = default)
    {
        var actingUser = await _userManager.FindByIdAsync(actingUserId.ToString());
        if (actingUser == null || !await _userManager.IsInRoleAsync(actingUser, AppRoles.SuperAdmin))
        {
            throw new UnauthorizedAccessException("Yalnızca Süper Yöneticiler (SuperAdmin) Admin rolünü atayabilir veya kaldırabilir.");
        }

        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        if (!member.UserId.HasValue || member.User == null)
            throw new InvalidOperationException("Bu kişiye ait bir uygulama hesabı bulunmuyor.");

        var targetUser = await _userManager.FindByIdAsync(member.UserId.Value.ToString());
        if (targetUser == null)
            throw new InvalidOperationException("Bağlı kullanıcı hesabı bulunamadı.");

        if (await _userManager.IsInRoleAsync(targetUser, AppRoles.SuperAdmin))
        {
            throw new InvalidOperationException("Süper Yönetici hesabı bu yöntemle değiştirilemez.");
        }

        var memberFullName = $"{member.FirstName} {member.LastName}".Trim();

        if (request.IsAdmin)
        {
            if (!await _userManager.IsInRoleAsync(targetUser, AppRoles.Admin))
            {
                var addResult = await _userManager.AddToRoleAsync(targetUser, AppRoles.Admin);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Admin rolü atanamadı: {errors}");
                }
            }

            await _auditLogService.LogAsync("AdminRoleGranted", "UserAccess", targetUser.Id.ToString(), memberFullName,
                $"'{memberFullName}' kullanıcısına Admin rolü atandı (İşlemi yapan: {actingUser.Email}).", cancellationToken: cancellationToken);
        }
        else
        {
            if (await _userManager.IsInRoleAsync(targetUser, AppRoles.Admin))
            {
                var remResult = await _userManager.RemoveFromRoleAsync(targetUser, AppRoles.Admin);
                if (!remResult.Succeeded)
                {
                    var errors = string.Join(", ", remResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Admin rolü kaldırılamadı: {errors}");
                }
            }

            await _auditLogService.LogAsync("AdminRoleRevoked", "UserAccess", targetUser.Id.ToString(), memberFullName,
                $"'{memberFullName}' kullanıcısından Admin rolü kaldırıldı (İşlemi yapan: {actingUser.Email}).", cancellationToken: cancellationToken);
        }

        var roles = await _userManager.GetRolesAsync(targetUser);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        return new MemberAdminDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Title = member.Title,
            Email = member.Email,
            TeamId = member.TeamId,
            TeamName = member.Team?.Name,
            DepartmentId = member.Team?.DepartmentId,
            DepartmentName = member.Team?.Department?.Name,
            ProjectCount = await _context.ProjectMembers.CountAsync(pm => pm.MemberId == member.Id, cancellationToken),
            HasApplicationAccount = true,
            ApplicationUserId = targetUser.Id,
            ApplicationUserEmail = targetUser.Email,
            ApplicationUserIsActive = targetUser.IsActive,
            CanCreateProjects = targetUser.CanCreateProjects,
            IsAdmin = isAdmin,
            IsSuperAdmin = isSuperAdmin
        };
    }

    public async Task<MemberAdminDto> CreateMemberAccountAsync(int memberId, CreateMemberAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        if (member.UserId.HasValue)
            throw new InvalidOperationException("Bu kişinin zaten bağlı bir uygulama hesabı bulunmaktadır.");

        if (string.IsNullOrWhiteSpace(member.Email))
            throw new InvalidOperationException("Uygulama hesabı oluşturmak için kişinin geçerli bir e-posta adresi olmalıdır.");

        var email = member.Email.Trim();

        // Sistemde bu e-posta ile hesap var mı kontrol et
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"'{email}' e-posta adresiyle sistemde zaten bir hesap bulunmaktadır. Yeni hesap açmak yerine mevcut hesabı bağlayabilirsiniz.");
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = member.FirstName,
            LastName = member.LastName,
            EmailConfirmed = true,
            IsActive = true,
            CanCreateProjects = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(newUser, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new ArgumentException($"Hesap oluşturulamadı: {errors}");
        }

        // Kişiyi yeni açılan hesaba bağla
        member.UserId = newUser.Id;
        await _context.SaveChangesAsync(cancellationToken);

        var memberFullName = $"{member.FirstName} {member.LastName}".Trim();
        await _auditLogService.LogAsync("MemberAccountCreated", "Member", member.Id.ToString(), memberFullName, $"'{memberFullName}' kişisi için yeni uygulama hesabı ({email}) oluşturuldu.", cancellationToken: cancellationToken);

        return new MemberAdminDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Title = member.Title,
            Email = member.Email,
            TeamId = member.TeamId,
            TeamName = member.Team?.Name,
            DepartmentId = member.Team?.DepartmentId,
            DepartmentName = member.Team?.Department?.Name,
            ProjectCount = await _context.ProjectMembers.CountAsync(pm => pm.MemberId == member.Id, cancellationToken),
            HasApplicationAccount = true,
            ApplicationUserId = newUser.Id,
            ApplicationUserEmail = newUser.Email,
            ApplicationUserIsActive = newUser.IsActive,
            CanCreateProjects = newUser.CanCreateProjects,
            IsAdmin = false,
            IsSuperAdmin = false
        };
    }

    public async Task<MemberAdminDto> LinkMemberAccountAsync(int memberId, LinkMemberAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.User)
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException("Kişi bulunamadı.");

        if (member.UserId.HasValue)
            throw new InvalidOperationException("Bu kişinin zaten bağlı bir uygulama hesabı bulunmaktadır.");

        ApplicationUser? targetUser = null;

        if (request.UserId.HasValue)
        {
            targetUser = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
            if (targetUser == null)
                throw new KeyNotFoundException("Bağlanacak kullanıcı hesabı bulunamadı.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(member.Email))
                throw new InvalidOperationException("E-posta adresiyle eşleştirme yapılamadı çünkü kişinin e-posta adresi tanımlı değil.");

            targetUser = await _userManager.FindByEmailAsync(member.Email.Trim());
            if (targetUser == null)
                throw new KeyNotFoundException($"'{member.Email}' e-posta adresine sahip bir kullanıcı hesabı bulunamadı.");
        }

        // Bu kullanıcı hesabının başka bir üyeye bağlı olup olmadığını kontrol et
        var alreadyLinkedMember = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == targetUser.Id && m.Id != member.Id, cancellationToken);

        if (alreadyLinkedMember != null)
        {
            throw new InvalidOperationException($"Bu kullanıcı hesabı zaten '{alreadyLinkedMember.FirstName} {alreadyLinkedMember.LastName}' kişisine bağlıdır.");
        }

        member.UserId = targetUser.Id;
        await _context.SaveChangesAsync(cancellationToken);

        var memberFullName = $"{member.FirstName} {member.LastName}".Trim();
        await _auditLogService.LogAsync("MemberAccountLinked", "Member", member.Id.ToString(), memberFullName, $"'{memberFullName}' kişisi mevcut uygulama hesabına ({targetUser.Email}) bağlandı.", cancellationToken: cancellationToken);

        var roles = await _userManager.GetRolesAsync(targetUser);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        return new MemberAdminDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Title = member.Title,
            Email = member.Email,
            TeamId = member.TeamId,
            TeamName = member.Team?.Name,
            DepartmentId = member.Team?.DepartmentId,
            DepartmentName = member.Team?.Department?.Name,
            ProjectCount = await _context.ProjectMembers.CountAsync(pm => pm.MemberId == member.Id, cancellationToken),
            HasApplicationAccount = true,
            ApplicationUserId = targetUser.Id,
            ApplicationUserEmail = targetUser.Email,
            ApplicationUserIsActive = targetUser.IsActive,
            CanCreateProjects = targetUser.CanCreateProjects,
            IsAdmin = isAdmin,
            IsSuperAdmin = isSuperAdmin
        };
    }
}

