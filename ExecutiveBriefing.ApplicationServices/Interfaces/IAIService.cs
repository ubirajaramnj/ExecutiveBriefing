using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.ApplicationServices.Interfaces
{
    public interface IAIService
    {
        Task<List<BriefingSection>> GenerateBriefingSectionsAsync(CompanyName companyName, string? market, IReadOnlyCollection<SourceMaterial> sources, CancellationToken cancellationToken = default);
    }
}
