using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.ApplicationServices.Interfaces
{
    public interface IAIService
    {
        Task<List<BriefingSection>> GenerateBriefingSectionsAsync(CompanyName companyName, string? market, IReadOnlyCollection<SourceMaterial> sources, CancellationToken cancellationToken = default);
        Task<(string? WebsiteUrl, string? IrPageUrl)> DiscoverCompanyUrlsAsync(CompanyName companyName, string? market, CancellationToken cancellationToken = default);
    }
}

