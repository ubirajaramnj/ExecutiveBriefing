using System.Collections.Concurrent;
using ExecutiveBriefing.Domain.Aggregates;
using ExecutiveBriefing.Domain.Repositories;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Infrastructure.Repositories
{
    public class InMemoryBriefingRepository : IBriefingRepository
    {
        private static readonly ConcurrentDictionary<Guid, Briefing> _briefings = new();

        public Task SaveAsync(Briefing briefing, CancellationToken cancellationToken = default)
        {
            _briefings[briefing.Id.Value] = briefing;
            return Task.CompletedTask;
        }

        public Task<Briefing?> GetByIdAsync(BriefingId id, CancellationToken cancellationToken = default)
        {
            _briefings.TryGetValue(id.Value, out var briefing);
            return Task.FromResult(briefing);
        }
    }
}
