using ExecutiveBriefing.Domain.Aggregates;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Domain.Repositories
{
    public interface IBriefingRepository
    {
        Task SaveAsync(Briefing briefing, CancellationToken cancellationToken = default);
        Task<Briefing?> GetByIdAsync(BriefingId id, CancellationToken cancellationToken = default);
    }
}
