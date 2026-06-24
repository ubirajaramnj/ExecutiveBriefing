using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Domain.Aggregates
{
    public class Briefing
    {
        public BriefingId Id { get; }
        public CompanyName CompanyName { get; }
        public string? Market { get; }
        public string? WebsiteUrl { get; }
        public string? IRPageUrl { get; }
        public DateTime CreatedAt { get; }

        private readonly List<BriefingSection> _sections = new();
        public IReadOnlyCollection<BriefingSection> Sections => _sections.AsReadOnly();

        private readonly List<SourceMaterial> _sources = new();
        public IReadOnlyCollection<SourceMaterial> Sources => _sources.AsReadOnly();

        private Briefing(BriefingId id, CompanyName companyName, string? market, string? websiteUrl, string? irPageUrl, DateTime createdAt)
        {
            Id = id;
            CompanyName = companyName;
            Market = market;
            WebsiteUrl = websiteUrl;
            IRPageUrl = irPageUrl;
            CreatedAt = createdAt;
        }

        public static Briefing Create(CompanyName companyName, string? market, string? websiteUrl, string? irPageUrl)
        {
            return new Briefing(
                new BriefingId(),
                companyName,
                market,
                websiteUrl,
                irPageUrl,
                DateTime.UtcNow
            );
        }

        public void AddSection(BriefingSection section)
        {
            if (_sections.Any(s => s.Title.Equals(section.Title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"A section with title '{section.Title}' already exists.");
            }
            _sections.Add(section);
        }

        public void AddSource(SourceMaterial source)
        {
            if (_sources.Any(s => s.ReferenceName.Equals(source.ReferenceName, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Source already added
            }
            _sources.Add(source);
        }
    }
}
