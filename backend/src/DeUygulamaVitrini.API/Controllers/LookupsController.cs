using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    /// <summary>
    /// Tüm proje durumlarını listeler.
    /// </summary>
    [HttpGet("project-statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectStatusDto>>> GetProjectStatuses(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetProjectStatusesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm proje kategorilerini listeler.
    /// </summary>
    [HttpGet("project-categories")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectCategoryDto>>> GetProjectCategories(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetProjectCategoriesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm teknolojileri listeler.
    /// </summary>
    [HttpGet("technologies")]
    [ProducesResponseType(typeof(IReadOnlyList<TechnologyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TechnologyDto>>> GetTechnologies(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetTechnologiesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm saha ve lokasyon bilgilerini listeler.
    /// </summary>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(IReadOnlyList<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocationDto>>> GetLocations(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetLocationsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm proje ekiplerini listeler (Lookup arayüzü).
    /// </summary>
    [HttpGet("lookups/teams")]
    [ProducesResponseType(typeof(IReadOnlyList<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetTeams(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetTeamsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm kişileri / üyeleri listeler.
    /// </summary>
    [HttpGet("members")]
    [ProducesResponseType(typeof(IReadOnlyList<MemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> GetMembers(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetMembersAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm etiketleri listeler.
    /// </summary>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetTags(CancellationToken cancellationToken)
    {
        var result = await _lookupService.GetTagsAsync(cancellationToken);
        return Ok(result);
    }
}
