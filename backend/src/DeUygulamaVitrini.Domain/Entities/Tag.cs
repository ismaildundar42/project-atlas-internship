using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Arama ve keşif için kullanılan serbest etiket.
/// Slug alanı URL-dostu ve unique olup arama filtrelerinde kullanılacaktır.
///
/// Örnekler: yapay-zeka, maden-sahasi, iot, goruntu-isleme
/// </summary>
public class Tag : BaseEntity
{
    public required string Name { get; set; }

    /// <summary>URL-dostu benzersiz tanımlayıcı. (ör. "yapay-zeka")</summary>
    public required string Slug { get; set; }

    // Navigation
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
