namespace ExecutiveBriefing.Domain.ValueObjects
{
    public record CompanyName
    {
        public string Value { get; init; }

        public CompanyName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Company name cannot be empty.", nameof(value));
            }
            if (value.Length > 150)
            {
                throw new ArgumentException("Company name cannot exceed 150 characters.", nameof(value));
            }
            Value = value;
        }
    }
}
