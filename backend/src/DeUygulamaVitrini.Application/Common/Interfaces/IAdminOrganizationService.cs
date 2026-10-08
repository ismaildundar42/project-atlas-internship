using DeUygulamaVitrini.Application.DTOs.Admin;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Yönetici Organizasyon (Ekip, Departman, Kişi) yönetimi servisi.
/// </summary>
public interface IAdminOrganizationService
{
    // Departments
    Task<IReadOnlyList<DepartmentAdminDto>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateDepartmentAsync(CreateDepartmentRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateDepartmentAsync(int id, UpdateDepartmentRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);

    // Teams
    Task<IReadOnlyList<TeamAdminDto>> GetTeamsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateTeamAsync(CreateTeamRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateTeamAsync(int id, UpdateTeamRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteTeamAsync(int id, CancellationToken cancellationToken = default);

    // Members
    Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(CancellationToken cancellationToken = default);
    Task<int> CreateMemberAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateMemberAsync(int id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteMemberAsync(int id, CancellationToken cancellationToken = default);

    // Member Identity Link & Project Access (Phase 13.2)
    Task<MemberAdminDto> UpdateMemberProjectAccessAsync(int memberId, UpdateMemberProjectAccessRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberAdminDto> CreateMemberAccountAsync(int memberId, CreateMemberAccountRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberAdminDto> LinkMemberAccountAsync(int memberId, LinkMemberAccountRequestDto request, CancellationToken cancellationToken = default);

    // Member Admin Role Assignment (Phase 19.7 - SuperAdmin Only)
    Task<MemberAdminDto> UpdateMemberAdminRoleAsync(int memberId, UpdateMemberAdminRoleRequestDto request, int actingUserId, CancellationToken cancellationToken = default);
}

