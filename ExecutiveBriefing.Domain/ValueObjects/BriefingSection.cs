namespace ExecutiveBriefing.Domain.ValueObjects
{
    public record BriefingSection
    {
        public string Title { get; init; }
        public string Content { get; init; }
        public int DisplayOrder { get; init; }

        private BriefingSection(string title, string content, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Section title cannot be empty.", nameof(title));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Section content cannot be empty.", nameof(content));
            }
            Title = title;
            Content = content;
            DisplayOrder = displayOrder;
        }

        public static BriefingSection Create(string title, string content, int displayOrder)
        {
            return new BriefingSection(title, content, displayOrder);
        }
    }
}
