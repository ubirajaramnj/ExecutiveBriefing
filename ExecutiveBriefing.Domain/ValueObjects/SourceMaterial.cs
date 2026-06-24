namespace ExecutiveBriefing.Domain.ValueObjects
{
    public enum SourceType
    {
        Upload,
        WebPage
    }

    public record SourceMaterial
    {
        public SourceType Type { get; init; }
        public string ReferenceName { get; init; }
        public string Content { get; init; }

        private SourceMaterial(SourceType type, string referenceName, string content)
        {
            if (string.IsNullOrWhiteSpace(referenceName))
            {
                throw new ArgumentException("Reference name cannot be empty.", nameof(referenceName));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Source content cannot be empty.", nameof(content));
            }
            Type = type;
            ReferenceName = referenceName;
            Content = content;
        }

        public static SourceMaterial Create(SourceType type, string referenceName, string content)
        {
            return new SourceMaterial(type, referenceName, content);
        }
    }
}
