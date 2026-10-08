using DeUygulamaVitrini.Application.DTOs.Profile;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface IProfileService
{
    Task<UserProfileResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
}
