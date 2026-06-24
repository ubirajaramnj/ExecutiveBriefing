namespace ExecutiveBriefing.Domain.ValueObjects
{
    public record BriefingId
    {
        public Guid Value { get; init; }

        public BriefingId()
        {
            Value = Guid.NewGuid();
        }

        public BriefingId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Briefing ID cannot be empty Guid.", nameof(value));
            }
            Value = value;
        }
    }
}
